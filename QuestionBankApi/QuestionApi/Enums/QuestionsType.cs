/*
 * QuestionsType.cs
 * -----------------
 * 定义题目的类型枚举。
 * 用于区分不同类型的题目，如选择题、判断题、编程题等。
 */

namespace QuestionBankApi.Enums;

/// <summary>
/// 题目类型枚举
/// </summary>
public enum QuestionType
{
    /// <summary>
    /// 单选题
    /// </summary>
    Single,

    /// <summary>
    /// 判断题
    /// </summary>
    Judge,

    /// <summary>
    /// 填空题
    /// </summary>
    Fill,

    /// <summary>
    /// 编程题
    /// </summary>
    Program,

    /// <summary>
    /// 简答题
    /// </summary>
    ShortAnswer
}