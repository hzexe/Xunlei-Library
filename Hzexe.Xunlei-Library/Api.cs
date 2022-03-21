using BaristaLabs.Skrapr;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using BaristaLabs.Skrapr.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hzexe.Xunlei_Library
{

    public class Api : IApi
    {
        readonly Process ps;
        readonly int port;
        readonly IServiceProvider serviceProvider;
        private bool disposedValue;
        ChromeSessionInfo session;
        SkraprDevTools devTools;
        readonly Prot.SqlContext context;
        bool isWorking = false;


        // readonly string xunleiRootDir;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="thunderFullPath">迅雷主程序完整路径  比如:<example>C:\Program Files\Thunder\Program\Thunder.exe</example> </param>
        /// <param name="port">通讯端口</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public Api(IServiceProvider serviceProvider, string thunderFullPath, int port=64321)
        {
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
            this.port = port;
            this.serviceProvider = serviceProvider;
            if (thunderFullPath == null || !thunderFullPath.EndsWith("Thunder.exe") || !File.Exists(thunderFullPath))
                throw new ArgumentException("迅雷路径不存在");
            var xlDir = new FileInfo(thunderFullPath).Directory.Parent.FullName;
            string dbpath = Path.Combine(xlDir, "profiles", "TaskDb.dat");
            DbContextOptionsBuilder<Prot.SqlContext> b = new DbContextOptionsBuilder<Prot.SqlContext>();
            b.UseSqlite(@$"DataSource={dbpath};Mode=ReadOnly");
            context = new Prot.SqlContext(b.Options);
            //var q=context.TaskBases.ToList();

            var sessionid = Process.GetCurrentProcess().SessionId;
            foreach (var item in Process.GetProcesses())
            {
                if (item.SessionId != sessionid || !item.ProcessName.Contains("Thunder"))
                    continue;
                //if (item.StartInfo.FileName.Equals("Thunder") ||
                //    (item.StartInfo.FileName.Equals("wine") && (item.StartInfo.Arguments ?? "").Contains("Thunder.exe"))
                //    )
                //{
                item.Kill(true);
                break;
                // }
            }
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
            ps = Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = new FileInfo(thunderFullPath).DirectoryName,
                FileName = thunderFullPath,
                Arguments = $"--remote-debugging-port={port}",
                WindowStyle = ProcessWindowStyle.Minimized
            });
#pragma warning restore CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS8602 // 解引用可能出现空引用。
            ps.Exited += Ps_Exited;
#pragma warning restore CS8602 // 解引用可能出现空引用。

            ps.WaitForInputIdle(5000);

            var sock = new System.Net.Sockets.Socket(System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    sock.ConnectAsync(new IPEndPoint(IPAddress.Loopback, port)).Wait();
                    sock.Close();
                    break;
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
            sock.Dispose();
            Thread.Sleep(4000);
            session = GetFrames().FirstOrDefault(x => x.Title == "迅雷");
            if (session == null) throw new Exception("未找到主窗口");
            devTools = SkraprDevTools.Connect(serviceProvider, session).Result;
        }

        public async Task<long?> DownloadLinkAsync(string url)
        {
            if (isWorking) throw new Exception("下载器正忙，稍后再试");
            isWorking = true;
            await ClosePopWindow(false);
            //点击添加
            string js = @"(function(){$('button.td-button.td-button--secondary').click(); return true})()";
            var r = await RunCmdAsync(this.devTools, js);
            Thread.Sleep(500);
            var taskpage = await Task.Run(() =>
            {
                while (true)
                {
                    var s = GetFrames().FirstOrDefault(x => x.Title == "新建任务面板");
                    if (s != null)
                        return s;
                    Thread.Sleep(50);
                }
            });
            var tl = await SkraprDevTools.Connect(serviceProvider, taskpage);
            js = "(function(){document.querySelector('textarea').innerText='" + url + "';";
            js += @"
var dom = document.querySelector('textarea')
var evt = new UIEvent('input', {
    bubbles: false,
    cancelable: false
});
dom.dispatchEvent(evt);

return document.title;
})();
";

            r = await RunCmdAsync(tl, js);
            Thread.Sleep(1000);
            js = "$('div.xly-dialog-link__operate button').nextElementSibling.click()";   //确定按钮
            r = await RunCmdAsync(tl, js);

            js = @"(function(){return $('div[slot=\'footer\']')==null||$('div[slot=\'footer\']').className.indexOf('is-disabled')==-1})()";
            while ((r = await RunCmdAsync(tl, js)).Result.Value.Equals(false))
            {
                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
            //等待下载按钮出来

            js = @"(function(){
var p=Array.from(document.querySelectorAll('span.td-checkbox__label')).find(el => el.textContent === '电脑').previousSibling.previousSibling;
if(!p.checked)
{
p.checked=true;
var evt = new UIEvent('change', {
    bubbles: false,
    cancelable: false
});
p.dispatchEvent(evt);
}
console.log(p.parentElement);
})();

