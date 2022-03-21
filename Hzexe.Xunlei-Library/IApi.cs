
namespace Hzexe.Xunlei_Library
{
    public interface IApi: IDisposable
    {
        Task<long?> DownloadLinkAsync(string url);
        Task<IEnumerable<TaskRecored>> GetAllTasksAsync();
        Task<TaskRecored?> GetTasksAsync(long taskId);
    }
}