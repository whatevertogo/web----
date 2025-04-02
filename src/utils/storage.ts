import type { TableQuestion } from '../types/question'

const STORAGE_KEY = 'question_db'

export const storage = {
  // 保存数据到 localStorage
  save(data: TableQuestion[]) {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(data))
    } catch (error) {
      console.error('存储数据失败:', error)
      throw error
    }
  },

  // 从 localStorage 读取数据
  load(): TableQuestion[] | null {
    try {
      const data = localStorage.getItem(STORAGE_KEY)
      return data ? JSON.parse(data) : null
    } catch (error) {
      console.error('加载数据失败:', error)
      return null
    }
  },

  // 清除数据
  clear() {
    localStorage.removeItem(STORAGE_KEY)
  }
} 