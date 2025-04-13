using Microsoft.EntityFrameworkCore;
using QuestionBankApi.Data;
using QuestionBankApi.DTOs;
using QuestionBankApi.Enums;
using QuestionBankApi.Models;
using QuestionBankApi.LoginSystemApi.Data;

namespace QuestionBankApi.Services;

/// <summary>
/// 试卷服务实现
/// </summary>
public class ExamService : IExamService
{
    private readonly QuestionBankDbContext _db;
    private readonly UserDBContext _userDb;
    private readonly IQuestionService _questionService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db">数据库上下文</param>
    /// <param name="userDb">用户数据库上下文</param>
    /// <param name="questionService">题目服务</param>
    public ExamService(QuestionBankDbContext db, UserDBContext userDb, IQuestionService questionService)
    {
        _db = db;
        _userDb = userDb;
        _questionService = questionService;
    }

    /// <summary>
    /// 创建试卷
    /// </summary>
    /// <param name="dto">试卷数据传输对象</param>
    /// <returns>创建的试卷</returns>
    public async Task<ExamDto> CreateExamAsync(ExamDto dto)
    {
        // 创建试卷实体
        var exam = new Exam
        {
            Title = dto.Title,
            Description = dto.Description,
            CreatedAt = DateTime.Now,
            Deadline = dto.Deadline,
            Status = dto.Status,
            TotalScore = dto.Questions.Sum(q => q.Score)
        };

        // 添加试卷
        _db.Exams.Add(exam);
        await _db.SaveChangesAsync();

        // 添加试卷题目
        foreach (var questionDto in dto.Questions)
        {
            var examQuestion = new ExamQuestion
            {
                ExamId = exam.Id,
                QuestionId = questionDto.QuestionId,
                Order = questionDto.Order,
                Score = questionDto.Score
            };
            _db.ExamQuestions.Add(examQuestion);
        }

        await _db.SaveChangesAsync();

        // 返回创建的试卷
        var createdExam = await GetExamByIdAsync(exam.Id);
        return createdExam ?? throw new Exception($"无法获取新创建的试卷，ID: {exam.Id}");
    }

