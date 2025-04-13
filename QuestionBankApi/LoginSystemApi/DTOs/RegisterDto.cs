using QuestionBankApi.LoginSystemApi.Models;

namespace QuestionBankApi.LoginSystemApi.DTOs;

/// <summary>
/// 注册请求DTO
/// </summary>
public class RegisterDto : LoginDto
{
    /// <summary>
    /// 用户角色
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Student;
}
