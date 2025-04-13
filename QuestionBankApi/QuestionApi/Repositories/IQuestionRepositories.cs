/*
 * IQuestionRepository.cs
 * ----------------------
 * 题目仓储接口，定义题目的数据访问操作。
 * 负责抽象数据库的增删改查方法，供仓储实现类调用。
 */

using QuestionBankApi.Models;

/// <summary>
/// 题目仓储接口，定义题目的数据访问操作
/// </summary>
public interface IQuestionRepository
{
    /// <summary>
    /// 异步获取所有题目
    /// </summary>
    /// <returns>题目列表</returns>
    Task<List<Question>> GetAllQuestionsAsync();

    /// <summary>
    /// 根据ID获取题目
    /// </summary>
    /// <param name="id">题目ID</param>
    /// <returns>题目实体</returns>
    /// <exception cref="KeyNotFoundException">未找到题目时抛出</exception>
    Task<Question> GetQuestionByIdAsync(int id);

    /// <summary>
    /// 异步添加题目
    /// </summary>
    /// <param name="question">题目实体</param>
    /// <returns>异步任务</returns>
    Task AddQuestionAsync(Question question);

    /// <summary>
    /// 异步更新题目
    /// </summary>
    /// <param name="question">题目实体</param>
    /// <returns>异步任务</returns>
    Task UpdateQuestionAsync(Question question);

    /// <summary>
    /// 异步删除题目
    /// </summary>
    /// <param name="id">题目ID</param>
    /// <returns>异步任务</returns>
    Task DeleteQuestionAsync(int id);
}