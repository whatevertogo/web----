namespace QuestionBankApi.Models;

/// <summary>
/// 试卷实体类
/// </summary>
public class Exam
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
    /// 试卷中的题目
    /// </summary>
    public ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
    
    /// <summary>
    /// 试卷分配
    /// </summary>
    public ICollection<ExamAssignment> Assignments { get; set; } = new List<ExamAssignment>();
    
    /// <summary>
    /// 试卷提交
    /// </summary>
    public ICollection<ExamSubmission> Submissions { get; set; } = new List<ExamSubmission>();
}

/// <summary>
/// 试卷题目关联实体类
/// </summary>
public class ExamQuestion
{
    /// <summary>
    /// 关联ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 试卷ID
    /// </summary>
    public int ExamId { get; set; }
    
    /// <summary>
    /// 试卷
    /// </summary>
    public Exam? Exam { get; set; }
    
    /// <summary>
    /// 题目ID
    /// </summary>
    public int QuestionId { get; set; }
    
    /// <summary>
    /// 题目
    /// </summary>
    public Question? Question { get; set; }
    
    /// <summary>
    /// 题目在试卷中的序号
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// 题目分值
    /// </summary>
    public int Score { get; set; }
}

/// <summary>
/// 试卷分配实体类
/// </summary>
public class ExamAssignment
{
    /// <summary>
    /// 分配ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 试卷ID
    /// </summary>
    public int ExamId { get; set; }
    
    /// <summary>
    /// 试卷
    /// </summary>
    public Exam? Exam { get; set; }
    
    /// <summary>
    /// 学生ID
    /// </summary>
    public int StudentId { get; set; }
    
    /// <summary>
    /// 分配时间
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 是否已提交
    /// </summary>
    public bool IsSubmitted { get; set; } = false;
}

/// <summary>
/// 试卷提交实体类
/// </summary>
public class ExamSubmission
{
    /// <summary>
    /// 提交ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 试卷ID
    /// </summary>
    public int ExamId { get; set; }
    
    /// <summary>
    /// 试卷
    /// </summary>
    public Exam? Exam { get; set; }
    
    /// <summary>
    /// 学生ID
    /// </summary>
    public int StudentId { get; set; }
    
    /// <summary>
    /// 提交时间
    /// </summary>
    public DateTime SubmittedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 完成时间（分钟）
    /// </summary>
    public int CompletionTime { get; set; }
    
    /// <summary>
    /// 得分
    /// </summary>
    public int Score { get; set; }
    
    /// <summary>
    /// 答案
    /// </summary>
    public ICollection<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
}

/// <summary>
/// 题目答案实体类
/// </summary>
public class QuestionAnswer
{
    /// <summary>
    /// 答案ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 提交ID
    /// </summary>
    public int SubmissionId { get; set; }
    
    /// <summary>
    /// 提交
    /// </summary>
    public ExamSubmission? Submission { get; set; }
    
    /// <summary>
    /// 题目ID
    /// </summary>
    public int QuestionId { get; set; }
    
    /// <summary>
    /// 题目
    /// </summary>
    public Question? Question { get; set; }
    
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
