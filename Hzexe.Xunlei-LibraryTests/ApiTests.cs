using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hzexe.Xunlei_Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;

namespace Hzexe.Xunlei_Library.Tests
{
    [TestClass()]
    public class ApiTests
    {
        Api api;
        [TestMethod()]
        [TestInitialize]
        public void ApiTest()
        {
            var serviceProvider = new ServiceCollection()
              .AddLogging()
              .BuildServiceProvider();
            api = new Api(serviceProvider,@"D:\Program Files\Thunder\Program\Thunder.exe", 64321);
        }

        [TestMethod()]
        public async Task DownloadTest()
        {
           long? taskid =await api.DownloadLinkAsync(@"https://download.visualstudio.microsoft.com/download/pr/144a5711-f076-44fa-bf55-f7e0121eb30c/B7AE307237F869E09F7413691A2CD1944357B5CEE28049C0A0D3430B47BB3EDC/VC_redist.x86.exe");
            if (taskid.HasValue)
            {
                var task = await api.GetTasksAsync(taskid.Value);
                Assert.IsNotNull(task);
            }
            var tasks=await api.GetAllTasksAsync();
            Assert.IsNotNull(tasks);
        }

        [TestMethod()]
        public async Task LoginByPasswordTest()
        {
            string? uname=await api.GetLoginInfoAsync();
            if (string.IsNullOrEmpty(uname))
            {
                await api.LoginByPassword("133xxxxxxxx", "xxxxxxxxx");
                Assert.IsNotNull(api);
            }
            
                uname = await api.GetLoginInfoAsync();
                Assert.IsTrue(uname.Length > 1);
            
        }


        [TestMethod()]
        public async Task LoginByEWMAsyncTest()
        {
            string? uname = await api.GetLoginInfoAsync();
            if (string.IsNullOrEmpty(uname))
            {
                var picdata = await api.LoginByEWMAsync();
                using (var ms = new System.IO.MemoryStream(picdata))
                {
                    var bitmap = Image.FromStream(ms);
                    Assert.IsNotNull(bitmap);
                }
            }
            else
            {
                uname = await api.GetLoginInfoAsync();
                Assert.IsTrue(uname.Length > 1);
            }
        }
    }


}