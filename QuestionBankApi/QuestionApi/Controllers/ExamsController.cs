/*
 * ExamsController.cs
 * -----------------
 * 试卷管理的API控制器，提供试卷的创建、分发、提交答案和成绩统计等功能。
 * 负责接收HTTP请求，调用业务服务层完成具体操作。
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionBankApi.DTOs;
using QuestionBankApi.Services;

namespace QuestionBankApi.Controllers;

/// <summary>
/// 试卷管理控制器，提供试卷的创建、分发、提交答案和成绩统计等功能
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // 需要登录才能访问
public class ExamsController : ControllerBase
{
    private readonly IExamService _examService;
    private readonly IQuestionService _questionService;

    /// <summary>
    /// 构造函数，注入试卷服务和题目服务
    /// </summary>
    /// <param name="examService">试卷服务接口</param>
    /// <param name="questionService">题目服务接口</param>
    public ExamsController(IExamService examService, IQuestionService questionService)
    {
        _examService = examService;
        _questionService = questionService;
    }

    /// <summary>
    /// 创建新试卷（仅管理员）
    /// </summary>
    /// <param name="dto">试卷数据传输对象</param>
    /// <returns>创建成功的试卷信息</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExamDto>> CreateExam([FromBody] ExamDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "请求数据无效", errors = ModelState });

            var result = await _examService.CreateExamAsync(dto);
            return CreatedAtAction(nameof(GetExam), new { id = result.Id },
                new { success = true, data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "创建试卷失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 获取试卷列表
    /// </summary>
    /// <returns>试卷列表</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ExamDto>>> GetExams()
    {
        try
        {
            // 根据用户角色返回不同的试卷列表
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "未授权访问" });

            var exams = role == "Admin"
                ? await _examService.GetAllExamsAsync() // 管理员可以看到所有试卷
                : await _examService.GetExamsByStudentIdAsync(int.Parse(userId)); // 学生只能看到分配给自己的试卷

            return Ok(new { success = true, data = exams });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "获取试卷列表失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 根据ID获取试卷信息
    /// </summary>
    /// <param name="id">试卷ID</param>
    /// <returns>试卷信息</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExamDto>> GetExam(int id)
    {
        try
        {
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
                return NotFound(new { success = false, message = $"未找到ID为{id}的试卷" });

            // 检查权限：管理员可以查看所有试卷，学生只能查看分配给自己的试卷
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (role != "Admin" && !string.IsNullOrEmpty(userId) && !await _examService.IsExamAssignedToStudentAsync(id, int.Parse(userId)))
                return Forbid();

            return Ok(new { success = true, data = exam });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "获取试卷失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 分配试卷给学生（仅管理员）
    /// </summary>
    /// <param name="id">试卷ID</param>
    /// <param name="dto">分配信息</param>
    /// <returns>操作结果</returns>
    [HttpPost("{id}/assign")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignExam(int id, [FromBody] ExamAssignmentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "请求数据无效", errors = ModelState });

            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
                return NotFound(new { success = false, message = $"未找到ID为{id}的试卷" });

            var result = await _examService.AssignExamToStudentsAsync(id, dto.StudentIds);
            return Ok(new { success = true, message = "试卷分配成功", data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "分配试卷失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 提交试卷答案（仅学生）
    /// </summary>
    /// <param name="id">试卷ID</param>
    /// <param name="dto">答案信息</param>
    /// <returns>操作结果</returns>
    [HttpPost("{id}/submit")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SubmitExam(int id, [FromBody] ExamSubmissionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "请求数据无效", errors = ModelState });

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "未授权访问" });

            // 检查试卷是否存在
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
                return NotFound(new { success = false, message = $"未找到ID为{id}的试卷" });

            // 检查试卷是否分配给当前学生
            if (string.IsNullOrEmpty(userId) || !await _examService.IsExamAssignedToStudentAsync(id, int.Parse(userId)))
                return Forbid();

            // 提交答案并计算成绩
            var result = await _examService.SubmitExamAsync(id, int.Parse(userId), dto);
            return Ok(new { success = true, message = "试卷提交成功", data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "提交试卷失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 获取试卷成绩统计（仅管理员）
    /// </summary>
    /// <param name="id">试卷ID</param>
    /// <returns>成绩统计信息</returns>
    [HttpGet("{id}/statistics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExamStatisticsDto>> GetExamStatistics(int id)
    {
        try
        {
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
                return NotFound(new { success = false, message = $"未找到ID为{id}的试卷" });

            var statistics = await _examService.GetExamStatisticsAsync(id);
            return Ok(new { success = true, data = statistics });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "获取成绩统计失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 获取学生的试卷成绩（管理员可查看所有学生，学生只能查看自己）
    /// </summary>
    /// <param name="id">试卷ID</param>
    /// <param name="studentId">学生ID（可选，仅管理员可用）</param>
    /// <returns>学生成绩信息</returns>
    [HttpGet("{id}/results")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExamResultDto>> GetExamResults(int id, [FromQuery] int? studentId = null)
    {
        try
        {
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
                return NotFound(new { success = false, message = $"未找到ID为{id}的试卷" });

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "未授权访问" });

            // 如果是学生，只能查看自己的成绩
            if (role != "Admin")
            {
                studentId = int.Parse(userId);
            }

            // 获取成绩
            var results = await _examService.GetExamResultsAsync(id, studentId);
            return Ok(new { success = true, data = results });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "获取成绩失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新试卷信息（仅管理员）
    /// </summary>
    /// <param name="id">试卷ID</param>
    /// <param name="dto">更新的试卷数据</param>
    /// <returns>操作结果</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)] // 成功且无内容返回
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateExam(int id, [FromBody] ExamDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { success = false, message = "请求路径中的ID与请求体中的ID不匹配" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "请求数据无效", errors = ModelState });
        }

        try
        {
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
            {
                return NotFound(new { success = false, message = $"未找到ID为{id}的试卷" });
            }

            var success = await _examService.UpdateExamAsync(dto);
            if (!success)
            {
                 // 这里可以添加更具体的错误信息，如果服务层能提供的话
                return StatusCode(500, new { success = false, message = "更新试卷时发生未知错误" });
            }

            return NoContent(); // 成功更新，返回 204 No Content
        }
        catch (Exception ex)
        {
            // 考虑添加更具体的异常处理，例如处理并发冲突
            return StatusCode(500, new { success = false, message = "更新试卷失败", error = ex.Message });
        }
    }
}
