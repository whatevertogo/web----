import { ref } from 'vue'
import type { ApiQuestion } from '@/types/question'
import { QuestionType, type TableQuestion } from '../types/question'
import { getQuestions, getQuestionById, addQuestion, updateQuestion, deleteQuestion } from './QuestionApi'
import { ElMessage } from 'element-plus'
import { validateQuestion } from '../utils/validators'
const retryRequest = async (fn: () => Promise<any>, retries = 3) => {
  for (let i = 0; i < retries; i++) {
    try {
      return await fn();
    } catch (err) {
      if (i === retries - 1) throw err;
      await new Promise(resolve => setTimeout(resolve, 1000 * (i + 1)));
    }
  }
};


// 定义 API 响应类型
interface ApiResponse {
  success: boolean;
  data: any;
  message?: string;
  error?: string;
}

// 注意: 所有响应都使用通用 ApiResponse 类型处理

interface SearchParams {
  keyword?: string;
  type?: QuestionType;
  dateRange?: [Date, Date];
}


// 测试数据
const testData: TableQuestion[] = [
  {
    id: 1,
    type: QuestionType.Single,
    question: 'Wrong,出现默认测试数据说明数据库未导入或者后端未打开，下列选项中，不属于 Vue 生命周期钩子的是？Wrong,出现默认测试数据说明数据库未导入或者后端未打开',
    options: ['created', 'mounted', 'updated', 'deleted'],
    correctAnswer: 'D',
    answers: ['deleted'],
    analysis: 'Vue 的生命周期钩子包括 beforeCreate, created, beforeMount, mounted, beforeUpdate, updated, beforeDestroy, destroyed。没有 deleted 这个钩子。',
    createTime: '2023-05-20 10:00:00',
    category: 'Vue',
    tags: ['Vue', '前端']
  },
  {
    id: 2,
    type: QuestionType.Judge,
    question: 'Wrong,出现默认测试数据说明数据库未导入或者后端未打开，Vue 3 中的 Composition API 可以与 Options API 混合使用。Wrong,出现默认测试数据说明数据库未导入或者后端未打开',
    correctAnswer: '正确',
    answers: ['正确'],
    analysis: 'Vue 3 允许在同一个组件中混合使用 Composition API 和 Options API。',
    createTime: '2023-05-21 11:30:00',
    category: 'Vue',
    tags: ['Vue3', 'Composition API']
  }
];

// 缓存题目列表
const questions = ref<TableQuestion[]>([])

// 选中的题目
let selectedQuestions: TableQuestion[] = []

// 将 API 返回的题目格式转换为前端使用的格式
export const convertApiQuestion = (q: ApiQuestion): TableQuestion => {
  let options = q.optionsJson ? JSON.parse(q.optionsJson) : [];
  const answers = q.answersJson ? JSON.parse(q.answersJson) : [];
  const examples = q.examplesJson ? JSON.parse(q.examplesJson) : null;
  const tags = q.tagsJson ? JSON.parse(q.tagsJson) : [];

  // 自动识别题型，覆盖后端错误type
  if (
    answers.length === 1 &&
    (answers[0] === '正确' || answers[0] === '错误' ||
     answers[0] === 'true' || answers[0] === 'false')
  ) {
    q.type = QuestionType.Judge;
    // 统一判断题答案格式
    if (answers[0] === 'true') answers[0] = '正确';
    if (answers[0] === 'false') answers[0] = '错误';

    if (!options || options.length === 0) {
      options = ["正确", "错误"];
    }
  }

  return {
    id: q.id,
    type: q.type,
    question: q.content,
    options,
    correctAnswer: answers[0],
    answers: (() => {
      if (q.type === QuestionType.Single || q.type === QuestionType.Judge) {
        return answers.length > 0 ? [answers[0]] : [];
      } else {
        return answers;
      }
    })(),
    analysis: q.analysis,
    sampleInput: examples?.input,
    sampleOutput: examples?.output,
    referenceAnswer: q.referenceAnswer,
    createTime: q.createTime || '',
    category: q.category,
    tags
  }
}

