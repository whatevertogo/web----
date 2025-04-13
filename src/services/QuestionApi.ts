import type { SearchParams } from '../types/question'
import { api } from './api'

function formatDateTime(date: Date): string {
  const pad = (n: number) => n < 10 ? '0' + n : n
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())} ${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}`
}

// 获取所有题目
export const getQuestions = (params?: SearchParams) => {
  console.log('调用 getQuestions API, 参数:', params);
  const queryParams: Record<string, string> = {}
  if (params) {
    if (params.keyword) queryParams.keyword = params.keyword
    if (params.type) queryParams.type = params.type.toString()
    if (params.dateRange && params.dateRange.length === 2) {
      queryParams.startDate = formatDateTime(params.dateRange[0])
      queryParams.endDate = formatDateTime(params.dateRange[1])
    }
  }
  console.log('最终查询参数:', queryParams);
  // 不需要添加 /api 前缀，因为在 api.ts 中已经配置了基础URL
  return api.get('/questions', queryParams)
}

// 根据ID获取题目
export const getQuestionById = (id: number) => api.get(`/questions/${id}`)

// 新增题目
export const addQuestion = (data: any) => api.post('/questions', data)

// 更新题目
export const updateQuestion = (data: any) => {
  // 确保 id 存在
  if (!data.id) {
    throw new Error('更新题目时必须提供 id')
  }
  return api.put('/questions', data)
}

// 删除题目
export const deleteQuestion = (id: number) => {
  // 确保 id 存在
  if (!id) {
    throw new Error('删除题目时必须提供 id')
  }
  // 使用与后端控制器一致的路径
  return api.delete(`/questions/${id}`)
}