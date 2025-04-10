using Microsoft.EntityFrameworkCore;
using QuestionBankApi.Data;
using QuestionBankApi.Models;

public class QuestionRepository : IQuestionRepository
{
    private readonly AppDbContext _context;

    public QuestionRepository(AppDbContext context)
    {
        _context = context;
    }

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