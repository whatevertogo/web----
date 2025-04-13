using QuestionBankApi.DTOs;

namespace QuestionBankApi.Services;

/// <summary>
/// 试卷服务接口
/// </summary>
public interface IExamService
{
    /// <summary>
    /// 创建试卷
    /// </summary>
    /// <param name="dto">试卷数据传输对象</param>
    /// <returns>创建的试卷</returns>
    Task<ExamDto> CreateExamAsync(ExamDto dto);
    
    /// <summary>
    /// 获取所有试卷
    /// </summary>
    /// <returns>试卷列表</returns>
    Task<IEnumerable<ExamDto>> GetAllExamsAsync();
    
    /// <summary>
    /// 根据ID获取试卷
    /// </summary>
    /// <param name="id">试卷ID</param>
    /// <returns>试卷信息</returns>
    Task<ExamDto?> GetExamByIdAsync(int id);
    
    /// <summary>
    /// 获取学生的试卷列表
    /// </summary>
    /// <param name="studentId">学生ID</param>
    /// <returns>试卷列表</returns>
    Task<IEnumerable<ExamDto>> GetExamsByStudentIdAsync(int studentId);
    
    /// <summary>
    /// 分配试卷给学生
    /// </summary>
    /// <param name="examId">试卷ID</param>
    /// <param name="studentIds">学生ID列表</param>
    /// <returns>分配结果</returns>
    Task<bool> AssignExamToStudentsAsync(int examId, List<int> studentIds);
    
    /// <summary>
    /// 检查试卷是否分配给学生
    /// </summary>
    /// <param name="examId">试卷ID</param>
    /// <param name="studentId">学生ID</param>
    /// <returns>是否分配</returns>
    Task<bool> IsExamAssignedToStudentAsync(int examId, int studentId);
    
    /// <summary>
    /// 提交试卷
    /// </summary>
    /// <param name="examId">试卷ID</param>
    /// <param name="studentId">学生ID</param>
    /// <param name="dto">提交数据</param>
    /// <returns>提交结果</returns>
    Task<ExamResultDto> SubmitExamAsync(int examId, int studentId, ExamSubmissionDto dto);
    
    /// <summary>
    /// 获取试卷统计信息
    /// </summary>
    /// <param name="examId">试卷ID</param>
    /// <returns>统计信息</returns>
    Task<ExamStatisticsDto> GetExamStatisticsAsync(int examId);
    
    /// <summary>
    /// 获取试卷成绩
    /// </summary>
    /// <param name="examId">试卷ID</param>
    /// <param name="studentId">学生ID（可选）</param>
    /// <returns>成绩信息</returns>
    Task<IEnumerable<ExamResultDto>> GetExamResultsAsync(int examId, int? studentId = null);
}
