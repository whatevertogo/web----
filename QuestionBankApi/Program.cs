using Microsoft.EntityFrameworkCore;
using QuestionBankApi.Data;
using QuestionBankApi.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 添加 CORS 服务
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        builder => builder
            .WithOrigins("http://localhost:5173") // Vue 开发服务器的默认地址
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 注册数据库上下文
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 注入服务
builder.Services.AddScoped<IQuestionService, QuestionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 启用 CORS
app.UseCors("AllowVueApp");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
