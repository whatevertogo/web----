/*
 * QuestionsController.cs
 * ----------------------
 * 题库管理的API控制器，提供题目的增删改查RESTful接口。
 * 负责接收HTTP请求，调用业务服务层完成具体操作。
 */

using Microsoft.AspNetCore.Mvc;
using QuestionBankApi.DTOs;
using QuestionBankApi.Services;

namespace QuestionBankApi.Controllers;

/// <summary>
/// 题目管理控制器，提供题目的增删改查API接口
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _service;

    /// <summary>
    /// 构造函数，注入题目服务
    /// </summary>
    /// <param name="service">题目服务接口</param>
    public QuestionsController(IQuestionService service)
    {
        _service = service;
    }

    /// <summary>
    /// 获取所有题目
    /// </summary>
    /// <returns>题目列表</returns>
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _service.GetAllAsync());

    /// <summary>
    /// 根据ID获取题目信息
    /// </summary>
    /// <param name="id">题目ID</param>
    /// <returns>题目信息</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var q = await _service.GetByIdAsync(id);
        return q is null ? NotFound() : Ok(q);
    }

    /// <summary>
    /// 新增题目
    /// </summary>
    /// <param name="dto">题目数据传输对象</param>
    /// <returns>创建成功的题目信息</returns>
    [HttpPost]
    public async Task<IActionResult> Post(QuestionDto dto)
    {
        var result = await _service.AddAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    /// <summary>
    /// 更新题目信息
    /// </summary>
    /// <param name="dto">题目数据传输对象</param>
    /// <returns>操作结果</returns>
    [HttpPut]
    public async Task<IActionResult> Put(QuestionDto dto)
    {
        var success = await _service.UpdateAsync(dto);
        return success ? NoContent() : NotFound();
    }

    /// <summary>
    /// 删除题目
    /// </summary>
    /// <param name="id">题目ID</param>
    /// <returns>操作结果</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
