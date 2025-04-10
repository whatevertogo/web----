using Microsoft.EntityFrameworkCore;
using QuestionBankApi.Data;
using QuestionBankApi.Models;

public class QuestionRepository : IQuestionRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// 依赖注入AppDbContext
    /// </summary>
    /// <param name="context">数据库上下文实例</param>
    public QuestionRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 异步获取所有问题
    /// </summary>
    /// <returns></returns>
    public async Task<List<Question>> GetAllQuestionsAsync()
    {
        return await _context.Questions.ToListAsync();
    }

    public async Task<Question> GetQuestionByIdAsync(int id)
    {
        //todo
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
        {
            throw new KeyNotFoundException($"Question with ID {id} not found.");
        }
        return question;
    }

    public async Task AddQuestionAsync(Question question)
    {
        await _context.Questions.AddAsync(question);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateQuestionAsync(Question question)
    {
        _context.Questions.Update(question);
        await _context.SaveChangesAsync();
    }

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