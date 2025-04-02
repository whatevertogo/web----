// 问题类型枚举
export enum QuestionType {
  Single = 'single',
  Judge = 'judge',
  Fill = 'fill',
  Program = 'program',
  ShortAnswer = 'shortAnswer'
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
  correctAnswer: 'true' | 'false';
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
}

// 简答题表单接口
export interface ShortAnswerQuestionForm extends BaseQuestion {
  type: QuestionType.ShortAnswer;
  referenceAnswer: string;
}

// 表格数据接口
export interface TableQuestion {
  id: number;
  type: QuestionType;
  question: string;
  createTime: string;
  options?: string[];
  correctAnswer?: string;
  analysis?: string;
  answers?: string[];
  sampleInput?: string;
  sampleOutput?: string;
  referenceAnswer?: string;
}

// 类型映射接口
export interface TypeMap extends Record<QuestionType, string> {
  [QuestionType.Single]: string;
  [QuestionType.Judge]: string;
  [QuestionType.Fill]: string;
  [QuestionType.Program]: string;
  [QuestionType.ShortAnswer]: string;
}