/*
 * IQuestionService.cs
 * -------------------
 * 题目服务接口，定义题目的业务逻辑操作。
 * 负责抽象题目的增删改查业务，供控制器调用。
 */

using QuestionBankApi.DTOs;

namespace QuestionBankApi.Services;

/// <summary>
/// 题目服务接口，定义题目的业务逻辑操作
/// </summary>
public interface IQuestionService
{
    /// <summary>
    /// 获取所有题目
    /// </summary>
    /// <returns>题目DTO列表</returns>
    Task<List<QuestionDto>> GetAllAsync();

    /// <summary>
    /// 根据条件筛选题目
    /// </summary>
    /// <param name="keyword">关键词</param>
    /// <param name="type">题型</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>题目DTO列表</returns>
    Task<List<QuestionDto>> GetAllAsync(string? keyword, int? type, DateTime? startDate, DateTime? endDate);

    /// <summary>
    /// 根据ID获取题目
    /// </summary>
    /// <param name="id">题目ID</param>
    /// <returns>题目DTO，未找到返回null</returns>
    Task<QuestionDto?> GetByIdAsync(int id);

    /// <summary>
    /// 添加题目
    /// </summary>
    /// <param name="dto">题目DTO</param>
    /// <returns>添加后的题目DTO</returns>
    Task<QuestionDto> AddAsync(QuestionDto dto);

    /// <summary>
    /// 更新题目
    /// </summary>
    /// <param name="dto">题目DTO</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateAsync(QuestionDto dto);

    /// <summary>
    /// 删除题目
    /// </summary>
    /// <param name="id">题目ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteAsync(int id);
}
