namespace QuestionBankApi.LoginSystemApi.Models;

/// <summary>
/// 用户角色枚举
/// </summary>
public enum UserRole
{
    /// <summary>
    /// 学生角色，只能访问答题页面
    /// </summary>
    Student,
    
    /// <summary>
    /// 管理员角色，可以访问所有功能
    /// </summary>
    Admin
}

/// <summary>
/// 用户实体类
/// </summary>
public class User
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = null!;
    
    /// <summary>
    /// 密码哈希
    /// </summary>
    public string PasswordHash { get; set; } = null!;
    
    /// <summary>
    /// 用户角色
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Student;
}
