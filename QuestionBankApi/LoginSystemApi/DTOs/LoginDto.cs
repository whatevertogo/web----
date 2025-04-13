namespace QuestionBankApi.LoginSystemApi.DTOs;

/// <summary>
/// 登录请求DTO
/// </summary>
public class LoginDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = null!;
    
    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = null!;
}
