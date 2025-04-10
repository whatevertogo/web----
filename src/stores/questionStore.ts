import { defineStore } from 'pinia'
import { questionService } from '../services/questionService'
import { ref } from 'vue'
import { QuestionType, TableQuestion } from '../types/question'

export const useQuestionStore = defineStore('question', () => {
  const questions = ref<TableQuestion[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // 答案缓存：key为题目ID，value为答案
  const answers = ref<Record<number, any>>({})

  // 设置答案
  function setAnswer(questionId: number, answer: any) {
    answers.value[questionId] = answer
  }

  // 加载题目
  async function loadQuestions() {
    isLoading.value = true
    error.value = null
    try {
      const data = await questionService.getQuestions()
      questions.value = data as TableQuestion[]
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
    loadQuestions,
    answers,
    setAnswer
    // ...其他方法
  }
})