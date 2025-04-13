using Microsoft.EntityFrameworkCore;
using QuestionBankApi.LoginSystemApi.Models;

namespace QuestionBankApi.LoginSystemApi.Data;

/// <summary>
/// 用户数据库上下文
/// </summary>
public class UserDBContext : DbContext
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">数据库上下文选项</param>
    public UserDBContext(DbContextOptions<UserDBContext> options) : base(options) { }

    /// <summary>
    /// 用户表
    /// </summary>
    public DbSet<User> Users => Set<User>();
    
    /// <summary>
    /// 模型创建时的配置
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // 配置用户表
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
        });
        
        // 添加种子数据
        var adminPasswordHash = BCryptHashPassword("admin123");
        var studentPasswordHash = BCryptHashPassword("student123");
        
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "admin", PasswordHash = adminPasswordHash, Role = UserRole.Admin },
            new User { Id = 2, Username = "student", PasswordHash = studentPasswordHash, Role = UserRole.Student }
        );
    }
    
    /// <summary>
    /// 使用 BCrypt 对密码进行哈希处理
    /// </summary>
    /// <param name="password">原始密码</param>
    /// <returns>哈希后的密码</returns>
    private static string BCryptHashPassword(string password)
    {
        // 在实际应用中，应该使用 BCrypt.Net-Next 包
        // 这里使用简单的 SHA256 哈希作为示例
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
