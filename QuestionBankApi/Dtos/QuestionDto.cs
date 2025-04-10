namespace QuestionBankApi.DTOs;

public class QuestionDto
{
    public int Id { get; set; }
    public string Type { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? OptionsJson { get; set; }
    public string? AnswersJson { get; set; }
    public string? Analysis { get; set; }
    public string? ExamplesJson { get; set; }
    public string? ReferenceAnswer { get; set; }
    public string? Category { get; set; }
    public int Difficulty { get; set; }
    public string? TagsJson { get; set; }
}
