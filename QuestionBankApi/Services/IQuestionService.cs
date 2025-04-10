using QuestionBankApi.DTOs;

namespace QuestionBankApi.Services;

public interface IQuestionService
{
    Task<List<QuestionDto>> GetAllAsync();
    Task<QuestionDto?> GetByIdAsync(int id);
    Task<QuestionDto> AddAsync(QuestionDto dto);
    Task<bool> UpdateAsync(QuestionDto dto);
    Task<bool> DeleteAsync(int id);
}
