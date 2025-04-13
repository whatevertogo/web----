using QuestionBankApi.LoginSystemApi.Models;

namespace QuestionBankApi.LoginSystemApi.Services;

/// <summary>
/// 认证服务接口
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <returns>JWT 令牌，登录失败返回 null</returns>
    Task<string?> LoginAsync(string username, string password);
    
    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <param name="role">角色</param>
    /// <returns>是否注册成功</returns>
    Task<bool> RegisterAsync(string username, string password, UserRole role);
}
