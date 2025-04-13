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
    private readonly QuestionBankDbContext _db;

    /// <summary>
    /// 构造函数，注入数据库上下文
    /// </summary>
    /// <param name="db">数据库上下文</param>
    public QuestionService(QuestionBankDbContext db)
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
        // 验证题目类型是否有效
        if (!Enum.IsDefined(typeof(Enums.QuestionType), dto.Type))
        {
            throw new ArgumentException($"无效的题目类型: {dto.Type}");
        }

        // 根据题目类型验证数据
        ValidateQuestionData(dto);

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

        // 验证题目类型是否有效
        if (!Enum.IsDefined(typeof(Enums.QuestionType), dto.Type))
        {
            throw new ArgumentException($"无效的题目类型: {dto.Type}");
        }

        // 根据题目类型验证数据
        ValidateQuestionData(dto);

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

    /// <summary>
    /// 验证题目数据
    /// </summary>
    /// <param name="dto">题目DTO</param>
    private static void ValidateQuestionData(QuestionDto dto)
    {
        // 验证题目内容
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            throw new ArgumentException("题目内容不能为空");
        }

        // 验证题目内容长度
        if (dto.Content.Length > 500)
        {
            throw new ArgumentException("题目内容不能超过500个字符");
        }

        // 验证题目内容安全性
        if (dto.Content.Contains("<script>") || dto.Content.Contains("javascript:"))
        {
            throw new ArgumentException("题目内容包含不安全的脚本标签");
        }

        // 根据题目类型验证
        switch (dto.Type)
        {
            case Enums.QuestionType.Single:
                ValidateSingleChoiceQuestion(dto);
                break;
            case Enums.QuestionType.Judge:
                ValidateJudgeQuestion(dto);
                break;
            case Enums.QuestionType.Fill:
                ValidateFillQuestion(dto);
                break;
            case Enums.QuestionType.Program:
                ValidateProgramQuestion(dto);
                break;
            case Enums.QuestionType.ShortAnswer:
                ValidateShortAnswerQuestion(dto);
                break;
        }
    }

    /// <summary>
    /// 验证单选题
    /// </summary>
    /// <param name="dto">题目DTO</param>
    private static void ValidateSingleChoiceQuestion(QuestionDto dto)
    {
        // 验证选项
        if (string.IsNullOrWhiteSpace(dto.OptionsJson))
        {
            throw new ArgumentException("单选题必须有选项");
        }

        // 验证答案
        if (string.IsNullOrWhiteSpace(dto.AnswersJson))
        {
            throw new ArgumentException("单选题必须有答案");
        }

        try
        {
            // 验证选项格式
            var options = System.Text.Json.JsonSerializer.Deserialize<string[]>(dto.OptionsJson);
            if (options == null || options.Length < 2)
            {
                throw new ArgumentException("单选题至少需要两个选项");
            }

            // 验证选项内容
            if (options.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("选项内容不能为空");
            }

            // 验证选项内容长度
            if (options.Any(o => o.Length > 100))
            {
                throw new ArgumentException("单个选项内容不能超过100个字符");
            }

            // 验证选项内容重复
            if (options.Length != options.Distinct().Count())
            {
                throw new ArgumentException("选项内容不能重复");
            }

            // 验证答案格式
            var answers = System.Text.Json.JsonSerializer.Deserialize<string[]>(dto.AnswersJson);
            if (answers == null || answers.Length != 1)
            {
                throw new ArgumentException("单选题必须有且仅有一个答案");
            }

            // 验证答案是否在选项中
            if (!options.Contains(answers[0]))
            {
                throw new ArgumentException("答案必须是选项之一");
            }
        }
        catch (System.Text.Json.JsonException)
        {
            throw new ArgumentException("选项或答案格式不正确");
        }
    }

    /// <summary>
    /// 验证判断题
    /// </summary>
    /// <param name="dto">题目DTO</param>
    private static void ValidateJudgeQuestion(QuestionDto dto)
    {
        // 验证答案
        if (string.IsNullOrWhiteSpace(dto.AnswersJson))
        {
            throw new ArgumentException("判断题必须有答案");
        }

        try
        {
            var answers = System.Text.Json.JsonSerializer.Deserialize<string[]>(dto.AnswersJson);
            if (answers == null || answers.Length != 1 || (answers[0] != "正确" && answers[0] != "错误"))
            {
                throw new ArgumentException("判断题答案必须是“正确”或“错误”");
            }

            // 验证选项
            if (!string.IsNullOrWhiteSpace(dto.OptionsJson))
            {
                var options = System.Text.Json.JsonSerializer.Deserialize<string[]>(dto.OptionsJson);
                if (options == null || options.Length != 2 ||
                    !options.Contains("正确") || !options.Contains("错误"))
                {
                    throw new ArgumentException("判断题选项必须是“正确”和“错误”");
                }
            }
        }
        catch (System.Text.Json.JsonException)
        {
            throw new ArgumentException("判断题答案格式不正确");
        }
    }

    /// <summary>
    /// 验证填空题
    /// </summary>
    /// <param name="dto">题目DTO</param>
    private static void ValidateFillQuestion(QuestionDto dto)
    {
        // 验证答案
        if (string.IsNullOrWhiteSpace(dto.AnswersJson))
        {
            throw new ArgumentException("填空题必须有答案");
        }
    }

    /// <summary>
    /// 验证编程题
    /// </summary>
    /// <param name="dto">题目DTO</param>
    private static void ValidateProgramQuestion(QuestionDto dto)
    {
        // 验证示例
        if (string.IsNullOrWhiteSpace(dto.ExamplesJson))
        {
            throw new ArgumentException("编程题必须有示例");
        }

        // 验证参考答案
        if (string.IsNullOrWhiteSpace(dto.ReferenceAnswer))
        {
            throw new ArgumentException("编程题必须有参考答案");
        }
    }

    /// <summary>
    /// 验证简答题
    /// </summary>
    /// <param name="dto">题目DTO</param>
    private static void ValidateShortAnswerQuestion(QuestionDto dto)
    {
        // 验证参考答案
        if (string.IsNullOrWhiteSpace(dto.ReferenceAnswer))
        {
            throw new ArgumentException("简答题必须有参考答案");
        }
    }
}