    /// <summary>
    /// 获取所有试卷
    /// </summary>
    /// <returns>试卷列表</returns>
    public async Task<IEnumerable<ExamDto>> GetAllExamsAsync()
    {
        var exams = await _db.Exams
            .Include(e => e.ExamQuestions)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return exams.Select(e => new ExamDto
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            CreatedAt = e.CreatedAt,
            Deadline = e.Deadline,
            TotalScore = e.TotalScore,
            Status = e.Status,
            Questions = e.ExamQuestions.Select(q => new ExamQuestionDto
            {
                QuestionId = q.QuestionId,
                Order = q.Order,
                Score = q.Score
            }).ToList()
        });
    }

    /// <summary>
    /// 根据ID获取试卷
    /// </summary>
    /// <param name="id">试卷ID</param>
    /// <returns>试卷信息</returns>
    public async Task<ExamDto?> GetExamByIdAsync(int id)
    {
        var exam = await _db.Exams
            .Include(e => e.ExamQuestions)
            .ThenInclude(q => q.Question)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exam == null)
            return null;

        var examDto = new ExamDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            CreatedAt = exam.CreatedAt,
            Deadline = exam.Deadline,
            TotalScore = exam.TotalScore,
            Status = exam.Status,
            Questions = new List<ExamQuestionDto>()
        };

        foreach (var examQuestion in exam.ExamQuestions.OrderBy(q => q.Order))
        {
            var question = examQuestion.Question;
            var questionDto = question != null ? new QuestionDto
            {
                Id = question.Id,
                Content = question.Content,
                Type = question.Type,
                OptionsJson = question.OptionsJson,
                AnswersJson = question.AnswersJson,
                Analysis = question.Analysis,
                Difficulty = question.Difficulty,
                TagsJson = question.TagsJson,
                CreateTime = question.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
            } : null;

            examDto.Questions.Add(new ExamQuestionDto
            {
                QuestionId = examQuestion.QuestionId,
                Order = examQuestion.Order,
                Score = examQuestion.Score,
                Question = questionDto
            });
        }

        return examDto;
    }

    /// <summary>
    /// 获取学生的试卷列表
    /// </summary>
    /// <param name="studentId">学生ID</param>
    /// <returns>试卷列表</returns>
    public async Task<IEnumerable<ExamDto>> GetExamsByStudentIdAsync(int studentId)
    {
        var assignments = await _db.ExamAssignments
            .Include(a => a.Exam)
            .ThenInclude(e => e != null ? e.ExamQuestions : null)
            .Where(a => a.StudentId == studentId)
            .ToListAsync();

        // 过滤掉没有试卷关联的分配记录
        var validAssignments = assignments.Where(a => a.Exam != null).ToList();

        return validAssignments.Select(a => new ExamDto
        {
            Id = a.Exam!.Id,
            Title = a.Exam.Title,
            Description = a.Exam.Description,
            CreatedAt = a.Exam.CreatedAt,
            Deadline = a.Exam.Deadline,
            TotalScore = a.Exam.TotalScore,
            Status = a.Exam.Status,
            Questions = a.Exam.ExamQuestions.Select(q => new ExamQuestionDto
            {
                QuestionId = q.QuestionId,
                Order = q.Order,
                Score = q.Score
            }).ToList()
        });
    }

    /// <summary>
    /// 分配试卷给学生
    /// </summary>
    /// <param name="examId">试卷ID</param>
    /// <param name="studentIds">学生ID列表</param>
    /// <returns>分配结果</returns>
    public async Task<bool> AssignExamToStudentsAsync(int examId, List<int> studentIds)
    {
        // 检查试卷是否存在
        var exam = await _db.Exams.FindAsync(examId);
        if (exam == null)
            return false;

        // 检查学生是否存在
        var existingStudents = await _userDb.Users
            .Where(u => studentIds.Contains(u.Id) && u.Role == LoginSystemApi.Models.UserRole.Student)
            .Select(u => u.Id)
            .ToListAsync();

        if (existingStudents.Count != studentIds.Count)
            return false;

        // 检查是否已分配
        var existingAssignments = await _db.ExamAssignments
            .Where(a => a.ExamId == examId && studentIds.Contains(a.StudentId))
            .Select(a => a.StudentId)
            .ToListAsync();

        // 添加新分配
        var newAssignments = studentIds
            .Except(existingAssignments)
            .Select(studentId => new ExamAssignment
            {
                ExamId = examId,
                StudentId = studentId,
                AssignedAt = DateTime.Now
            });

        _db.ExamAssignments.AddRange(newAssignments);

        // 更新试卷状态为已发布
        if (exam.Status == 0)
        {
            exam.Status = 1; // 已发布
        }

        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 检查试卷是否分配给学生
    /// </summary>
    /// <param name="examId">试卷ID</param>
    /// <param name="studentId">学生ID</param>
    /// <returns>是否分配</returns>
    public async Task<bool> IsExamAssignedToStudentAsync(int examId, int studentId)
    {
        return await _db.ExamAssignments
            .AnyAsync(a => a.ExamId == examId && a.StudentId == studentId);
    }

    /// <summary>
    /// 提交试卷
    /// </summary>
    /// <param name="examId">试卷ID</param>
    /// <param name="studentId">学生ID</param>
    /// <param name="dto">提交数据</param>
    /// <returns>提交结果</returns>
    public async Task<ExamResultDto> SubmitExamAsync(int examId, int studentId, ExamSubmissionDto dto)
    {
        // 检查试卷是否存在
        var exam = await _db.Exams
            .Include(e => e.ExamQuestions)
            .FirstOrDefaultAsync(e => e.Id == examId);

        if (exam == null)
            throw new Exception($"未找到ID为{examId}的试卷");

        // 检查是否已分配给学生
        var assignment = await _db.ExamAssignments
            .FirstOrDefaultAsync(a => a.ExamId == examId && a.StudentId == studentId);

        if (assignment == null)
            throw new Exception("该试卷未分配给当前学生");

        // 检查是否已提交
        if (assignment.IsSubmitted)
            throw new Exception("该试卷已提交，不能重复提交");

        // 检查截止时间
        if (exam.Deadline.HasValue && DateTime.Now > exam.Deadline.Value)
            throw new Exception("已超过截止时间，不能提交");

        // 创建提交记录
        var submission = new ExamSubmission
        {
            ExamId = examId,
            StudentId = studentId,
            SubmittedAt = DateTime.Now,
            CompletionTime = dto.CompletionTime,
            Score = 0 // 初始分数为0，后面计算
        };

        _db.ExamSubmissions.Add(submission);
        await _db.SaveChangesAsync();

        // 计算得分
        int totalScore = 0;
        int correctCount = 0;

        foreach (var answerDto in dto.Answers)
        {
            // 获取题目信息
            var question = await _db.Questions.FindAsync(answerDto.QuestionId);
            if (question == null)
                continue;

            // 获取题目在试卷中的分值
            var examQuestion = exam.ExamQuestions.FirstOrDefault(q => q.QuestionId == answerDto.QuestionId);
            if (examQuestion == null)
                continue;

            // 判断答案是否正确
            bool isCorrect = IsAnswerCorrect((int)question.Type, question.AnswersJson, answerDto.Answer);
            int score = isCorrect ? examQuestion.Score : 0;

            // 创建答案记录
            var answer = new QuestionAnswer
            {
                SubmissionId = submission.Id,
                QuestionId = answerDto.QuestionId,
                Answer = answerDto.Answer,
                IsCorrect = isCorrect,
                Score = score
            };

            _db.QuestionAnswers.Add(answer);

            // 累计得分
            totalScore += score;
            if (isCorrect)
                correctCount++;
        }

        // 更新提交记录的得分
        submission.Score = totalScore;

        // 更新分配记录为已提交
        assignment.IsSubmitted = true;

        await _db.SaveChangesAsync();

        // 返回提交结果
        var student = await _userDb.Users.FindAsync(studentId);

        return new ExamResultDto
        {
            ExamId = examId,
            StudentId = studentId,
            StudentName = student?.Username,
            SubmittedAt = submission.SubmittedAt,
            CompletionTime = submission.CompletionTime,
            TotalScore = exam.TotalScore,
            Score = totalScore,
            CorrectCount = correctCount,
            QuestionCount = exam.ExamQuestions.Count,
            Answers = dto.Answers.Select(a => new QuestionAnswerDto
            {
                QuestionId = a.QuestionId,
                Answer = a.Answer,
                IsCorrect = _db.QuestionAnswers
                    .FirstOrDefault(qa => qa.SubmissionId == submission.Id && qa.QuestionId == a.QuestionId)?.IsCorrect ?? false,
                Score = _db.QuestionAnswers
                    .FirstOrDefault(qa => qa.SubmissionId == submission.Id && qa.QuestionId == a.QuestionId)?.Score ?? 0
            }).ToList()
        };
    }

    /// <summary>
    /// 获取试卷统计信息
    /// </summary>
    /// <param name="examId">试卷ID</param>
    /// <returns>统计信息</returns>
    public async Task<ExamStatisticsDto> GetExamStatisticsAsync(int examId)
    {
        // 检查试卷是否存在
        var exam = await _db.Exams
            .Include(e => e.ExamQuestions)
            .ThenInclude(q => q.Question)
            .FirstOrDefaultAsync(e => e.Id == examId);

        if (exam == null)
            throw new Exception($"未找到ID为{examId}的试卷");

        // 获取所有提交记录
        var submissions = await _db.ExamSubmissions
            .Include(s => s.Answers)
            .Where(s => s.ExamId == examId)
            .ToListAsync();

        // 获取所有分配记录
        var assignments = await _db.ExamAssignments
            .Where(a => a.ExamId == examId)
            .ToListAsync();

        // 计算统计信息
        var statistics = new ExamStatisticsDto
        {
            ExamId = examId,
            ExamTitle = exam.Title,
            StudentCount = assignments.Count,
            SubmittedCount = submissions.Count,
            AverageScore = submissions.Any() ? submissions.Average(s => s.Score) : 0,
            HighestScore = submissions.Any() ? submissions.Max(s => s.Score) : 0,
            LowestScore = submissions.Any() ? submissions.Min(s => s.Score) : 0,
            PassRate = submissions.Any() ? (double)submissions.Count(s => s.Score >= exam.TotalScore * 0.6) / submissions.Count : 0,
            ScoreDistribution = new Dictionary<string, int>
            {
                { "0-59", submissions.Count(s => s.Score < exam.TotalScore * 0.6) },
                { "60-69", submissions.Count(s => s.Score >= exam.TotalScore * 0.6 && s.Score < exam.TotalScore * 0.7) },
                { "70-79", submissions.Count(s => s.Score >= exam.TotalScore * 0.7 && s.Score < exam.TotalScore * 0.8) },
                { "80-89", submissions.Count(s => s.Score >= exam.TotalScore * 0.8 && s.Score < exam.TotalScore * 0.9) },
                { "90-100", submissions.Count(s => s.Score >= exam.TotalScore * 0.9) }
            },
            QuestionStatistics = new List<QuestionStatisticsDto>(),
            StudentResults = new List<ExamResultDto>()
        };

        // 计算各题统计信息
        foreach (var examQuestion in exam.ExamQuestions.OrderBy(q => q.Order))
        {
            var question = examQuestion.Question;
            if (question == null)
                continue;

            var answers = submissions
                .SelectMany(s => s.Answers)
                .Where(a => a.QuestionId == question.Id)
                .ToList();

            var correctCount = answers.Count(a => a.IsCorrect);
            var incorrectCount = answers.Count - correctCount;

            var questionStatistics = new QuestionStatisticsDto
            {
                QuestionId = question.Id,
                Order = examQuestion.Order,
                Type = (int)question.Type,
                Content = question.Content,
                CorrectCount = correctCount,
                IncorrectCount = incorrectCount,
                CorrectRate = answers.Any() ? (double)correctCount / answers.Count : 0
            };

            // 如果是选择题，计算各选项的选择人数
            if (question.Type == QuestionType.Single || question.Type == QuestionType.Judge)
            {
                var optionCounts = new Dictionary<string, int>();
                foreach (var answer in answers)
                {
                    var options = answer.Answer.Split(',');
                    foreach (var option in options)
                    {
                        if (!string.IsNullOrEmpty(option))
                        {
                            if (optionCounts.ContainsKey(option))
                                optionCounts[option]++;
                            else
                                optionCounts[option] = 1;
                        }
                    }
                }
                questionStatistics.OptionCounts = optionCounts;
            }

            statistics.QuestionStatistics.Add(questionStatistics);
        }

        // 获取学生成绩
        var results = await GetExamResultsAsync(examId);
        statistics.StudentResults = results.ToList();

        return statistics;
    }

    /// <summary>
    /// 获取试卷成绩
    /// </summary>
    /// <param name="examId">试卷ID</param>
    /// <param name="studentId">学生ID（可选）</param>
    /// <returns>成绩信息</returns>
    public async Task<IEnumerable<ExamResultDto>> GetExamResultsAsync(int examId, int? studentId = null)
    {
        // 构建查询
        var query = _db.ExamSubmissions
            .Include(s => s.Answers)
            .Where(s => s.ExamId == examId);

        // 如果指定了学生ID，只查询该学生的成绩
        if (studentId.HasValue)
        {
            query = query.Where(s => s.StudentId == studentId.Value);
        }

        // 执行查询
        var submissions = await query.ToListAsync();

        // 获取试卷信息
        var exam = await _db.Exams
            .Include(e => e.ExamQuestions)
            .FirstOrDefaultAsync(e => e.Id == examId);

        if (exam == null)
            throw new Exception($"未找到ID为{examId}的试卷");

        // 获取学生信息
        var studentIds = submissions.Select(s => s.StudentId).Distinct().ToList();
        var students = await _userDb.Users
            .Where(u => studentIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username);

        // 构建结果
        var results = new List<ExamResultDto>();
        foreach (var submission in submissions)
        {
            var result = new ExamResultDto
            {
                ExamId = examId,
                StudentId = submission.StudentId,
                StudentName = students.ContainsKey(submission.StudentId) ? students[submission.StudentId] : null,
                SubmittedAt = submission.SubmittedAt,
                CompletionTime = submission.CompletionTime,
                TotalScore = exam.TotalScore,
                Score = submission.Score,
                CorrectCount = submission.Answers.Count(a => a.IsCorrect),
                QuestionCount = exam.ExamQuestions.Count,
                Answers = submission.Answers.Select(a => new QuestionAnswerDto
                {
                    QuestionId = a.QuestionId,
                    Answer = a.Answer,
                    IsCorrect = a.IsCorrect,
                    Score = a.Score
                }).ToList()
            };

            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// 判断答案是否正确
    /// </summary>
    /// <param name="questionType">题目类型</param>
    /// <param name="correctAnswer">正确答案</param>
    /// <param name="studentAnswer">学生答案</param>
    /// <returns>是否正确</returns>
    private static bool IsAnswerCorrect(int questionType, string? correctAnswer, string studentAnswer)
    {
        // 去除空格
        correctAnswer = correctAnswer?.Trim() ?? "";
        studentAnswer = studentAnswer?.Trim() ?? "";

        // 单选题和判断题
        if (questionType == 1 || questionType == 3)
        {
            return string.Equals(correctAnswer, studentAnswer, StringComparison.OrdinalIgnoreCase);
        }
        // 多选题
        else if (questionType == 2)
        {
            // 分割答案
            var correctOptions = correctAnswer.Split(',').Select(o => o.Trim()).Where(o => !string.IsNullOrEmpty(o)).OrderBy(o => o).ToArray();
            var studentOptions = studentAnswer.Split(',').Select(o => o.Trim()).Where(o => !string.IsNullOrEmpty(o)).OrderBy(o => o).ToArray();

            // 比较
            return correctOptions.SequenceEqual(studentOptions, StringComparer.OrdinalIgnoreCase);
        }
        // 填空题和简答题
        else if (questionType == 4 || questionType == 5)
        {
            // 简单比较，实际应用中可能需要更复杂的比较逻辑
            return string.Equals(correctAnswer, studentAnswer, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}
