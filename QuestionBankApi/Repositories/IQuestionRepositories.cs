using QuestionBankApi.Models;

public interface IQuestionRepository
{
    /// <summary>
    /// 异步获取所有问题
    /// </summary>
    /// <returns></returns>
    Task<List<Question>> GetAllQuestionsAsync();
    /// <summary>
    /// 通过id得到问题
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    Task<Question> GetQuestionByIdAsync(int id);
    /// <summary>
    /// 异步添加问题
    /// </summary>
    /// <param name="question"></param>
    /// <returns></returns>
    Task AddQuestionAsync(Question question);
    /// <summary>
    /// 异步更新问题
    /// </summary>
    /// <param name="question"></param>
    /// <returns></returns>
    Task UpdateQuestionAsync(Question question);
    /// <summary>
    /// 异步删除问题
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DeleteQuestionAsync(int id);
}