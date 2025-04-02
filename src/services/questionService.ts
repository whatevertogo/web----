import { ref } from 'vue'
import { QuestionType, TableQuestion, SingleQuestionForm, JudgeQuestionForm, FillQuestionForm, ProgramQuestionForm, ShortAnswerQuestionForm } from '../types/question'
import { storage } from '../utils/storage'

// 添加初始化状态标志
let cachedData: TableQuestion[] | null = null

// 从 localStorage 加载数据或使用默认数据
const loadInitialData = (): TableQuestion[] => {
  const savedData = storage.load()
  return savedData || [
    // Vue 3 基础概念
    {
      id: 1,
      type: QuestionType.Single,
      question: 'Vue 3 相比 Vue 2，主要的架构升级是什么？',
      options: [
        '使用 TypeScript 重写',
        '重写虚拟 DOM 实现',
        '采用模块化架构设计',
        '以上都是'
      ],
      correctAnswer: 'D',
      analysis: 'Vue 3 完全使用 TypeScript 重写，采用模块化架构设计，并重写了虚拟 DOM 的实现以提升性能。',
      createTime: new Date().toLocaleString()
    },
    {
      id: 2,
      type: QuestionType.Single,
      question: 'Vue 3 中，创建应用实例的方法是？',
      options: [
        'new Vue()',
        'createApp()',
        'Vue.create()',
        'Vue.application()'
      ],
      correctAnswer: 'B',
      analysis: 'Vue 3 使用 createApp() 创建应用实例，这是一个新的工厂函数。',
      createTime: new Date().toLocaleString()
    },
    {
      id: 3,
      type: QuestionType.Judge,
      question: 'Vue 3 中的 ref 和 reactive 都可以创建响应式对象。',
      correctAnswer: 'true',
      analysis: '是的，但它们有不同的使用场景：ref 主要用于基本类型，reactive 用于对象。',
      createTime: new Date().toLocaleString()
    },
    {
      id: 4,
      type: QuestionType.Fill,
      question: 'Vue 3 中，_____ 用于监听响应式数据的变化，而 _____ 用于监听多个数据源。',
      answers: ['watch', 'watchEffect'],
      analysis: 'watch 需要明确指定要监听的数据，而 watchEffect 会自动收集依赖。',
      createTime: new Date().toLocaleString()
    },
    // Composition API 部分
    {
      id: 5,
      type: QuestionType.Single,
      question: 'Composition API 中，如何在组件卸载时清理副作用？',
      options: [
        '在 onUnmounted 中执行清理',
        '在 setup 函数返回一个清理函数',
        '使用 watchEffect 的返回函数',
        '以上都可以'
      ],
      correctAnswer: 'D',
      analysis: '这三种方式都可以用于清理副作用，选择哪种方式取决于具体场景。',
      createTime: new Date().toLocaleString()
    },
    {
      id: 6,
      type: QuestionType.Program,
      question: '使用 Composition API 实现一个带防抖功能的搜索输入框。',
      sampleInput: '要求：\n1. 使用 ref 存储输入值\n2. 使用 watchEffect 监听变化\n3. 实现 300ms 的防抖',
      sampleOutput: `<script setup>
import { ref, watchEffect } from 'vue'

const searchText = ref('')
const debounceTimer = ref(null)

const search = () => {
  if (debounceTimer.value) clearTimeout(debounceTimer.value)
  debounceTimer.value = setTimeout(() => {
    console.log('Searching:', searchText.value)
  }, 300)
}

watchEffect(() => {
  if (searchText.value) search()
})
</script>

<template>
  <input v-model="searchText" placeholder="Search..." />
</template>`,
      analysis: '这个示例展示了如何结合 ref、watchEffect 和防抖功能实现一个实用的搜索输入框。',
      createTime: new Date().toLocaleString()
    },
    {
      id: 7,
      type: QuestionType.Single,
      question: '在 Composition API 中，如何获取当前组件实例？',
      options: [
        'this',
        'getCurrentInstance()',
        'useComponent()',
        'getComponentInstance()'
      ],
      correctAnswer: 'B',
      analysis: '在 Composition API 中，可以使用 getCurrentInstance() 获取当前组件实例，但不建议过度依赖它。',
      createTime: new Date().toLocaleString()
    },
    {
      id: 8,
      type: QuestionType.Judge,
      question: 'setup 函数中的 props 是响应式的。',
      correctAnswer: 'true',
      analysis: 'props 是响应式的，但不能直接解构，否则会失去响应性。如需解构，可以使用 toRefs。',
      createTime: new Date().toLocaleString()
    },
    {
      id: 9,
      type: QuestionType.Single,
      question: 'Vue 3 中，创建响应式数据的方法有哪些？',
      options: [
        'ref()',
        'reactive()',
        'ref() 和 reactive()',
        'ref()、reactive() 和 toRef()'
      ],
      correctAnswer: 'D',
      analysis: 'Vue 3 提供了 ref()、reactive() 和 toRef() 等多种创建响应式数据的方法，它们适用于不同的场景。',
      createTime: new Date().toLocaleString()
    },
    {
      id: 10,
      type: QuestionType.Single,
      question: 'Vue 3 中，如何在 setup 中访问路由信息？',
      options: [
        'this.$route',
        'useRoute()',
        'getCurrentRoute()',
        'getRouteInfo()'
      ],
      correctAnswer: 'B',
      analysis: '在 Composition API 中，使用 useRoute() 和 useRouter() 来访问路由信息和路由实例。',
      createTime: new Date().toLocaleString()
    },
    // ... 继续添加更多题目
  ]
}

