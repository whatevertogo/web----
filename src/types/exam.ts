// src/types/exam.ts

// 你可能需要从其他类型文件导入相关类型，例如 QuestionDto
// import type { QuestionDto } from './question';

// --- 从后端 ExamResultDto.cs 对应 ---
export interface ExamResultDto {
  examId: number;
  studentId: number;
  studentName?: string | null;
  submittedAt: string; // DateTime -> string
  completionTime: number; // 分钟
  totalScore: number; // 试卷总分
  score: number; // 学生得分
  correctCount: number; // 学生答对题数
  questionCount: number; // 试卷总题数
  // answers?: any[]; // 可能不需要在统计视图显示详细答案
}

// --- 从后端 QuestionStatisticsDto.cs 对应 ---
export interface QuestionStatisticsDto {
  questionId: number;
  order: number;              // 题目序号
  type: number;               // 题目类型 (可以考虑映射为枚举或字符串)
  content?: string | null;
  correctCount: number;       // 正确人数
  incorrectCount: number;     // 错误人数
  correctRate: number;        // 正确率 (0 to 1)
  optionCounts?: { [key: string]: number } | null; // 选项 -> 选择人数 (A: 5, B: 10, ...)
}

// --- 从后端 ExamStatisticsDto.cs 对应 ---
export interface ExamStatisticsDto {
  examId: number;
  examTitle?: string | null;
  studentCount: number;       // 参与学生数 (Assigned)
  submittedCount: number;     // 已提交数
  averageScore: number;
  highestScore: number;
  lowestScore: number;
  passRate: number;           // 及格率 (0 to 1)
  scoreDistribution: { [key: string]: number }; // 分数段 -> 人数
  questionStatistics: QuestionStatisticsDto[]; // 各题统计
  studentResults: ExamResultDto[];           // 学生成绩列表
}

// --- 从后端 ExamQuestionDto.cs 对应 ---
export interface ExamQuestionDto {
    questionId: number;
    order: number;
    score: number;
    // question?: QuestionDto | null; // 如果需要嵌套题目详情，需要导入 QuestionDto
}

// --- 从后端 ExamDto.cs 对应 ---
export interface ExamDto {
    id: number;
    title: string;
    description?: string | null;
    createdAt: string; // DateTime -> string
    deadline?: string | null; // Nullable DateTime -> string | null
    totalScore: number;
    status: number; // 0-草稿, 1-已发布, 2-已结束
    questions: ExamQuestionDto[];
}
