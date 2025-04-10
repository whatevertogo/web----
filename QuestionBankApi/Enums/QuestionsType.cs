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
    SingleChoice,

    /// <summary>
    /// 多选题
    /// </summary>
    MultipleChoice,

    /// <summary>
    /// 判断题
    /// </summary>
    TrueFalse,

    /// <summary>
    /// 填空题
    /// </summary>
    FillInTheBlank,

    /// <summary>
    /// 简答题
    /// </summary>
    ShortAnswer,

    /// <summary>
    /// 论述题
    /// </summary>
    Essay,

    /// <summary>
    /// 代码补全题
    /// </summary>
    CodeCompletion,

    /// <summary>
    /// 代码调试题
    /// </summary>
    CodeDebugging,

    /// <summary>
    /// 代码输出题
    /// </summary>
    CodeOutput,

    /// <summary>
    /// 代码分析题
    /// </summary>
    CodeAnalysis
}