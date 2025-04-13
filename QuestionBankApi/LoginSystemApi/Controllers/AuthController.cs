using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionBankApi.LoginSystemApi.DTOs;
using QuestionBankApi.LoginSystemApi.Models;
using QuestionBankApi.LoginSystemApi.Services;

namespace QuestionBankApi.LoginSystemApi.Controllers;

/// <summary>
/// 认证控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="authService">认证服务</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="model">登录请求</param>
    /// <returns>JWT 令牌</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        // 打印调试信息
        Console.WriteLine($"接收到登录请求: 用户名={model.Username}, 密码长度={model.Password?.Length ?? 0}");

        // 检查用户名和密码是否为空
        if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
        {
            Console.WriteLine("用户名或密码为空");
            return Unauthorized(new { message = "用户名和密码不能为空" });
        }

        // 使用空合并运算符确保参数不为 null
        var token = await _authService.LoginAsync(model.Username ?? string.Empty, model.Password ?? string.Empty);
        if (token == null)
        {
            Console.WriteLine("登录失败，返回未授权响应");
            return Unauthorized(new { message = "用户名或密码错误" });
        }

        Console.WriteLine("登录成功，返回令牌");
        return Ok(new { token });
    }

    /// <summary>
    /// 用户注册（仅管理员可用）
    /// </summary>
    /// <param name="model">注册请求</param>
    /// <returns>注册结果</returns>
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        // 检查用户名和密码是否为空
        if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
        {
            return BadRequest(new { message = "用户名和密码不能为空" });
        }

        var result = await _authService.RegisterAsync(model.Username, model.Password, model.Role);
        if (!result)
            return BadRequest(new { message = "用户名已存在" });
        return Ok(new { message = "注册成功" });
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <returns>用户信息</returns>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return Ok(new { userId, username, role });
    }

    /// <summary>
    /// 创建测试用户（仅开发环境使用）
    /// </summary>
    /// <returns>创建结果</returns>
    [HttpGet("create-test-user")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateTestUser()
    {
        try
        {
            // 创建特定的测试用户
            var username = "1879483647";
            var password = "159286";

            // 检查用户是否已存在
            // 确保用户名和密码不为 null
            var result = await _authService.RegisterAsync(username, password, QuestionBankApi.LoginSystemApi.Models.UserRole.Admin);

            if (result)
            {
                return Ok(new { message = $"测试用户 {username} 创建成功" });
            }
            else
            {
                return Ok(new { message = $"用户 {username} 已存在" });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"创建测试用户失败: {ex.Message}" });
        }
    }
}
