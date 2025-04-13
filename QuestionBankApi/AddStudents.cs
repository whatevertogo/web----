using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionBankApi.LoginSystemApi.Data;
using QuestionBankApi.LoginSystemApi.Models;
using QuestionBankApi.LoginSystemApi.Services;

namespace QuestionBankApi;

/// <summary>
/// 添加学生用户的工具类
/// </summary>
public static class AddStudents
{
    /// <summary>
    /// 添加测试学生用户
    /// </summary>
    /// <param name="app">Web应用程序</param>
    public static void AddTestStudents(WebApplication app)
    {
        // 创建一个新的作用域
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                // 获取用户数据库上下文
                var userDbContext = services.GetRequiredService<UserDBContext>();

                // 检查是否已经有学生用户
                if (!userDbContext.Users.Any(u => u.Role == UserRole.Student))
                {
                    logger.LogInformation("添加测试学生用户...");

                    // 添加5个测试学生
                    for (int i = 1; i <= 5; i++)
                    {
                        var username = $"student{i}";
                        var passwordHash = HashPassword("student123");

                        // 检查用户是否已存在
                        if (!userDbContext.Users.Any(u => u.Username == username))
                        {
                            userDbContext.Users.Add(new User
                            {
                                Username = username,
                                PasswordHash = passwordHash,
                                Role = UserRole.Student
                            });
                        }
                    }

                    userDbContext.SaveChanges();
                    logger.LogInformation("测试学生用户添加完成");
                }
                else
                {
                    logger.LogInformation("已存在学生用户，跳过添加测试学生");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "添加测试学生用户时出错");
            }
        }
    }

    /// <summary>
    /// 计算密码的哈希值
    /// </summary>
    /// <param name="password">密码</param>
    /// <returns>哈希值</returns>
    private static string HashPassword(string password)
    {
        // 使用与 AuthService 相同的哈希算法
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
