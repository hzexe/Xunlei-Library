using Hzexe.Xunlei_Library;

namespace Hzexe.Xunlei
{
    public class Xunlei
    {
        private readonly Api api;
        private readonly ILogger log;

        public Xunlei(IServiceProvider sc, IConfiguration configuration, ILogger<Xunlei> log)
        {
            this.log = log;
            var path = configuration.GetValue<string>("XunleiPath")!;
            api = new Api(sc, path);
            api.ExitEvent += Api_ExitEvent;
        }

        private void Api_ExitEvent(object? sender, EventArgs e)
        {
            log.LogInformation("迅雷已退出");
        }

        public async Task<long?> DownloadLinkAsync(string url)
        {
            return await api.DownloadLinkAsync(url);
        }

        public async Task<IEnumerable<TaskRecored>> GetAllTasksAsync()
        {
            return await api.GetAllTasksAsync();
        }

        public async Task<TaskRecored?> GetTasksAsync(Int64 taskId)
        {
            return await api.GetTasksAsync(taskId);
        }

        public async Task<String?> GetLoginInfoAsync()
        {
            return await api.GetLoginInfoAsync();
        }

        public async Task LoginByEWMAsync()
        {
            await api.LoginByEWMAsync();
        }

        public async Task LoginByPassword(string username, string password)
        {
            await api.LoginByPassword(username, password);
        }

        public void Dispose()
        {
            api.Dispose();
        }
    }
}
