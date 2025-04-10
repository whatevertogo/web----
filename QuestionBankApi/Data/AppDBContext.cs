/*
 * AppDbContext.cs
 * ----------------
 * Entity Framework Core 的数据库上下文类。
 * 负责管理数据库连接、实体映射，定义数据库中的数据表。
 *
 * 关键点：
 * - 继承自EF Core的DbContext
 * - 定义了题目表Questions
 */

using Microsoft.EntityFrameworkCore;
using QuestionBankApi.Models;

namespace QuestionBankApi.Data;

/// <summary>
/// 应用程序的数据库上下文，管理数据库连接和实体映射
public class AppDbContext : DbContext
/// </summary>
{
    /// <summary>
    /// 构造函数，传入数据库上下文配置参数
    /// </summary>
    /// <param name="options">数据库上下文配置</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>
    /// 题目表，存储所有题目信息
    /// </summary>
    public DbSet<Question> Questions => Set<Question>();
}
