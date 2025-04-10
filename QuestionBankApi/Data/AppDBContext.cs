using Microsoft.EntityFrameworkCore;
using QuestionBankApi.Models;

namespace QuestionBankApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Question> Questions => Set<Question>();
}
