using QuestionBankApi.Models;

public interface IQuestionRepository
{
    Task<List<Question>> GetAllQuestionsAsync();
    Task<Question> GetQuestionByIdAsync(int id);
    Task AddQuestionAsync(Question question);
    Task UpdateQuestionAsync(Question question);
    Task DeleteQuestionAsync(int id);
}