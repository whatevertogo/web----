/*
 * QuestionService.cs
 * ------------------
 * 题目服务实现类，实现IQuestionService接口。
 * 负责封装题目的业务逻辑，调用数据库上下文进行数据操作。
 */

using QuestionBankApi.DTOs;
using QuestionBankApi.Models;
using QuestionBankApi.Data;
using Microsoft.EntityFrameworkCore;

namespace QuestionBankApi.Services;

/// <summary>
/// 题目服务实现，封装题目的业务逻辑
/// </summary>
public class QuestionService : IQuestionService
{
    private readonly AppDbContext _db;

    /// <summary>
    /// 构造函数，注入数据库上下文
    /// </summary>
    /// <param name="db">数据库上下文</param>
    public QuestionService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 获取所有题目
    /// </summary>
    /// <returns>题目DTO列表</returns>
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
                TagsJson = q.TagsJson,
                CreateTime = q.CreateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
            }).ToListAsync();
    }

    public async Task<List<QuestionDto>> GetAllAsync(string? keyword, int? type, DateTime? startDate, DateTime? endDate)
    {
        var query = _db.Questions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(q => q.Content.Contains(keyword));
        }

        if (type.HasValue)
        {
            query = query.Where(q => (int)q.Type == type.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(q => q.CreateTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(q => q.CreateTime <= endDate.Value);
        }

        return await query
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
                TagsJson = q.TagsJson,
                CreateTime = q.CreateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
            }).ToListAsync();
    }

    /// <summary>
    /// 根据ID获取题目
    /// </summary>
    /// <param name="id">题目ID</param>
    /// <returns>题目DTO，未找到返回null</returns>
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
            TagsJson = q.TagsJson,
            CreateTime = q.CreateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
        };
    }

    /// <summary>
    /// 添加题目
    /// </summary>
    /// <param name="dto">题目DTO</param>
    /// <returns>添加后的题目DTO</returns>
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
        dto.CreateTime = model.CreateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        return dto;
    }

    /// <summary>
    /// 更新题目
    /// </summary>
    /// <param name="dto">题目DTO</param>
    /// <returns>是否成功</returns>
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

    /// <summary>
    /// 删除题目
    /// </summary>
    /// <param name="id">题目ID</param>
    /// <returns>是否成功</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var q = await _db.Questions.FindAsync(id);
        if (q == null) return false;

        _db.Questions.Remove(q);
        await _db.SaveChangesAsync();
        return true;
    }
}
