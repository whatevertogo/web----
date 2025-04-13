
public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtHelper _jwt;

    public AuthService(AppDbContext db, IJwtHelper jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;
        return _jwt.GenerateToken(user);
    }

    public async Task<bool> RegisterAsync(string username, string password)
    {
        if (await _db.Users.AnyAsync(u => u.Username == username)) return false;
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        _db.Users.Add(new User { Username = username, PasswordHash = hash });
        await _db.SaveChangesAsync();
        return true;
    }
}
