using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Hzexe.Xunlei_Library;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hzexe.Xunlei.Hubs
{
    /// <summary>
    /// 任务更新通知中心，用于实时推送下载任务状态变化
    /// </summary>
    public class TaskUpdateHub : Hub
    {
        /// <summary>
        /// 客户端连接时调用
        /// </summary>
        /// <returns>异步任务</returns>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 客户端断开连接时调用
        /// </summary>
        /// <param name="exception">断开连接的异常信息</param>
        /// <returns>异步任务</returns>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }

    /// <summary>
    /// 任务更新后台服务，定期检查下载任务状态并推送更新
    /// </summary>
    public class TaskUpdateService : BackgroundService
    {
        private readonly IHubContext<TaskUpdateHub> _hubContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TaskUpdateService> _logger;
        private readonly ConcurrentDictionary<string, DateTime> _lastUpdateTime = new();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="hubContext">SignalR Hub上下文</param>
        /// <param name="serviceProvider">服务提供者，用于创建作用域服务</param>
        /// <param name="logger">日志记录器</param>
        public TaskUpdateService(IHubContext<TaskUpdateHub> hubContext, IServiceProvider serviceProvider, ILogger<TaskUpdateService> logger)
        {
            _hubContext = hubContext;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// 后台服务执行方法
        /// </summary>
        /// <param name="stoppingToken">停止令牌</param>
        /// <returns>异步任务</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 创建服务作用域以避免DbContext并发问题
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var xunlei = scope.ServiceProvider.GetRequiredService<Xunlei>();
                        
                        // 获取下载中的任务，添加重试机制
                        var downloadingTasks = await GetTasksWithRetryAsync(xunlei, stoppingToken);
                        
                        // 检查每个下载中任务是否有更新
                        foreach (var task in downloadingTasks)
                        {
                            var taskId = task.TaskId.ToString();
                            var currentTime = DateTime.Now;
                            
                            // 如果是第一次看到这个任务，或者距离上次更新超过5秒，则推送更新
                            if (!_lastUpdateTime.ContainsKey(taskId) || 
                                (currentTime - _lastUpdateTime[taskId]).TotalSeconds >= 5)
                            {
                                _lastUpdateTime[taskId] = currentTime;
                                
                                // 推送任务更新到客户端
                                await _hubContext.Clients.All.SendAsync("UpdateTask", new
                                {
                                    TaskId = task.TaskId,
                                    Name = task.Name,
                                    Status = task.Status,
                                    TotalReceiveSize = task.TotalReceiveSize,
                                    ResourceSize = task.ResourceSize,
                                    Progress = CalculateProgress(task.TotalReceiveSize, task.ResourceSize)
                                }, stoppingToken);
                            }
                        }
                    }
                    
                    // 等待5秒后再次检查
                    await Task.Delay(5000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "检查下载任务更新时发生错误");
                    await Task.Delay(10000, stoppingToken); // 出错时等待10秒后重试
                }
            }
        }

        /// <summary>
        /// 带重试机制获取下载任务
        /// </summary>
        /// <param name="xunlei">Xunlei服务实例</param>
        /// <param name="stoppingToken">停止令牌</param>
        /// <returns>下载任务列表</returns>
        private async Task<IEnumerable<TaskRecored>> GetTasksWithRetryAsync(Xunlei xunlei, CancellationToken stoppingToken)
        {
            const int maxRetries = 3;
            int retryCount = 0;
            
            while (retryCount < maxRetries)
            {
                try
                {
                    return await xunlei.GetDownloadingTasksAsync();
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogWarning(ex, "获取下载任务失败，重试次数: {RetryCount}/{MaxRetries}", retryCount, maxRetries);
                    
                    // 如果是最后一次重试，抛出异常
                    if (retryCount >= maxRetries)
                    {
                        _logger.LogError(ex, "获取下载任务失败，已达到最大重试次数");
                        throw;
                    }
                    
                    // 等待一段时间后重试
                    await Task.Delay(500 * retryCount, stoppingToken);
                }
            }
            
            // 理论上不会执行到这里，但为了编译通过
            throw new Exception("获取下载任务失败");
        }

        /// <summary>
        /// 计算下载进度
        /// </summary>
        /// <param name="totalReceiveSize">已接收大小</param>
        /// <param name="resourceSize">资源总大小</param>
        /// <returns>进度百分比</returns>
        private static int CalculateProgress(long totalReceiveSize, long resourceSize)
        {
            if (resourceSize <= 0) return 0;
            return Math.Min(100, (int)((double)totalReceiveSize / resourceSize * 100));
        }
    }
}