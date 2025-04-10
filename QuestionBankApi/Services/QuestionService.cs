using QuestionBankApi.DTOs;
using QuestionBankApi.Models;
using QuestionBankApi.Data;
using Microsoft.EntityFrameworkCore;

namespace QuestionBankApi.Services;

public class QuestionService : IQuestionService
{
    private readonly AppDbContext _db;

    public QuestionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<QuestionDto>> GetAllAsync()
    {
        return await _db.Questions
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                Type = q.Type,
                Content = q.Content,
                OptionsJson = q.OptionsJson,
                AnswersJson = q.AnswersJson,
                Analysis = q.Analysis,
                ExamplesJson = q.ExamplesJson,
                ReferenceAnswer = q.ReferenceAnswer,
                Category = q.Category,
                Difficulty = q.Difficulty,
                TagsJson = q.TagsJson
            }).ToListAsync();
    }

    public async Task<QuestionDto?> GetByIdAsync(int id)
    {
        var q = await _db.Questions.FindAsync(id);
        if (q == null) return null;

        return new QuestionDto
        {
            Id = q.Id,
            Type = q.Type,
            Content = q.Content,
            OptionsJson = q.OptionsJson,
            AnswersJson = q.AnswersJson,
            Analysis = q.Analysis,
            ExamplesJson = q.ExamplesJson,
            ReferenceAnswer = q.ReferenceAnswer,
            Category = q.Category,
            Difficulty = q.Difficulty,
            TagsJson = q.TagsJson
        };
    }

    public async Task<QuestionDto> AddAsync(QuestionDto dto)
    {
        var model = new Question
        {
            Type = dto.Type,
            Content = dto.Content,
            OptionsJson = dto.OptionsJson,
            AnswersJson = dto.AnswersJson,
            Analysis = dto.Analysis,
            ExamplesJson = dto.ExamplesJson,
            ReferenceAnswer = dto.ReferenceAnswer,
            Category = dto.Category,
            Difficulty = dto.Difficulty,
            TagsJson = dto.TagsJson,
            CreateTime = DateTime.UtcNow
        };

        _db.Questions.Add(model);
        await _db.SaveChangesAsync();

        dto.Id = model.Id;
        return dto;
    }

    public async Task<bool> UpdateAsync(QuestionDto dto)
    {
        var q = await _db.Questions.FindAsync(dto.Id);
        if (q == null) return false;

        q.Type = dto.Type;
        q.Content = dto.Content;
        q.OptionsJson = dto.OptionsJson;
        q.AnswersJson = dto.AnswersJson;
        q.Analysis = dto.Analysis;
        q.ExamplesJson = dto.ExamplesJson;
        q.ReferenceAnswer = dto.ReferenceAnswer;
        q.Category = dto.Category;
        q.Difficulty = dto.Difficulty;
        q.TagsJson = dto.TagsJson;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var q = await _db.Questions.FindAsync(id);
        if (q == null) return false;

        _db.Questions.Remove(q);
        await _db.SaveChangesAsync();
        return true;
    }
}
