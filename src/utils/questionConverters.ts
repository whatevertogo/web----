import { QuestionType } from '@/types/question'
import type { TableQuestion, ApiQuestion } from '@/types/question'
import { api } from '@/services/api'
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
      // 确保判断题答案格式为“正确”或“错误”
      let correctAnswer = question.correctAnswer;
      if (correctAnswer === 'true') correctAnswer = '正确';
      if (correctAnswer === 'false') correctAnswer = '错误';

      apiData.answersJson = JSON.stringify([correctAnswer])
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
    try {
      const response = await api.get<ApiQuestion>(`/questions/${id}`)

      // 如果是对象且有所需属性
      if (response && typeof response === 'object') {
        return questionTypeConverters.fromApiFormat(response)
      }

      throw new Error('获取题目失败：响应格式异常')
    } catch (error: any) {
      console.error('获取题目失败:', error)
      throw new Error(`获取题目失败: ${error.message || '未知错误'}`)
    }
  },

  async updateQuestion(question: TableQuestion): Promise<boolean> {
    try {
      const apiData = questionTypeConverters.toApiFormat(question)
      const response = await api.put<boolean>('/questions', apiData)

      // 如果是204 No Content响应
      if (response && Object.keys(response).length === 0) {
        return true
      }

      // 如果是对象且有 success 属性
      // 使用类型断言来确保 response 不是 null
      if (response && typeof response === 'object' && response !== null) {
        const responseObj = response as Record<string, unknown>;
        if ('success' in responseObj) {
          return Boolean(responseObj.success);
        }
      }

      return false
    } catch (error: any) {
      console.error('更新题目失败:', error)
      throw new Error(`更新题目失败: ${error.message || '未知错误'}`)
    }
  }
}