(
function(){
if(document.querySelectorAll('span.td-checkbox__label')).find(el => el.textContent === '云盘')==null) return;
var dom = Array.from(document.querySelectorAll('span.td-checkbox__label')).find(el => el.textContent === '云盘').previousSibling.previousSibling;
if(dom.checked)
{
dom.checked=false;
var evt = new UIEvent('change', {
    bubbles: false,
    cancelable: false
});
dom.dispatchEvent(evt);
}
console.log(dom.parentElement);
}
)();
";
            r = await RunCmdAsync(tl, js);
            Thread.Sleep(200);
            js = @"
(function(){
$('div[slot=\'footer\'] button').click();
})();

";
            r = await RunCmdAsync(tl, js);
            Thread.Sleep(2000);
            //查看重复任务是否有重复任务
            await ClosePopWindow(true);
            //查看是否有弹出窗口
            isWorking = false;
            tl.Dispose();

            return this.context.TaskBases.FirstOrDefault(x => x.Url == url+'\0')?.TaskId;
        }


        private async Task ClosePopWindow(bool yes)
        {
            var s = GetFrames().FirstOrDefault(x => x.Title == "消息提示框");
            if (null == s) return;
            using (var dev = await SkraprDevTools.Connect(serviceProvider, s))
            {
                var js = "(function(){return $('p.td-dialog-comfirm__title').innerText;})()";
                var r = await RunCmdAsync(dev, js);
                if (yes)
                    js = "$('button.td-button.td-button--secondary').click()";
                else
                    js = "$('a.td-dialog__close').close()";
                r = await RunCmdAsync(dev, js);
            }
        }


        private async Task<BaristaLabs.Skrapr.ChromeDevTools.Runtime.EvaluateCommandResponse> RunCmdAsync(SkraprDevTools dev, string js)
        {

            var evaluateResponse = await dev.Session.Runtime.Evaluate(new BaristaLabs.Skrapr.ChromeDevTools.Runtime.EvaluateCommand()
            {
                Expression = js,
                ContextId = 1,
                GeneratePreview = false,
                ObjectGroup = "console",
                IncludeCommandLineAPI = true,
                ReturnByValue = false,
                UserGesture = true,
                AwaitPromise = true,
                Silent = true
            });
            // if (evaluateResponse.ExceptionDetails != null)
            //    throw new JavaScriptException(evaluateResponse.ExceptionDetails);
            return evaluateResponse;
        }




        private ChromeSessionInfo[] GetFrames()
        {
            using (HttpClient client = new HttpClient())
            {
                var data = client.GetStringAsync($"http://{IPAddress.Loopback.ToString()}:{port}/json/list?t={DateTimeOffset.Now.ToUnixTimeSeconds()}").Result;
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ChromeSessionInfo[]>(data);
            }
        }

        private void Ps_Exited(object? sender, EventArgs e)
        {

        }


        public async Task<IEnumerable<TaskRecored>> GetAllTasksAsync()
        {
            return await Task.FromResult(context.TaskBases.AsEnumerable().Select(x => new TaskRecored(x)));
        }

        public async Task<TaskRecored?> GetTasksAsync(Int64 taskId)
        {
            return await Task.FromResult(context.TaskBases.Where(x => x.TaskId == taskId).Select(x => new TaskRecored(x)).FirstOrDefault());
        }

        public async Task<String?> GetLoginInfoAsync() {
            var js = @"(function(){
var obj=$('p.xly-aside-personal__user');
if(obj==null)
return null;
else
return obj.innerText;
})()";
            var r = await RunCmdAsync(this.devTools, js);
           return r.Result.Value as string;
        }

        public async Task LoginByPassword(string username, string password)
        {
            if (!string.IsNullOrEmpty(await GetLoginInfoAsync()))
            {
                throw new Exception("已有用户登录");
            }

            var js = @"$('div.td-avatar img').click()";
            var r = await RunCmdAsync(this.devTools, js);
            Thread.Sleep(2000);
            var taskpage = await Task.Run(() =>
            {
                while (true)
                {
                    var s = GetFrames().FirstOrDefault(x => x.Title == "迅雷个人中心-登录");
                    if (s != null)
                        return s;
                    Thread.Sleep(50);
                }
            });
            using (var tl = await SkraprDevTools.Connect(serviceProvider, taskpage))
            {
                js = @"
(function(w,uname,pwd){
w.document.getElementById('al_u').value=uname;
w.document.getElementById('al_p').value=pwd;
w.document.getElementById('al_remember').checked=false;
w.document.getElementById('al_submit').click();
        })(document.getElementById('loginIframe').contentWindow,'" + username + "','" + password + "');";
                r = await RunCmdAsync(tl, js);
                Thread.Sleep(2000);

                var ds = GetFrames().FirstOrDefault(x => x.Title == "迅雷个人中心-登录");
                if (null != ds)
                {
                    js = @"
(function(w){
return w.document.getElementById('al_warn').innerText;
        })(document.getElementById('loginIframe').contentWindow);";
                    r = await RunCmdAsync(tl, js);
                    string errordesc = "" + r.Result.Value as string;
                    //关闭
                    r = await RunCmdAsync(tl, "document.getElementById('loginIframe').contentWindow.document.getElementById('close').click()");
                    throw new Exception(errordesc);
                }
            }
        }

        public async Task<byte[]> LoginByEWMAsync()
        {
            if (!string.IsNullOrEmpty(await GetLoginInfoAsync()))
            {
                throw new Exception("已有用户登录");
            }

            var js = @"($'div.td-avatar img').click()";
            var r = await RunCmdAsync(this.devTools, js);
            Thread.Sleep(2000);
            var taskpage = await Task.Run(() =>
            {
                while (true)
                {
                    var s = GetFrames().FirstOrDefault(x => x.Title == "迅雷个人中心-登录");
                    if (s != null)
                        return s;
                    Thread.Sleep(50);
                }
            });
            Thread.Sleep(2000);
            using (var tl = await SkraprDevTools.Connect(serviceProvider, taskpage))
            {
                js = @"
(function(){
return document.getElementById('xlx-code-img').src;
        })();";
                r = await RunCmdAsync(tl, js);
               string bs= (r.Result.Value+"").Split(',').Last();
                r = await RunCmdAsync(tl, "document.getElementById('loginIframe').contentWindow.document.getElementById('close').click()");
                return Convert.FromBase64String(bs);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    devTools?.Dispose();
                    context?.Dispose();
                    // TODO: 释放托管状态(托管对象)
                    if (!ps.HasExited)
                    {
                        ps.Kill(true);
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~Api()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


    public enum DownloadStatus : int
    {
        Unknown0 = 0,
        Unknown1,
        Unknown2,
        Unknown3,
        Unknown4,
        Downloading = 5,
        Unknown6 = 6,
        Deleted = 7,
        Completed = 8
    }
}