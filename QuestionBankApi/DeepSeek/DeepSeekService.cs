/*
 * DeepSeekService.cs
 * ------------------
 * DeepSeek AI 服务实现类。
 * 负责封装与 DeepSeek API 的通信逻辑。
 */

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace QuestionBankApi.DeepSeek;

/// <summary>
/// DeepSeek AI 服务，封装与 DeepSeek API 的通信
/// </summary>
public class DeepSeekService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<DeepSeekService> _logger;

    /// <summary>
    /// 构造函数，注入配置和日志服务
    /// </summary>
    /// <param name="config">配置服务</param>
    /// <param name="logger">日志服务</param>
    public DeepSeekService(IConfiguration config, ILogger<DeepSeekService> logger)
    {
        _httpClient = new HttpClient();
        _logger = logger;

        // 从配置中获取 API 密钥和模型名称
        _apiKey = config.GetValue<string>("DeepSeek:ApiKey") ?? config["DeepSeek:ApiKey"];
        _model = config.GetValue<string>("DeepSeek:Model") ?? config["DeepSeek:Model"] ?? "deepseek-chat";

        // 记录密钥信息（注意不要记录完整密钥）
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("未找到 DeepSeek API 密钥配置，将使用模拟响应");
            _apiKey = "sk-demo-key";
        }
        else
        {
            _logger.LogInformation("找到 DeepSeek API 密钥配置，密钥前缀: {Prefix}", _apiKey.Substring(0, Math.Min(8, _apiKey.Length)));
        }

        _logger.LogInformation("DeepSeek 服务已初始化，使用模型: {Model}", _model);
    }

    /// <summary>
    /// 与 DeepSeek AI 聊天
    /// </summary>
    /// <param name="userMessage">用户消息</param>
    /// <param name="model">模型名称（可选）</param>
    /// <returns>AI 回复</returns>
    public async Task<string> ChatAsync(string userMessage, string? model = null)
    {
        try
        {
            _logger.LogInformation("准备发送消息到 DeepSeek AI，模型: {Model}", model ?? _model);

            // 模拟响应，用于测试
            if (_apiKey == "sk-demo-key")
            {
                _logger.LogWarning("使用演示密钥，返回模拟响应");
                await Task.Delay(1000); // 模拟网络延迟
                return $"这是一个模拟响应。您发送的消息是: \"{userMessage}\"。请在 appsettings.json 中配置有效的 DeepSeek API 密钥。";
            }

            // 构建请求
            var url = "https://api.deepseek.com/v1/chat/completions";
            var requestBody = new
            {
                model = model ?? _model, // 优先使用参数，否则使用默认值
                messages = new[]
                {
                    new { role = "user", content = userMessage }
                }
            };

            // 序列化请求体
            var json = JsonSerializer.Serialize(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            // 发送请求
            _logger.LogInformation("发送请求到 DeepSeek API");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();

            // 解析响应
            using var doc = JsonDocument.Parse(responseString);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            _logger.LogInformation("收到 DeepSeek AI 回复，长度: {Length} 字符", content?.Length ?? 0);
            return content ?? "无回复";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeepSeek AI 聊天请求失败");
            throw;
        }
    }
}
