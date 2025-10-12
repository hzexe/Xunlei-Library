using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hzexe.Xunlei_Library;
using Hzexe.Xunlei;
using System;
using System.Threading.Tasks;

namespace Hzexe.Xunlei.Controllers
{
    /// <summary>
    /// 迅雷任务管理控制器，提供下载任务的CRUD操作
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly Xunlei _xunlei;
        private readonly ILogger<TasksController> _logger;

        /// <summary>
        /// 初始化任务控制器
        /// </summary>
        /// <param name="xunlei">迅雷服务实例</param>
        /// <param name="logger">日志记录器</param>
        public TasksController(Xunlei xunlei, ILogger<TasksController> logger)
        {
            _xunlei = xunlei;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有下载任务
        /// </summary>
        /// <param name="page">页码，从1开始</param>
        /// <param name="pageSize">每页大小，默认为20</param>
        /// <returns>任务列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllTasks(int page = 1, int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                
                var tasks = await _xunlei.GetAllTasksAsync();
                var pagedTasks = tasks
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                return Ok(new { 
                    success = true, 
                    data = pagedTasks,
                    totalCount = tasks.Count(),
                    page = page,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取所有任务失败");
                return StatusCode(500, new { success = false, message = "获取任务列表失败" });
            }
        }

        /// <summary>
        /// 获取未完成的下载任务（下载中和暂停的任务）
        /// </summary>
        /// <param name="page">页码，从1开始</param>
        /// <param name="pageSize">每页大小，默认为20</param>
        /// <returns>未完成任务列表</returns>
        [HttpGet("unfinished")]
        public async Task<IActionResult> GetUnfinishedTasks(int page = 1, int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                
                var tasks = await _xunlei.GetUnfinishedTasksAsync();
                var pagedTasks = tasks
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                return Ok(new { 
                    success = true, 
                    data = pagedTasks,
                    totalCount = tasks.Count(),
                    page = page,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取未完成任务失败");
                return StatusCode(500, new { success = false, message = "获取未完成任务列表失败" });
            }
        }

        /// <summary>
        /// 根据状态获取任务（分页）
        /// </summary>
        /// <param name="status">任务状态：downloading（下载中）或completed（已完成）</param>
        /// <param name="page">页码，默认为1</param>
        /// <param name="pageSize">每页大小，默认为10</param>
        /// <returns>指定状态的任务列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetTasksByStatus(string status, int page = 1, int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                
                IEnumerable<TaskRecored> tasks;
                
                if (status == "downloading")
                {
                    tasks = await _xunlei.GetDownloadingTasksAsync();
                }
                else if (status == "completed")
                {
                    tasks = await _xunlei.GetCompletedTasksAsync();
                }
                else
                {
                    return BadRequest(new { success = false, message = "无效的状态参数，支持的状态：downloading, completed" });
                }
                
                var totalCount = tasks.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                
                // 确保页码在有效范围内
                page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));
                
                var pagedTasks = tasks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                
                return Ok(new {
                    success = true,
                    tasks = pagedTasks,
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据状态获取任务时发生错误，状态: {Status}", status);
                return StatusCode(500, new { success = false, message = "获取任务列表失败" });
            }
        }

        /// <summary>
        /// 获取下载中的任务（分页）
        /// </summary>
        /// <param name="page">页码，默认为1</param>
        /// <param name="pageSize">每页大小，默认为10</param>
        /// <returns>下载中的任务列表</returns>
        [HttpGet("downloading")]
        public async Task<IActionResult> GetDownloadingTasks(int page = 1, int pageSize = 10)
        {
            try
            {
                var allTasks = await _xunlei.GetDownloadingTasksAsync();
                var totalCount = allTasks.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                
                // 确保页码在有效范围内
                page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));
                
                var pagedTasks = allTasks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                
                return Ok(new {
                    success = true,
                    tasks = pagedTasks,
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取下载中任务时发生错误");
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// 获取已完成的任务（分页）
        /// </summary>
        /// <param name="page">页码，默认为1</param>
        /// <param name="pageSize">每页大小，默认为10</param>
        /// <returns>已完成的任务列表</returns>
        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedTasks(int page = 1, int pageSize = 10)
        {
            try
            {
                var allTasks = await _xunlei.GetCompletedTasksAsync();
                var totalCount = allTasks.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                
                // 确保页码在有效范围内
                page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));
                
                var pagedTasks = allTasks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                
                return Ok(new {
                    success = true,
                    tasks = pagedTasks,
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取已完成任务时发生错误");
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// 获取下载中任务数量
        /// </summary>
        /// <returns>下载中任务数量</returns>
        [HttpGet("downloading/count")]
        public async Task<IActionResult> GetDownloadingTasksCount()
        {
            const int maxRetries = 3;
            int retryCount = 0;
            
            while (retryCount < maxRetries)
            {
                try
                {
                    var tasks = await _xunlei.GetDownloadingTasksAsync();
                    return Ok(new { 
                        success = true, 
                        count = tasks.Count() 
                    });
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogWarning(ex, "获取下载中任务数量失败，重试次数: {RetryCount}/{MaxRetries}", retryCount, maxRetries);
                    
                    // 如果是最后一次重试，记录错误并返回失败
                    if (retryCount >= maxRetries)
                    {
                        _logger.LogError(ex, "获取下载中任务数量失败，已达到最大重试次数");
                        return StatusCode(500, new { success = false, message = "获取任务数量失败" });
                    }
                    
                    // 等待一段时间后重试
                    await Task.Delay(500 * retryCount);
                }
            }
            
            // 理论上不会执行到这里，但为了编译通过
            return StatusCode(500, new { success = false, message = "获取任务数量失败" });
        }

        /// <summary>
        /// 获取已完成任务数量
        /// </summary>
        /// <returns>已完成任务数量</returns>
        [HttpGet("completed/count")]
        public async Task<IActionResult> GetCompletedTasksCount()
        {
            try
            {
                var tasks = await _xunlei.GetCompletedTasksAsync();
                return Ok(new { 
                    success = true, 
                    count = tasks.Count() 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取已完成任务数量失败");
                return StatusCode(500, new { success = false, message = "获取任务数量失败" });
            }
        }

        /// <summary>
        /// 根据任务ID获取任务详情
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <returns>任务详情</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(long id)
        {
            try
            {
                var task = await _xunlei.GetTasksAsync(id);
                if (task == null)
                {
                    return NotFound();
                }
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务详情时发生错误，任务ID: {TaskId}", id);
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// 创建新的下载任务
        /// </summary>
        /// <param name="request">下载请求</param>
        /// <returns>创建结果</returns>
        [HttpPost("download")]
        public async Task<IActionResult> CreateDownloadTask([FromBody] DownloadRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Url))
                {
                    return BadRequest("下载链接不能为空");
                }

                var taskId = await _xunlei.DownloadLinkAsync(request.Url);
                if (taskId.HasValue)
                {
                    return Ok(new { success = true, taskId = taskId.Value });
                }
                else
                {
                    return BadRequest("创建下载任务失败");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建下载任务时发生错误，URL: {Url}", request.Url);
                return Problem(ex.Message);
            }
        }
    }

    /// <summary>
    /// 下载请求模型
    /// </summary>
    public class DownloadRequest
    {
        /// <summary>
        /// 下载链接
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }
}