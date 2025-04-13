/*
 * QuestionBankDbContext.cs
 * ----------------
 * Entity Framework Core 的数据库上下文类。
 * 负责管理数据库连接、实体映射，定义数据库中的数据表。
 *
 * 关键点：
 * - 继承自EF Core的DbContext
 * - 定义了题目表Questions
 */

using Microsoft.EntityFrameworkCore;
using QuestionBankApi.Enums;
using QuestionBankApi.Models;

namespace QuestionBankApi.Data;

/// <summary>
/// 应用程序的数据库上下文，管理数据库连接和实体映射
public class QuestionBankDbContext : DbContext
/// </summary>
{
    /// <summary>
    /// 构造函数，传入数据库上下文配置参数
    /// </summary>
    /// <param name="options">数据库上下文配置</param>
    public QuestionBankDbContext(DbContextOptions<QuestionBankDbContext> options) : base(options) { }

    /// <summary>
    /// 题目表，存储所有题目信息
    /// </summary>
    public DbSet<Question> Questions => Set<Question>();

    /// <summary>
    /// 试卷表，存储所有试卷信息
    /// </summary>
    public DbSet<Exam> Exams => Set<Exam>();

    /// <summary>
    /// 试卷题目关联表，存储试卷和题目的关联关系
    /// </summary>
    public DbSet<ExamQuestion> ExamQuestions => Set<ExamQuestion>();

    /// <summary>
    /// 试卷分配表，存储试卷分配给学生的记录
    /// </summary>
    public DbSet<ExamAssignment> ExamAssignments => Set<ExamAssignment>();

    /// <summary>
    /// 试卷提交表，存储学生提交的试卷记录
    /// </summary>
    public DbSet<ExamSubmission> ExamSubmissions => Set<ExamSubmission>();

    /// <summary>
    /// 题目答案表，存储学生提交的答案记录
    /// </summary>
    public DbSet<QuestionAnswer> QuestionAnswers => Set<QuestionAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Question>().HasData(
            new Question
            {
                Id = 1,                Type = QuestionType.Single,
                Content = "中国的首都是哪里？",
                OptionsJson = "[\"北京\", \"上海\", \"广州\", \"深圳\"]",
                AnswersJson = "[\"北京\"]",
                Analysis = "中国的首都是北京。",
                Difficulty = 1,
                CreateTime = DateTime.Now
            },
            new Question
            {
                Id = 2,
                Type = QuestionType.Judge, // 确保这里是枚举值 1
                Content = "太阳是恒星。",
                OptionsJson = "[\"正确\", \"错误\"]", // 添加选项
                AnswersJson = "[\"正确\"]",
                Analysis = "太阳是一颗恒星。",
                Difficulty = 1,
                CreateTime = DateTime.Now
            }
        );

    }
}
