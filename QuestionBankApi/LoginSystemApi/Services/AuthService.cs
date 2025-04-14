using Microsoft.EntityFrameworkCore;
using QuestionBankApi.LoginSystemApi.Data;
using QuestionBankApi.LoginSystemApi.Helpers;
using QuestionBankApi.LoginSystemApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace QuestionBankApi.LoginSystemApi.Services;

/// <summary>
/// 认证服务实现
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserDBContext _db;
    private readonly IJwtHelper _jwt;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db">用户数据库上下文</param>
    /// <param name="jwt">JWT 帮助类</param>
    public AuthService(UserDBContext db, IJwtHelper jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <returns>JWT 令牌，登录失败返回 null</returns>
    public async Task<string?> LoginAsync(string username, string password)
    {
        // 检查参数
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Console.WriteLine("用户名或密码为空");
            return null;
        }

        // 打印调试信息
        Console.WriteLine($"尝试登录用户: {username}");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            Console.WriteLine($"用户不存在: {username}");
            return null;
        }

        // 计算当前密码的哈希值进行比较
        var inputHash = HashPassword(password);
        var storedHash = user.PasswordHash;

        Console.WriteLine($"输入密码的哈希: {inputHash}");
        Console.WriteLine($"存储的密码哈希: {storedHash}");

        if (inputHash != storedHash)
        {
            Console.WriteLine("密码验证失败");
            return null;
        }

        Console.WriteLine("登录成功，生成令牌");
        return _jwt.GenerateToken(user);
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <param name="role">角色</param>
    /// <returns>是否注册成功</returns>
    public async Task<bool> RegisterAsync(string username, string password, UserRole role)
    {
        if (await _db.Users.AnyAsync(u => u.Username == username)) return false;
        var hash = HashPassword(password);
        _db.Users.Add(new User { Username = username, PasswordHash = hash, Role = role });
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 密码哈希方法
    /// </summary>
    /// <param name="password">原始密码</param>
    /// <returns>哈希后的密码</returns>
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
