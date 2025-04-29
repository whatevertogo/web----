// 问题类型枚举（与后端对应）
export enum QuestionType {
  Single = 0,
  Judge = 1,
  Fill = 2,
  Program = 3,
  ShortAnswer = 4
}

// 题型映射类型
export interface TypeMap {
  [key: string]: string;
  [QuestionType.Single]: string;
  [QuestionType.Judge]: string;
  [QuestionType.Fill]: string;
  [QuestionType.Program]: string;
  [QuestionType.ShortAnswer]: string;
}

// 搜索参数接口
export interface SearchParams {
  keyword?: string;
  type?: QuestionType;
  dateRange?: [Date, Date];
}

// 基础问题接口
export interface BaseQuestion {
  id?: number;
  createTime?: string;
  question: string;
  analysis: string;
}

// 单选题表单接口
export interface SingleQuestionForm extends BaseQuestion {
  type: QuestionType.Single;
  options: string[];
  correctAnswer: string;
}

// 判断题表单接口
export interface JudgeQuestionForm extends BaseQuestion {
  type: QuestionType.Judge;
  correctAnswer: '正确' | '错误';
}

// 填空题表单接口
export interface FillQuestionForm extends BaseQuestion {
  type: QuestionType.Fill;
  answers: string[];
}

// 编程题表单接口
export interface ProgramQuestionForm extends BaseQuestion {
  type: QuestionType.Program;
  sampleInput: string;
  sampleOutput: string;
  referenceAnswer: string;
}

// 简答题表单接口
export interface ShortAnswerQuestionForm extends BaseQuestion {
  type: QuestionType.ShortAnswer;
  referenceAnswer: string;
}

// 题目 DTO 类型联合
export type QuestionForm =
  | SingleQuestionForm
  | JudgeQuestionForm
  | FillQuestionForm
  | ProgramQuestionForm
  | ShortAnswerQuestionForm;

// 表格数据接口
export interface TableQuestion {
  id: number;
  type: QuestionType;
  question: string;
  options?: string[];
  correctAnswer?: string;
  answers?: string[];
  sampleInput?: string;
  sampleOutput?: string;
  referenceAnswer?: string;
  analysis?: string;
  createTime: string;
  category?: string;
  tags?: string[];
}
export interface ApiQuestion {
  id: number;
  type: QuestionType;
  content: string;
  optionsJson?: string;
  answersJson?: string;
  examplesJson?: string;
  tagsJson?: string;
  analysis?: string;
  referenceAnswer?: string;
  createTime?: string;
  difficulty?: number;
  category?: string;
}
