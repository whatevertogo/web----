namespace QuestionBankApi.DTOs;

/// <summary>
/// 试卷数据传输对象
/// </summary>
public class ExamDto
{
    /// <summary>
    /// 试卷ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 试卷标题
    /// </summary>
    public string Title { get; set; } = null!;
    
    /// <summary>
    /// 试卷描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 截止时间
    /// </summary>
    public DateTime? Deadline { get; set; }
    
    /// <summary>
    /// 总分
    /// </summary>
    public int TotalScore { get; set; }
    
    /// <summary>
    /// 试卷状态：0-草稿，1-已发布，2-已结束
    /// </summary>
    public int Status { get; set; } = 0;
    
    /// <summary>
    /// 试卷中的题目列表
    /// </summary>
    public List<ExamQuestionDto> Questions { get; set; } = new List<ExamQuestionDto>();
}

/// <summary>
/// 试卷中的题目数据传输对象
/// </summary>
public class ExamQuestionDto
{
    /// <summary>
    /// 题目ID
    /// </summary>
    public int QuestionId { get; set; }
    
    /// <summary>
    /// 题目在试卷中的序号
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// 题目分值
    /// </summary>
    public int Score { get; set; }
    
    /// <summary>
    /// 题目详情（包含题干、选项等）
    /// </summary>
    public QuestionDto? Question { get; set; }
}

/// <summary>
/// 试卷分配数据传输对象
/// </summary>
public class ExamAssignmentDto
{
    /// <summary>
    /// 学生ID列表
    /// </summary>
    public List<int> StudentIds { get; set; } = new List<int>();
}

/// <summary>
/// 试卷提交数据传输对象
/// </summary>
public class ExamSubmissionDto
{
    /// <summary>
    /// 答案列表
    /// </summary>
    public List<QuestionAnswerDto> Answers { get; set; } = new List<QuestionAnswerDto>();
    
    /// <summary>
    /// 完成时间（分钟）
    /// </summary>
    public int CompletionTime { get; set; }
}

/// <summary>
/// 题目答案数据传输对象
/// </summary>
public class QuestionAnswerDto
{
    /// <summary>
    /// 题目ID
    /// </summary>
    public int QuestionId { get; set; }
    
    /// <summary>
    /// 学生答案
    /// </summary>
    public string Answer { get; set; } = null!;
    
    /// <summary>
    /// 是否正确
    /// </summary>
    public bool IsCorrect { get; set; }
    
    /// <summary>
    /// 得分
    /// </summary>
    public int Score { get; set; }
}

/// <summary>
/// 试卷成绩数据传输对象
/// </summary>
public class ExamResultDto
{
    /// <summary>
    /// 试卷ID
    /// </summary>
    public int ExamId { get; set; }
    
    /// <summary>
    /// 学生ID
    /// </summary>
    public int StudentId { get; set; }
    
    /// <summary>
    /// 学生用户名
    /// </summary>
    public string? StudentName { get; set; }
    
    /// <summary>
    /// 提交时间
    /// </summary>
    public DateTime SubmittedAt { get; set; }
    
    /// <summary>
    /// 完成时间（分钟）
    /// </summary>
    public int CompletionTime { get; set; }
    
    /// <summary>
    /// 总分
    /// </summary>
    public int TotalScore { get; set; }
    
    /// <summary>
    /// 得分
    /// </summary>
    public int Score { get; set; }
    
    /// <summary>
    /// 正确题目数
    /// </summary>
    public int CorrectCount { get; set; }
    
    /// <summary>
    /// 题目总数
    /// </summary>
    public int QuestionCount { get; set; }
    
    /// <summary>
    /// 答案详情
    /// </summary>
    public List<QuestionAnswerDto> Answers { get; set; } = new List<QuestionAnswerDto>();
}

/// <summary>
/// 试卷统计数据传输对象
/// </summary>
public class ExamStatisticsDto
{
    /// <summary>
    /// 试卷ID
    /// </summary>
    public int ExamId { get; set; }
    
    /// <summary>
    /// 试卷标题
    /// </summary>
    public string? ExamTitle { get; set; }
    
    /// <summary>
    /// 参与学生数
    /// </summary>
    public int StudentCount { get; set; }
    
    /// <summary>
    /// 已提交数
    /// </summary>
    public int SubmittedCount { get; set; }
    
    /// <summary>
    /// 平均分
    /// </summary>
    public double AverageScore { get; set; }
    
    /// <summary>
    /// 最高分
    /// </summary>
    public int HighestScore { get; set; }
    
    /// <summary>
    /// 最低分
    /// </summary>
    public int LowestScore { get; set; }
    
    /// <summary>
    /// 及格率
    /// </summary>
    public double PassRate { get; set; }
    
    /// <summary>
    /// 各分数段人数
    /// </summary>
    public Dictionary<string, int> ScoreDistribution { get; set; } = new Dictionary<string, int>();
    
    /// <summary>
    /// 各题正确率
    /// </summary>
    public List<QuestionStatisticsDto> QuestionStatistics { get; set; } = new List<QuestionStatisticsDto>();
    
    /// <summary>
    /// 学生成绩列表
    /// </summary>
    public List<ExamResultDto> StudentResults { get; set; } = new List<ExamResultDto>();
}

/// <summary>
/// 题目统计数据传输对象
/// </summary>
public class QuestionStatisticsDto
{
    /// <summary>
    /// 题目ID
    /// </summary>
    public int QuestionId { get; set; }
    
    /// <summary>
    /// 题目序号
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// 题目类型
    /// </summary>
    public int Type { get; set; }
    
    /// <summary>
    /// 题目内容
    /// </summary>
    public string? Content { get; set; }
    
    /// <summary>
    /// 正确人数
    /// </summary>
    public int CorrectCount { get; set; }
    
    /// <summary>
    /// 错误人数
    /// </summary>
    public int IncorrectCount { get; set; }
    
    /// <summary>
    /// 正确率
    /// </summary>
    public double CorrectRate { get; set; }
    
    /// <summary>
    /// 各选项选择人数（选择题）
    /// </summary>
    public Dictionary<string, int>? OptionCounts { get; set; }
}
