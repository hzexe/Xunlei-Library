using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BaristaLabs.Skrapr;
using BaristaLabs.Skrapr.Extensions;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using Troschuetz.Random.Generators;
using System.Runtime.Loader;
using FreeSql;
using FreeSql.DataAnnotations;
using Hzexe.Xunlei_Library.Prot;

namespace Hzexe.Xunlei_Library
{

    public class Api : IApi
    {
        readonly Process ps;
        readonly int port;
        private bool disposedValue;
        ChromeSessionInfo session;
        SkraprDevTools devTools;
        bool isWorking = false;
        readonly ILogger log;

        // 并发控制锁对象
        private static readonly object _operationLock = new object();

        // FreeSQL实例
        readonly IFreeSql freeSql;

        // 临时服务提供者，用于SkraprDevTools
        IServiceProvider tempServiceProvider;

        // readonly string xunleiRootDir;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="freeSql">FreeSQL实例</param>
        /// <param name="thunderFullPath">迅雷主程序完整路径  比如:<example>C:\Program Files\Thunder\Program\Thunder.exe</example> </param>
        /// <param name="port">通讯端口</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public Api(IFreeSql freeSql, IConfiguration cfg, ILogger<Api> log, IServiceProvider tempServiceProvider)
        {
            this.log = log;
            this.freeSql = freeSql;
            this.tempServiceProvider = tempServiceProvider;
            // 需要添加 using Microsoft.Extensions.Configuration;
            string thunderFullPath = cfg.GetSection("XunleiPath").Value;
            this.port = 64321;
            if (thunderFullPath == null || !thunderFullPath.EndsWith("Thunder.exe") || !File.Exists(thunderFullPath))
                throw new ArgumentException("迅雷路径不存在");
            // 其余代码保持不变
            this.port = 64321;
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
            devTools = SkraprDevTools.Connect(tempServiceProvider, session).Result;
        }

        public async Task<long?> DownloadLinkAsync(string url)
        {
            // 使用锁确保下载操作的互斥性
            lock (_operationLock)
            {
                if (isWorking) throw new Exception("下载器正忙，稍后再试");
                isWorking = true;
            }

            try
            {
                await ClosePopWindow(false);
                log.LogInformation("开始下载链接: {Url}", url);
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
                log.LogInformation("新建任务面板已找到: {Title}", taskpage.Title);
                var tl = await SkraprDevTools.Connect(tempServiceProvider, taskpage);
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
                Thread.Sleep(500);
                log.LogInformation("链接已输入到新建任务面板: {Url}", url);

                js = "$('div.xly-dialog-link__operate button').nextElementSibling.click()";   //确定按钮
                r = await RunCmdAsync(tl, js);
                Thread.Sleep(500);
                log.LogInformation("开始判断任务类型");
                //如果是bt类的连接，此时原先的页面已被关闭了
                bool isbt = false;
                try
                {
                    //用来判断当前页面是否已关闭，如果已关闭，则抛出异常
                    await tl.Session.DOM.GetOuterHTML(new BaristaLabs.Skrapr.ChromeDevTools.DOM.GetOuterHTMLCommand(), throwExceptionIfResponseNotReceived: true);
                }
                catch (System.InvalidOperationException)
                {
                    isbt = true;
                }
                catch (System.Net.Sockets.SocketException)
                {
                    isbt = true;
                }
                catch (BaristaLabs.Skrapr.ChromeDevTools.CommandResponseException)
                {
                    isbt = false;
                }
                catch (Exception ex)
                {
                    log.LogError("{functionName} An unhandled error occurred while processing task: {exception}", nameof(DownloadLinkAsync), ex);
                    throw;
                }
                log.LogInformation("任务类型判断完成: {IsBt}", isbt);
                if (isbt)
                {
                    taskpage = await Task.Run(() =>
                    {
                        while (true)
                        {
                            var s = GetFrames().FirstOrDefault(x => x.Title == "新建任务面板");
                            if (s != null)
                                return s;
                            Thread.Sleep(50);
                        }
                    });
                    log.LogInformation("[bt任务]新建任务面板已找到: {Title}", taskpage.Title);
                    var tjs = """
(function(){
let dom= $x("//a[@class='xly-tag-tasks is-disabled'][contains(text(), '全选')]");
if(null==dom) return;
dom[0].click();
})()
""";
                    //tl.Dispose();
                    tl = await SkraprDevTools.Connect(tempServiceProvider, taskpage);
                    r = await RunCmdAsync(tl, tjs);
                    log.LogInformation("[bt任务]已连接到新的任务页面");

                }

                {
                    log.LogInformation("开始等待确认下载按钮");
                    //等待确认下载按钮出来
                    js = @"(function(){return $('div[slot=\'footer\']')==null||$('div[slot=\'footer\']').className.indexOf('is-disabled')==-1})()";
                    while ((r = await RunCmdAsync(tl, js)).Result.Value.Equals(false))
                    {
                        Thread.Sleep(500);
                    }
                    Thread.Sleep(100);
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
                    log.LogInformation("开始调整下载位置");

                    r = await RunCmdAsync(tl, js);
                    Thread.Sleep(200);
                    js = @"
(function(){
$('div[slot=\'footer\'] button').click();
})();

";
                    log.LogInformation("开始确认下载地址");
                    r = await RunCmdAsync(tl, js);
                    Thread.Sleep(500);
                    //查看重复任务是否有重复任务
                    await ClosePopWindow(true);
                    //查看是否有弹出窗口
                    tl.Dispose();
                    log.LogInformation("已开始下载，开始查询任务ID");

                    return freeSql.Select<Prot.TaskBase>().OrderByDescending(x => x.CreationTime).First().TaskId;
                }
            }
            finally
            {
                // 确保在操作完成后重置isWorking标志
                lock (_operationLock)
                {
                    isWorking = false;
                }
            }
        }


