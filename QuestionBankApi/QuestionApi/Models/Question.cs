/*
 * Question.cs
 * ------------
 * 题目实体模型，对应数据库中的题目表结构。
 * 定义了题目的各种属性，包括内容、选项、答案、解析、示例、分类、难度等。
 */

using QuestionBankApi.Enums;

namespace QuestionBankApi.Models;

/// <summary>
/// 题目实体类，映射到数据库中的题目表
/// </summary>
public class Question
{
    /// <summary>
    /// 题目ID，主键
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

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }
}