// 定义问题类型
type QuestionForm = SingleQuestionForm | JudgeQuestionForm | FillQuestionForm | ProgramQuestionForm | ShortAnswerQuestionForm

const questions = ref<TableQuestion[]>([])

// 初始化数据
const initializeData = async (force = false) => {
  if (cachedData && !force) return
  
  const savedData = storage.load()
  if (savedData) {
    cachedData = savedData
    questions.value = [...savedData]
  } else {
    cachedData = loadInitialData()
    questions.value = [...cachedData]
  }
}

// 添加清除缓存方法
const clearCache = () => {
  cachedData = null
  questions.value = []
}

// 保存数据到 localStorage
const saveData = () => {
  storage.save(questions.value)
}

// 获取下一个可用的 ID
const getNextId = (): number => {
  const maxId = Math.max(...questions.value.map(q => q.id))
  return maxId + 1
}

// 添加试题
const addQuestion = (question: QuestionForm): TableQuestion => {
  const newQuestion: TableQuestion = {
    id: getNextId(),
    type: question.type as QuestionType, // 确保 type 是必选的
    question: question.question,
    createTime: new Date().toLocaleString(),
    analysis: question.analysis,
    // 根据题型添加特定字段
    ...(question.type === QuestionType.Single && {
      options: (question as SingleQuestionForm).options,
      correctAnswer: (question as SingleQuestionForm).correctAnswer
    }),
    ...(question.type === QuestionType.Judge && {
      correctAnswer: (question as JudgeQuestionForm).correctAnswer
    }),
    ...(question.type === QuestionType.Fill && {
      answers: (question as FillQuestionForm).answers
    }),
    ...(question.type === QuestionType.Program && {
      sampleInput: (question as ProgramQuestionForm).sampleInput,
      sampleOutput: (question as ProgramQuestionForm).sampleOutput
    }),
    ...(question.type === QuestionType.ShortAnswer && {
      referenceAnswer: (question as ShortAnswerQuestionForm).referenceAnswer
    })
  }

  questions.value.push(newQuestion)
  saveData()
  return newQuestion
}

// 添加时间范围过滤方法
const filterByDateRange = (questions: TableQuestion[], dateRange?: [Date, Date]) => {
  if (!dateRange || !dateRange[0] || !dateRange[1]) {
    return questions
  }

  const startTime = dateRange[0].getTime()
  const endTime = dateRange[1].getTime()

  return questions.filter(q => {
    const questionTime = new Date(q.createTime).getTime()
    return questionTime >= startTime && questionTime <= endTime
  })
}

// 修改获取试题列表方法
const getQuestions = async (params?: {
  keyword?: string
  type?: QuestionType
  dateRange?: [Date, Date]
}): Promise<TableQuestion[]> => {
  // 确保数据已初始化
  await initializeData()
  
  // 模拟网络延迟
  await new Promise(resolve => setTimeout(resolve, 300))
  
  let result = [...questions.value]

  if (params) {
    const { keyword, type, dateRange } = params

    if (keyword) {
      result = result.filter(q => 
        q.question.toLowerCase().includes(keyword.toLowerCase())
      )
    }

    if (type) {
      result = result.filter(q => q.type === type)
    }

    if (dateRange) {
      result = filterByDateRange(result, dateRange)
    }
  }

  return result
}

// 删除试题
const deleteQuestion = (id: number): boolean => {
  const index = questions.value.findIndex(q => q.id === id)
  if (index > -1) {
    questions.value.splice(index, 1)
    saveData()
    return true
  }
  return false
}

// 更新试题
const updateQuestion = (question: TableQuestion): boolean => {
  const index = questions.value.findIndex(q => q.id === question.id)
  if (index > -1) {
    questions.value[index] = {...question}
    saveData()
    return true
  }
  return false
}

// 清理题目数据，确保没有重复
const cleanQuestionData = (question) => {
  // 基本数据清理
  if (!question.question) question.question = '';
  if (!question.id) question.id = Date.now() + Math.floor(Math.random() * 1000);
  
  // 确保题目文本没有重复包含自己
  if (question.type === QuestionType.Judge) {
    // 检查题目是否已经包含选项文本
    const lowerQuestion = question.question.toLowerCase();
    if (lowerQuestion.includes('正确') && lowerQuestion.includes('错误')) {
      // 可能题目已经包含了选项，需要清理
      question.question = question.question.replace(/[（(]?[正确|错误][)）]?/g, '');
    }
  }
  
  return question;
}

export const questionService = {
  questions,
  addQuestion,
  getQuestions,
  deleteQuestion,
  updateQuestion,
  filterByDateRange,
  cleanQuestionData,
  initializeData,
  clearCache
} 