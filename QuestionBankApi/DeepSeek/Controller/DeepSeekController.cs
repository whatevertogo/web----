/*
 * DeepSeekController.cs
 * -----------------
 * DeepSeek AI 的API控制器，提供与 DeepSeek AI 交互的功能。
 * 负责接收HTTP请求，调用 DeepSeek 服务完成具体操作。
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QuestionBankApi.DeepSeek.Controller;

/// <summary>
/// DeepSeek AI 控制器，提供与 DeepSeek AI 交互的功能
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // 所有已登录用户都可以访问，包括学生和管理员
public class DeepSeekController : ControllerBase
{
    private readonly DeepSeekService _deepSeekService;

    /// <summary>
    /// 构造函数，注入 DeepSeek 服务
    /// </summary>
    /// <param name="deepSeekService">DeepSeek 服务</param>
    public DeepSeekController(DeepSeekService deepSeekService)
    {
        _deepSeekService = deepSeekService;
    }

    /// <summary>
    /// 聊天请求模型
    /// </summary>
    public class ChatRequest
    {
        /// <summary>
        /// 用户消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 模型名称（可选）
        /// </summary>
        public string? Model { get; set; }
    }

    /// <summary>
    /// 与 DeepSeek AI 聊天
    /// </summary>
    /// <param name="req">聊天请求</param>
    /// <returns>AI 回复</returns>
    [HttpPost("chat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Chat([FromBody] ChatRequest req)
    {
        try
        {
            if (string.IsNullOrEmpty(req.Message))
            {
                return BadRequest(new { success = false, message = "消息不能为空" });
            }

            var reply = await _deepSeekService.ChatAsync(req.Message, req.Model);
            return Ok(new { success = true, reply });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "AI 聊天请求失败", error = ex.Message });
        }
    }
}
