/*
 * Program.cs
 * -------------
 * ASP.NET Core Web API 的应用程序入口。
 * 负责配置服务（依赖注入）、数据库连接、Swagger文档、CORS策略，并启动Web服务器。
 *
 * 关键功能：
 * - 配置允许Vue前端跨域访问
 * - 注册数据库上下文AppDbContext
 * - 注册题目服务IQuestionService
 * - 启用Swagger API文档
 * - 映射所有控制器路由
 */

using Microsoft.EntityFrameworkCore;
using QuestionBankApi.Data;
using QuestionBankApi.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 添加 CORS 服务，允许前端跨域请求
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        builder => builder
            .WithOrigins("https://localhost:5173") // Vue开发服务器的HTTPS地址
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// 注册控制器
builder.Services.AddControllers();

// 注册API文档生成器
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 注册数据库上下文，使用mySQL，连接字符串来自配置文件
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// 注入题目服务
builder.Services.AddScoped<IQuestionService, QuestionService>();

var app = builder.Build();

// 如果是开发环境，启用Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 启用CORS策略
app.UseCors("AllowVueApp");

// 启用HTTPS重定向
app.UseHttpsRedirection();

// 启用授权中间件
app.UseAuthorization();

// 映射控制器路由
app.MapControllers();

// 启动Web应用
app.Run();
