
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var success = await _auth.RegisterAsync(dto.Username, dto.Password);
        return success ? Ok("注册成功") : BadRequest("用户名已存在");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var token = await _auth.LoginAsync(dto.Username, dto.Password);
        return token == null ? Unauthorized("用户名或密码错误") : Ok(new { token });
    }
}
