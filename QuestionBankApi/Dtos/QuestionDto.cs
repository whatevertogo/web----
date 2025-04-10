/*
 * QuestionDto.cs
 * ---------------
 * 题目的数据传输对象（DTO）。
 * 用于前后端或不同层之间传递题目信息，避免直接暴露数据库实体。
 */

using QuestionBankApi.Enums;

namespace QuestionBankApi.DTOs;

/// <summary>
/// 题目数据传输对象，用于接口传输
/// </summary>
public class QuestionDto
{
    /// <summary>
    /// 题目ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 题目类型（枚举）
    /// </summary>
    public QuestionType Type { get; set; }

    /// <summary>
    /// 题目内容
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// 题目选项，JSON格式字符串
    /// </summary>
    public string? OptionsJson { get; set; }

    /// <summary>
    /// 答案，JSON格式字符串
    /// </summary>
    public string? AnswersJson { get; set; }

    /// <summary>
    /// 题目解析
    /// </summary>
    public string? Analysis { get; set; }

    /// <summary>
    /// 示例数据，JSON格式字符串
    /// </summary>
    public string? ExamplesJson { get; set; }

    /// <summary>
    /// 参考答案
    /// </summary>
    public string? ReferenceAnswer { get; set; }

    /// <summary>
    /// 题目分类
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// 难度等级
    /// </summary>
    public int Difficulty { get; set; }

    /// <summary>
    /// 标签，JSON格式字符串
    /// </summary>
    public string? TagsJson { get; set; }
}
