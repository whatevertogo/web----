import type { SearchParams } from '../types/question'
import { api } from './api'

function formatDateTime(date: Date): string {
  const pad = (n: number) => n < 10 ? '0' + n : n
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())} ${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}`
}

// 获取所有题目
export const getQuestions = (params?: SearchParams) => {
  const queryParams: Record<string, string> = {}
  if (params) {
    if (params.keyword) queryParams.keyword = params.keyword
    if (params.type) queryParams.type = params.type.toString()
    if (params.dateRange) {
      queryParams.startDate = formatDateTime(params.dateRange[0])
      queryParams.endDate = formatDateTime(params.dateRange[1])
    }
  }
  return api.get('/questions', queryParams)
}

// 根据ID获取题目
export const getQuestionById = (id: number) => api.get(`/questions/${id}`)

// 新增题目
export const addQuestion = (data: any) => api.post('/questions', data)

// 更新题目
export const updateQuestion = (data: any) => api.put('/questions', data)

// 删除题目
export const deleteQuestion = (id: number) => api.delete(`/questions/${id}`)