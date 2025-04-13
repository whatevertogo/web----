/*
 * UsersController.cs
 * -----------------
 * 用户管理的API控制器，提供用户相关的功能。
 * 负责接收HTTP请求，调用业务服务层完成具体操作。
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionBankApi.LoginSystemApi.Data;
using QuestionBankApi.LoginSystemApi.Models;

namespace QuestionBankApi.Controllers;

/// <summary>
/// 用户管理控制器，提供用户相关的功能
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // 需要登录才能访问
public class UsersController : ControllerBase
{
    private readonly UserDBContext _userDb;

    /// <summary>
    /// 构造函数，注入用户数据库上下文
    /// </summary>
    /// <param name="userDb">用户数据库上下文</param>
    public UsersController(UserDBContext userDb)
    {
        _userDb = userDb;
    }

    /// <summary>
    /// 获取所有学生用户（仅管理员可用）
    /// </summary>
    /// <returns>学生用户列表</returns>
    [HttpGet("students")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStudents()
    {
        try
        {
            // 获取所有角色为Student的用户
            var students = await _userDb.Users
                .Where(u => u.Role == UserRole.Student)
                .Select(u => new
                {
                    id = u.Id,
                    username = u.Username,
                    name = u.Username // 如果没有name字段，可以暂时使用username
                })
                .ToListAsync();

            return Ok(new { success = true, data = students });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "获取学生列表失败", error = ex.Message });
        }
    }
}
