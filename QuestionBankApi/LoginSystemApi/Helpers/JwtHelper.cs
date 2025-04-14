using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuestionBankApi.LoginSystemApi.Models;

namespace QuestionBankApi.LoginSystemApi.Helpers;

/// <summary>
/// JWT 帮助类接口
/// </summary>
public interface IJwtHelper
{
    /// <summary>
    /// 生成 JWT 令牌
    /// </summary>
    /// <param name="user">用户</param>
    /// <returns>JWT 令牌</returns>
    string GenerateToken(User user);
}

/// <summary>
/// JWT 帮助类实现
/// </summary>
public class JwtHelper : IJwtHelper
{
    private readonly IConfiguration _config;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="config">配置</param>
    public JwtHelper(IConfiguration config) => _config = config;

    /// <summary>
    /// 生成 JWT 令牌
    /// </summary>
    /// <param name="user">用户</param>
    /// <returns>JWT 令牌</returns>
    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? ""));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        // 设置令牌过期时间为10分钟
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(10),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
