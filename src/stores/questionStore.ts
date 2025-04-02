import { defineStore } from 'pinia'
import { ref } from 'vue'
import { QuestionType, TableQuestion } from '../types/question'
import { storage } from '../utils/storage'

export const useQuestionStore = defineStore('question', () => {
  const questions = ref<TableQuestion[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // 加载题目
  async function loadQuestions() {
    isLoading.value = true
    error.value = null
    try {
      const data = storage.load()
      questions.value = data || []
    } catch (err) {
      error.value = '加载题目失败'
      console.error(err)
    } finally {
      isLoading.value = false
    }
  }

  // 其他方法...

  return {
    questions,
    isLoading,
    error,
    loadQuestions
    // ...其他方法
  }
}) 