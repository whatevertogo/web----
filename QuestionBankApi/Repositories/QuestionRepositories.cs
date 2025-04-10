/*
 * QuestionRepository.cs
 * ---------------------
 * 题目仓储实现类，实现IQuestionRepository接口。
 * 负责具体操作数据库中的题目数据，封装EF Core的调用。
 */

using Microsoft.EntityFrameworkCore;
using QuestionBankApi.Data;
using QuestionBankApi.Models;

/// <summary>
/// 题目仓储实现，封装EF Core数据库操作
/// </summary>
public class QuestionRepository : IQuestionRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// 构造函数，注入数据库上下文
    /// </summary>
    /// <param name="context">数据库上下文实例</param>
    public QuestionRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 异步获取所有题目
    /// </summary>
    /// <returns>题目列表</returns>
    public async Task<List<Question>> GetAllQuestionsAsync()
    {
        return await _context.Questions.ToListAsync();
    }

    /// <summary>
    /// 根据ID获取题目
    /// </summary>
    /// <param name="id">题目ID</param>
    /// <returns>题目实体</returns>
    /// <exception cref="KeyNotFoundException">未找到题目时抛出</exception>
    public async Task<Question> GetQuestionByIdAsync(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
        {
            throw new KeyNotFoundException($"Question with ID {id} not found.");
        }
        return question;
    }

    /// <summary>
    /// 异步添加题目
    /// </summary>
    /// <param name="question">题目实体</param>
    /// <returns>异步任务</returns>
    public async Task AddQuestionAsync(Question question)
    {
        await _context.Questions.AddAsync(question);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 异步更新题目
    /// </summary>
    /// <param name="question">题目实体</param>
    /// <returns>异步任务</returns>
    public async Task UpdateQuestionAsync(Question question)
    {
        _context.Questions.Update(question);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 异步删除题目
    /// </summary>
    /// <param name="id">题目ID</param>
    /// <returns>异步任务</returns>
    public async Task DeleteQuestionAsync(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question != null)
        {
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
        }
    }
}