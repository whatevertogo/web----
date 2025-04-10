import { QuestionType } from '@/types/question'
import type { TableQuestion, ApiQuestion } from '@/types/question'
import { api } from '@/services/api'
import type { ApiResponse } from '@/types/api'
import { convertApiQuestion } from '@/services/questionService'

/**
 * 题目类型转换工具
 */
export const questionTypeConverters = {
  /**
   * 将前端TableQuestion转换为API格式
   */
  toApiFormat: (question: TableQuestion): any => {
    const apiData: any = {
      id: question.id,
      type: question.type,
      content: question.question,
      optionsJson: question.options ? JSON.stringify(question.options) : '[]',
      answersJson: question.answers ? JSON.stringify(question.answers) : '[]',
      examplesJson: question.sampleInput || question.sampleOutput ? JSON.stringify({
        input: question.sampleInput,
        output: question.sampleOutput
      }) : null,
      tagsJson: question.tags ? JSON.stringify(question.tags) : '[]',
      analysis: question.analysis,
      referenceAnswer: question.referenceAnswer,
      createTime: question.createTime,
      category: question.category
    }

    // 判断题特殊处理
    if (question.type === QuestionType.Judge) {
      apiData.answersJson = JSON.stringify([question.correctAnswer])
      apiData.optionsJson = JSON.stringify(["正确", "错误"])
    }

    return apiData
  },

  /**
   * 将API格式转换为前端TableQuestion
   */
  fromApiFormat: (apiQuestion: ApiQuestion): TableQuestion => {
    return convertApiQuestion(apiQuestion)
  }
}

/**
 * 类型安全的题目API
 */
export const typeSafeQuestionApi = {  async getQuestion(id: number): Promise<TableQuestion> {
    const response = await api.get<ApiResponse<ApiQuestion>>(`/questions/${id}`)
    if (!response.data) {
      throw new Error('获取题目失败：没有返回数据')
    }
    return questionTypeConverters.fromApiFormat(response.data)
  },

  async updateQuestion(question: TableQuestion): Promise<boolean> {
    const apiData = questionTypeConverters.toApiFormat(question)
    const response = await api.put<ApiResponse<boolean>>(`/questions/${question.id}`, apiData)
    return response.success || false
  }
}