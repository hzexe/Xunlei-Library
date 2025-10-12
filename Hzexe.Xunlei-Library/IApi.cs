
namespace Hzexe.Xunlei_Library
{
    /// <summary>
    /// 迅雷API接口，提供下载任务管理和用户登录功能
    /// </summary>
    public interface IApi: IDisposable
    {
        /// <summary>
        /// 下载指定链接
        /// </summary>
        /// <param name="url">下载链接</param>
        /// <returns>任务ID</returns>
        Task<long?> DownloadLinkAsync(string url);
        
        /// <summary>
        /// 获取所有下载任务
        /// </summary>
        /// <returns>任务列表</returns>
        Task<IEnumerable<TaskRecored>> GetAllTasksAsync();
        
        /// <summary>
        /// 获取未完成的下载任务（下载中和暂停的任务）
        /// </summary>
        /// <returns>未完成任务列表</returns>
        Task<IEnumerable<TaskRecored>> GetUnfinishedTasksAsync();
        
        /// <summary>
        /// 获取已完成的下载任务
        /// </summary>
        /// <returns>已完成任务列表</returns>
        Task<IEnumerable<TaskRecored>> GetCompletedTasksAsync();
        
       
        /// <summary>
        /// 根据任务ID获取任务信息
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>任务信息</returns>
        Task<TaskRecored?> GetTasksAsync(long taskId);
        
        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        /// <returns>用户名，未登录返回null</returns>
        Task<String?> GetLoginInfoAsync();
        
        /// <summary>
        /// 使用用户名密码登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        Task LoginByPassword(string username, string password);
        
        /// <summary>
        /// 获取二维码登录图片
        /// </summary>
        /// <returns>二维码图片字节数组</returns>
        Task<byte[]> GetQRCodeAsync();
    }
}