        private async Task ClosePopWindow(bool yes)
        {
            var s = GetFrames().FirstOrDefault(x => x.Title == "消息提示框");
            if (null == s) return;
            using (var dev = await SkraprDevTools.Connect(tempServiceProvider, s))
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
            try
            {
                // 使用FreeSQL查询所有任务
                var tasks = await freeSql.Select<Prot.TaskBase>().ToListAsync();
                return tasks.Select(x => new TaskRecored(x));
            }
            catch (Exception ex)
            {
                throw new Exception("获取所有任务失败", ex);
            }
        }

        public async Task<IEnumerable<TaskRecored>> GetUnfinishedTasksAsync()
        {
            try
            {
                // 使用FreeSQL查询未完成任务
                var tasks = await freeSql.Select<Prot.TaskBase>()
                    .Where(x => x.Status == (int)DownloadStatus.Downloading || x.Status == (int)DownloadStatus.Unknown6)
                    .ToListAsync();
                return tasks.Select(x => new TaskRecored(x));
            }
            catch (Exception ex)
            {
                throw new Exception("获取未完成任务失败", ex);
            }
        }

        public async Task<IEnumerable<TaskRecored>> GetCompletedTasksAsync()
        {
            try
            {
                // 使用FreeSQL查询已完成任务
                var tasks = await freeSql.Select<Prot.TaskBase>()
                    .Where(x => x.Status != (int)DownloadStatus.Downloading)
                    .ToListAsync();
                return tasks.Select(x => new TaskRecored(x));
            }
            catch (Exception ex)
            {
                throw new Exception("获取已完成任务失败", ex);
            }
        }

        public async Task<TaskRecored?> GetTasksAsync(Int64 taskId)
        {
            try
            {
                // 使用FreeSQL查询指定任务
                var task = freeSql.Select<Prot.TaskBase>().Where(a => a.TaskId == taskId).ToOne();

                return task == null ? null : new TaskRecored(task);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "获取任务详情失败: {TaskId}", taskId);
                throw;
            }
        }

        public async Task<String?> GetLoginInfoAsync()
        {
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
            // 使用锁确保登录操作的互斥性
            lock (_operationLock)
            {
                if (isWorking) throw new Exception("系统正忙，稍后再试");
                isWorking = true;
            }

            try
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
                // 增加等待时间，确保登录页面完全加载
                Thread.Sleep(500);
                using (var tl = await SkraprDevTools.Connect(tempServiceProvider, taskpage))
                {
                    // 等待iframe加载完成
                    await Task.Delay(1000);
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
            finally
            {
                // 确保在操作完成后重置isWorking标志
                lock (_operationLock)
                {
                    isWorking = false;
                }
            }
        }

        public async Task<byte[]> GetQRCodeAsync()
        {
            // 使用锁确保登录操作的互斥性
            lock (_operationLock)
            {
                if (isWorking) throw new Exception("系统正忙，稍后再试");
                isWorking = true;
            }

            try
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
                // 增加等待时间，确保登录页面完全加载
                Thread.Sleep(3000);
                using (var tl = await SkraprDevTools.Connect(tempServiceProvider, taskpage))
                {
                    // 等待iframe加载完成
                    await Task.Delay(1000);
                    js = @"
(function(){
return document.getElementById('xlx-code-img').src;
        })();";
                    r = await RunCmdAsync(tl, js);
                    string bs = (r.Result.Value + "").Split(',').Last();
                    // 不立即关闭登录窗口，让用户有时间扫描二维码
                    return Convert.FromBase64String(bs);
                }
            }
            finally
            {
                // 确保在操作完成后重置isWorking标志
                lock (_operationLock)
                {
                    isWorking = false;
                }
            }
        }

        public async Task CloseLoginWindowAsync()
        {
            // 使用锁确保关闭登录窗口操作的互斥性
            lock (_operationLock)
            {
                if (isWorking) throw new Exception("系统正忙，稍后再试");
                isWorking = true;
            }

            try
            {
                var taskpage = GetFrames().FirstOrDefault(x => x.Title == "迅雷个人中心-登录");
                if (taskpage != null)
                {
                    using (var tl = await SkraprDevTools.Connect(tempServiceProvider, taskpage))
                    {
                        await RunCmdAsync(tl, "document.getElementById('loginIframe').contentWindow.document.getElementById('close').click()");
                    }
                }
            }
            finally
            {
                // 确保在操作完成后重置isWorking标志
                lock (_operationLock)
                {
                    isWorking = false;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // 释放FreeSQL资源
                    freeSql?.Dispose();
                    devTools?.Dispose();
                    if (!ps.HasExited)
                    {
                        ps.Kill(true);
                    }
                }

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
        Completed = 8,
        UnknownError = 9
    }
}