export const questionService = {
  questions,
  selectedQuestions,

  async getQuestions(params?: SearchParams): Promise<TableQuestion[]> {
    try {
      console.log('开始获取题目数据...');
      // 使用 retryRequest 包装 API 调用，增强错误处理
      const response = await retryRequest(() => getQuestions(params));
      console.log('原始 API 响应:', response);

      // 如果是空响应
      if (!response) {
        console.warn('服务器返回空响应，使用测试数据');
        questions.value = testData;
        return testData;
      }

      // 如果是数组，直接使用
      if (Array.isArray(response)) {
        console.log('响应是数组格式');
        const convertedQuestions = response.map(convertApiQuestion);
        questions.value = convertedQuestions;
        return convertedQuestions;
      }

      // 如果是对象且有 data 属性，并且 data 是数组
      if (response && typeof response === 'object' && 'data' in response && Array.isArray(response.data)) {
        console.log('响应是带 data 属性的对象格式');
        const convertedQuestions = response.data.map(convertApiQuestion);
        questions.value = convertedQuestions;
        return convertedQuestions;
      }

      // 如果是对象且有 questions 属性，并且 questions 是数组
      if (response && typeof response === 'object' && 'questions' in response && Array.isArray(response.questions)) {
        console.log('响应是带 questions 属性的对象格式');
        const convertedQuestions = response.questions.map(convertApiQuestion);
        questions.value = convertedQuestions;
        return convertedQuestions;
      }

      // 如果是对象且有 items 属性，并且 items 是数组
      if (response && typeof response === 'object' && 'items' in response && Array.isArray(response.items)) {
        console.log('响应是带 items 属性的对象格式');
        const convertedQuestions = response.items.map(convertApiQuestion);
        questions.value = convertedQuestions;
        return convertedQuestions;
      }

      // 如果是对象且有 results 属性，并且 results 是数组
      if (response && typeof response === 'object' && 'results' in response && Array.isArray(response.results)) {
        console.log('响应是带 results 属性的对象格式');
        const convertedQuestions = response.results.map(convertApiQuestion);
        questions.value = convertedQuestions;
        return convertedQuestions;
      }

      // 如果是对象且有 content 属性，并且 content 是数组
      if (response && typeof response === 'object' && 'content' in response && Array.isArray(response.content)) {
        console.log('响应是带 content 属性的对象格式');
        const convertedQuestions = response.content.map(convertApiQuestion);
        questions.value = convertedQuestions;
        return convertedQuestions;
      }

      // 如果没有有效数据，返回测试数据
      console.warn('获取题目响应格式异常，使用测试数据:', response);
      questions.value = testData;
      return testData;
    } catch (error: any) {
      console.error('获取题目失败，使用测试数据:', error);
      // 使用测试数据代替
      questions.value = testData;
      return testData;
    }
  },

  async getQuestionById(id: number): Promise<TableQuestion> {
    try {
      const response = await retryRequest(() => getQuestionById(id))

      // 如果是对象且有所需属性
      if (response && typeof response === 'object') {
        return convertApiQuestion(response)
      }

      throw new Error('获取题目详情失败: 响应格式异常')
    } catch (error: any) {
      ElMessage.error(`获取题目详情失败: ${error.message}`)
      throw error
    }
  },

  async addQuestion(data: any) {
    try {
      // 预处理：保证单选题和判断题只有一个正确答案
      if (data.type === QuestionType.Single) {
        // 处理单选题
        if (!data.correctAnswer) {
          throw new Error('单选题必须有正确答案')
        }

        // 如果是字母形式的答案（A, B, C）
        if (/^[A-Z]$/.test(data.correctAnswer)) {
          const index = data.correctAnswer.charCodeAt(0) - 65
          if (!data.options || index < 0 || index >= data.options.length) {
            throw new Error('正确答案必须是有效的选项索引')
          }
          // 创建 answers 数组，存储选项内容而不是字母
          data.answers = [data.options[index]]
        } else if (Array.isArray(data.answers) && data.answers.length > 0) {
          // 如果已经有 answers 数组，确保只取第一个
          data.answers = [data.answers[0]]
        } else {
          // 如果没有 answers 数组，使用 correctAnswer
          data.answers = [data.correctAnswer]
        }
      } else if (data.type === QuestionType.Judge) {
        // 处理判断题
        if (!data.correctAnswer || !['正确', '错误'].includes(data.correctAnswer)) {
          throw new Error('判断题答案必须是"正确"或"错误"')
        }
        data.answers = [data.correctAnswer]
      } else if (data.type === QuestionType.ShortAnswer) {
        // 处理简答题
        if (!data.referenceAnswer) {
          throw new Error('简答题必须提供参考答案')
        }
        // 简答题不需要特殊处理 answers
      }

      // 验证题目数据
      console.log('验证题目数据:', data.type, QuestionType.ShortAnswer)
      const errors = validateQuestion(data)
      if (errors.length > 0) {
        console.error('验证错误:', errors)
        throw new Error(errors.join(', '))
      }

      // 转换为 API 格式
      const apiData = {
        type: data.type,
        content: data.question,
        optionsJson: data.options ? JSON.stringify(data.options) : null,
        answersJson: JSON.stringify(
          data.type === QuestionType.Judge
            ? [data.correctAnswer === 'true' ? '正确' : (data.correctAnswer === 'false' ? '错误' : data.correctAnswer)]
            : data.type === QuestionType.Single
              ? (Array.isArray(data.correctAnswer) ? data.correctAnswer : [data.correctAnswer]).map((ans: string) => {
                  // 如果是字母索引，转换为选项内容
                  if (/^[A-Z]$/.test(ans)) {
                    const index = ans.charCodeAt(0) - 65;
                    return data.options?.[index] ?? ans;
                  }
                  return ans;
                })
              : data.type === QuestionType.Fill
                ? (Array.isArray(data.answers) ? data.answers : [data.answers])
                : data.type === QuestionType.ShortAnswer
                  ? [] // 简答题不需要 answersJson，使用 referenceAnswer 字段
                  : Array.isArray(data.answers) ? data.answers : []
        ),
        analysis: data.analysis,
        examplesJson: data.sampleInput && data.sampleOutput ?
          JSON.stringify({ input: data.sampleInput, output: data.sampleOutput }) : null,
        referenceAnswer: data.referenceAnswer,
        difficulty: data.difficulty || 1,
        category: data.category,
        tagsJson: data.tags ? JSON.stringify(data.tags) : null
      }

      const response = await retryRequest(() => addQuestion(apiData))

      // 如果是对象且有所需属性
      if (response && typeof response === 'object') {
        const convertedQuestion = convertApiQuestion(response)
        questions.value = [...questions.value, convertedQuestion]
        return convertedQuestion
      }

      throw new Error('添加题目失败: 响应格式异常')
    } catch (error: any) {
      ElMessage.error(`添加题目失败: ${error.message}`)
      throw error
    }
  },

  async updateQuestion(data: any) {
    try {
      // 预处理：保证单选题和判断题只有一个正确答案
      if (data.type === QuestionType.Single) {
        // 处理单选题
        if (!data.correctAnswer) {
          throw new Error('单选题必须有正确答案')
        }

        // 如果是字母形式的答案（A, B, C）
        if (/^[A-Z]$/.test(data.correctAnswer)) {
          const index = data.correctAnswer.charCodeAt(0) - 65
          if (!data.options || index < 0 || index >= data.options.length) {
            throw new Error('正确答案必须是有效的选项索引')
          }
          // 创建 answers 数组，存储选项内容而不是字母
          data.answers = [data.options[index]]
        } else if (Array.isArray(data.answers) && data.answers.length > 0) {
          // 如果已经有 answers 数组，确保只取第一个
          data.answers = [data.answers[0]]
        } else {
          // 如果没有 answers 数组，使用 correctAnswer
          data.answers = [data.correctAnswer]
        }
      } else if (data.type === QuestionType.Judge) {
        // 处理判断题
        if (!data.correctAnswer || !['正确', '错误'].includes(data.correctAnswer)) {
          throw new Error('判断题答案必须是"正确"或"错误"')
        }
        data.answers = [data.correctAnswer]
      } else if (data.type === QuestionType.ShortAnswer) {
        // 处理简答题
        if (!data.referenceAnswer) {
          throw new Error('简答题必须提供参考答案')
        }
        // 简答题不需要特殊处理 answers
      }

      // 验证题目数据
      console.log('更新题目数据:', data.type, QuestionType.ShortAnswer)
      const errors = validateQuestion(data)
      if (errors.length > 0) {
        console.error('更新验证错误:', errors)
        throw new Error(errors.join(', '))
      }

      // 转换为 API 格式
      const apiData = {
        id: data.id,
        type: data.type,
        content: data.question,
        optionsJson: data.options ? JSON.stringify(data.options) : null,
        answersJson: JSON.stringify(
          data.type === QuestionType.Judge
            ? [data.correctAnswer === 'true' ? '正确' : (data.correctAnswer === 'false' ? '错误' : data.correctAnswer)]
            : data.type === QuestionType.Single
              ? (Array.isArray(data.correctAnswer) ? data.correctAnswer : [data.correctAnswer]).map((ans: string) => {
                  // 如果是字母索引，转换为选项内容
                  if (/^[A-Z]$/.test(ans)) {
                    const index = ans.charCodeAt(0) - 65;
                    return data.options?.[index] ?? ans;
                  }
                  return ans;
                })
              : data.type === QuestionType.Fill
                ? (Array.isArray(data.answers) ? data.answers : [data.answers])
                : data.type === QuestionType.ShortAnswer
                  ? [] // 简答题不需要 answersJson，使用 referenceAnswer 字段
                  : Array.isArray(data.answers) ? data.answers : []
        ),
        analysis: data.analysis,
        examplesJson: data.sampleInput && data.sampleOutput ?
          JSON.stringify({ input: data.sampleInput, output: data.sampleOutput }) : null,
        referenceAnswer: data.referenceAnswer,
        difficulty: data.difficulty || 1,
        category: data.category,
        tagsJson: data.tags ? JSON.stringify(data.tags) : null
      }

      const response = await retryRequest(() => updateQuestion(apiData))

      // 如果是204 No Content响应，说明更新成功但没有返回数据
      if (response && Object.keys(response).length === 0) {
        // 使用原始数据更新缓存
        const index = questions.value.findIndex(q => q.id === data.id)
        if (index !== -1) {
          // 将编辑表单数据转换为表格数据格式
          const updatedQuestion: TableQuestion = {
            id: data.id,
            type: data.type,
            question: data.question,
            options: data.options || [],
            correctAnswer: data.correctAnswer,
            answers: data.answers || [],
            analysis: data.analysis,
            sampleInput: data.sampleInput,
            sampleOutput: data.sampleOutput,
            referenceAnswer: data.referenceAnswer,
            createTime: data.createTime,
            category: data.category,
            tags: data.tags || []
          }
          questions.value[index] = updatedQuestion
        }
        return data // 返回原始数据
      }

      // 如果有返回数据，处理正常响应
      if (response && response.success) {
        const convertedQuestion = convertApiQuestion(response.data)
        const index = questions.value.findIndex(q => q.id === data.id)
        if (index !== -1) {
          questions.value[index] = convertedQuestion
        }
        return convertedQuestion
      }
      throw new Error(response.message || '更新题目失败')
    } catch (error: any) {
      ElMessage.error(`更新题目失败: ${error.message}`)
      throw error
    }
  },

  async deleteQuestion(id: number) {
    try {
      const response = await retryRequest(() => deleteQuestion(id))

      // 如果是204 No Content响应或者成功响应
      if (response && (Object.keys(response).length === 0 || response.success)) {
        questions.value = questions.value.filter(q => q.id !== id)
        return true
      }

      throw new Error('删除题目失败: 响应格式异常')
    } catch (error: any) {
      ElMessage.error(`删除题目失败: ${error.message}`)
      throw error
    }
  }
}
