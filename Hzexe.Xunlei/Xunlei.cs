using Hzexe.Xunlei_Library;

namespace Hzexe.Xunlei
{
    /// <summary>
    /// 迅雷封装类，提供下载任务管理和用户登录功能
    /// </summary>
    public class Xunlei : IDisposable
    {
        private readonly Api api;
        private readonly ILogger log;

        /// <summary>
        /// 初始化迅雷封装类
        /// </summary>
        /// <param name="log">日志记录器</param>
        /// <param name="api">API实例</param>
        public Xunlei(ILogger<Xunlei> log, Api api)
        {
            this.log = log;
            this.api = api;
            //api.ExitEvent += Api_ExitEvent;
        }

        /// <summary>
        /// 处理迅雷退出事件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void Api_ExitEvent(object? sender, EventArgs e)
        {
            log.LogInformation("迅雷已退出");
        }

        /// <summary>
        /// 创建下载任务
        /// </summary>
        /// <param name="url">下载链接</param>
        /// <returns>任务ID</returns>
        public async Task<long?> DownloadLinkAsync(string url)
        {
            return await api.DownloadLinkAsync(url);
        }

        /// <summary>
        /// 获取所有任务
        /// </summary>
        /// <returns>任务列表</returns>
        public async Task<IEnumerable<TaskRecored>> GetAllTasksAsync()
        {
            return await api.GetAllTasksAsync();
        }

        /// <summary>
        /// 获取下载中的任务
        /// </summary>
        /// <returns>下载中的任务列表</returns>
        public async Task<IEnumerable<TaskRecored>> GetDownloadingTasksAsync()
        {
            // 直接调用Api的GetUnfinishedTasksAsync方法，它已经筛选了下载中的任务
            var unfinishedTasks = await api.GetUnfinishedTasksAsync();
            return unfinishedTasks.Where(t => t.Status == DownloadStatus.Downloading);
        }

        /// <summary>
        /// 获取未完成的下载任务（下载中和暂停的任务）
        /// </summary>
        /// <returns>未完成任务列表</returns>
        public async Task<IEnumerable<TaskRecored>> GetUnfinishedTasksAsync()
        {
            return await api.GetUnfinishedTasksAsync();
        }

        /// <summary>
        /// 获取已完成的任务
        /// </summary>
        /// <returns>已完成的任务列表</returns>
        public async Task<IEnumerable<TaskRecored>> GetCompletedTasksAsync()
        {
            // 直接调用Api的GetCompletedTasksAsync方法，避免获取所有任务后再筛选
            return await api.GetCompletedTasksAsync();
        }

        /// <summary>
        /// 获取特定任务
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>任务详情</returns>
        public async Task<TaskRecored?> GetTasksAsync(Int64 taskId)
        {
            return await api.GetTasksAsync(taskId);
        }

        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        /// <returns>用户名</returns>
        public async Task<String?> GetLoginInfoAsync()
        {
            return await api.GetLoginInfoAsync();
        }

        /// <summary>
        /// 获取二维码图片数据
        /// </summary>
        /// <returns>二维码图片字节数组</returns>
        public async Task<byte[]> GetQRCodeAsync()
        {
            return await api.GetQRCodeAsync();
        }

        /// <summary>
        /// 用户名密码登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        public async Task LoginByPassword(string username, string password)
        {
            await api.LoginByPassword(username, password);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            api.Dispose();
        }
    }
}
