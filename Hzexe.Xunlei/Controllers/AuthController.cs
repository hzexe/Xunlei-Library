using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hzexe.Xunlei_Library;
using Hzexe.Xunlei;
using System;
using System.Threading.Tasks;

namespace Hzexe.Xunlei.Controllers
{
    /// <summary>
    /// 用户认证控制器，提供登录和用户信息查询功能
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly Xunlei _xunlei;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// 初始化认证控制器
        /// </summary>
        /// <param name="xunlei">迅雷服务实例</param>
        /// <param name="logger">日志记录器</param>
        public AuthController(Xunlei xunlei, ILogger<AuthController> logger)
        {
            _xunlei = xunlei;
            _logger = logger;
        }

        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        /// <returns>用户信息，未登录时返回null</returns>
        [HttpGet("info")]
        public async Task<IActionResult> GetLoginInfo()
        {
            try
            {
                var loginInfo = await _xunlei.GetLoginInfoAsync();
                return Ok(new { username = loginInfo, isLoggedIn = !string.IsNullOrEmpty(loginInfo) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取登录信息时发生错误");
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// 使用用户名密码登录
        /// </summary>
        /// <param name="request">登录请求</param>
        /// <returns>登录结果</returns>
        [HttpPost("login")]
        public async Task<IActionResult> LoginWithPassword([FromBody] PasswordLoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest("用户名和密码不能为空");
                }

                await _xunlei.LoginByPassword(request.Username, request.Password);
                var loginInfo = await _xunlei.GetLoginInfoAsync();
                return Ok(new { success = true, username = loginInfo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户名密码登录时发生错误，用户名: {Username}", request.Username);
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// 获取二维码登录图片
        /// </summary>
        /// <returns>二维码图片</returns>
        [HttpGet("qrcode")]
        public async Task<IActionResult> GetQRCode()
        {
            try
            {
                // 获取二维码图片数据
                byte[] qrCodeData = await _xunlei.GetQRCodeAsync();
                
                // 将字节数组转换为Base64字符串
                var base64String = Convert.ToBase64String(qrCodeData);
                var dataUrl = $"data:image/png;base64,{base64String}";
                
                return Ok(new { 
                    success = true, 
                    data = new { 
                        qrCodeUrl = dataUrl,
                        qrCodeId = Guid.NewGuid().ToString() // 生成一个唯一ID用于状态检查
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取二维码时发生错误");
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// 检查二维码登录状态
        /// </summary>
        /// <param name="id">二维码ID（可选）</param>
        /// <returns>登录状态</returns>
        [HttpGet("qrcode/{id?}/status")]
        public async Task<IActionResult> CheckQRCodeStatus(string? id = null)
        {
            try
            {
                var loginInfo = await _xunlei.GetLoginInfoAsync();
                bool isLoggedIn = !string.IsNullOrEmpty(loginInfo);
                
                if (isLoggedIn)
                {
                    return Ok(new { 
                        success = true,
                        data = new {
                            status = "confirmed",
                            user = new { username = loginInfo }
                        }
                    });
                }
                else
                {
                    return Ok(new { 
                        success = true,
                        data = new {
                            status = "waiting"
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查二维码登录状态时发生错误");
                return Problem(ex.Message);
            }
        }
    }

    /// <summary>
    /// 用户名密码登录请求模型
    /// </summary>
    public class PasswordLoginRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}