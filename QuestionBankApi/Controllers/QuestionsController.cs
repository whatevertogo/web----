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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<QuestionDto>>> Get(
        string? keyword,
        int? type,
        DateTime? startDate,
        DateTime? endDate)
    {
        try
        {
            var questions = await _service.GetAllAsync(keyword, type, startDate, endDate);
            return Ok(new { success = true, data = questions });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "获取题目列表失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 根据ID获取题目信息
    /// </summary>
    /// <param name="id">题目ID</param>
    /// <returns>题目信息</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuestionDto>> Get(int id)
    {
        try
        {
            var question = await _service.GetByIdAsync(id);
            if (question == null)
                return NotFound(new { success = false, message = $"未找到ID为{id}的题目" });

            return Ok(new { success = true, data = question });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "获取题目失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 新增题目
    /// </summary>
    /// <param name="dto">题目数据传输对象</param>
    /// <returns>创建成功的题目信息</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuestionDto>> Post([FromBody] QuestionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "请求数据无效", errors = ModelState });

            var result = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, 
                new { success = true, data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "创建题目失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新题目信息
    /// </summary>
    /// <param name="dto">题目数据传输对象</param>
    /// <returns>操作结果</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Put([FromBody] QuestionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "请求数据无效", errors = ModelState });

            var success = await _service.UpdateAsync(dto);
            if (!success)
                return NotFound(new { success = false, message = $"未找到ID为{dto.Id}的题目" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "更新题目失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 删除题目
    /// </summary>
    /// <param name="id">题目ID</param>
    /// <returns>操作结果</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound(new { success = false, message = $"未找到ID为{id}的题目" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "删除题目失败", error = ex.Message });
        }
    }
}
