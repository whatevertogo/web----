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
interface ApiResponse<T> {
  success: boolean;
  data: T[];
  message?: string;
}

interface SingleResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

interface SearchParams {
  keyword?: string;
  type?: QuestionType;
  dateRange?: [Date, Date];
}


// 缓存题目列表
const questions = ref<TableQuestion[]>([])

// 将 API 返回的题目格式转换为前端使用的格式
export const convertApiQuestion = (q: ApiQuestion): TableQuestion => {
  let options = q.optionsJson ? JSON.parse(q.optionsJson) : [];
  const answers = q.answersJson ? JSON.parse(q.answersJson) : [];
  const examples = q.examplesJson ? JSON.parse(q.examplesJson) : null;
  const tags = q.tagsJson ? JSON.parse(q.tagsJson) : [];

  // 自动识别题型，覆盖后端错误type
  if (
    answers.length === 1 &&
    (answers[0] === '正确' || answers[0] === '错误')
  ) {
    q.type = QuestionType.Judge;
    if (!options || options.length === 0) {
      options = ["正确", "错误"];
    }
  } else if (
    options && options.length > 0 &&
    answers.length === 1
  ) {
    q.type = QuestionType.Single;
  } else if (
    options && options.length > 0 &&
    answers.length > 1
  ) {
    // 预留多选题类型，当前未定义
    // q.type = QuestionType.Multiple;
  } else if (
    Array.isArray(answers) && answers.length > 0 &&
    (!options || options.length === 0)
  ) {
    q.type = QuestionType.Fill;
  } else if (
    !options || options.length === 0
  ) {
    // 无选项，视为编程题或简答题
    // 这里默认不覆盖，保持后端类型
  }

  // 判断题补全选项
  if (
    (!options || options.length === 0) && q.type === QuestionType.Judge
  ) {
    options = ["正确", "错误"];
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
  
  async getQuestions(params?: SearchParams): Promise<TableQuestion[]> {
    try {
      const response = await retryRequest(() => getQuestions(params)) as ApiResponse<ApiQuestion>;
      if (response.success) {
        const convertedQuestions = response.data.map(convertApiQuestion);
        questions.value = convertedQuestions;
        return convertedQuestions;
      }
      throw new Error(response.message || '获取题目失败');
    } catch (error: any) {
      console.error('获取题目失败:', error);
      ElMessage.error(`获取题目失败: ${error.message}`);
      throw error;
    }
  },

  async getQuestionById(id: number): Promise<TableQuestion> {
    try {
      const response = await getQuestionById(id) as SingleResponse<ApiQuestion>
      if (response.success) {
        return convertApiQuestion(response.data)
      }
      throw new Error(response.message || '获取题目详情失败')
    } catch (error: any) {
      ElMessage.error(`获取题目详情失败: ${error.message}`)
      throw error
    }
  },

  async addQuestion(data: any) {
    try {
      // 预处理：保证单选题和判断题只有一个正确答案
      if (data.type === QuestionType.Single || data.type === QuestionType.Judge) {
        if (Array.isArray(data.answers) && data.answers.length > 0) {
          data.answers = [data.answers[0]];
        }
        if (Array.isArray(data.correctAnswer) && data.correctAnswer.length > 0) {
          data.correctAnswer = [data.correctAnswer[0]];
        }
        // 如果answers为空，则用correctAnswer填充
        if (!data.answers || data.answers.length === 0) {
          data.answers = Array.isArray(data.correctAnswer) ? data.correctAnswer : [data.correctAnswer];
        }
      }

      // 验证题目数据
      const errors = validateQuestion(data)
      if (errors.length > 0) {
        throw new Error(errors.join(', '))
      }

      // 转换为 API 格式
      const apiData = {
        type: data.type,
        content: data.question,
        optionsJson: data.options ? JSON.stringify(data.options) : null,
        answersJson: JSON.stringify(
          (Array.isArray(data.correctAnswer) ? data.correctAnswer : [data.correctAnswer]).map((ans: string) => {
            const index = ans.charCodeAt(0) - 65;
            return data.options?.[index] ?? ans;
          })
        ),
        analysis: data.analysis,
        examplesJson: data.sampleInput && data.sampleOutput ?
          JSON.stringify({ input: data.sampleInput, output: data.sampleOutput }) : null,
        referenceAnswer: data.referenceAnswer,
        difficulty: data.difficulty || 1,
        category: data.category,
        tagsJson: data.tags ? JSON.stringify(data.tags) : null
      }

      const response = await addQuestion(apiData) as SingleResponse<ApiQuestion>
      if (response.success) {
        const convertedQuestion = convertApiQuestion(response.data)
        questions.value = [...questions.value, convertedQuestion]
        return convertedQuestion
      }
      throw new Error(response.message || '添加题目失败')
    } catch (error: any) {
      ElMessage.error(`添加题目失败: ${error.message}`)
      throw error
    }
  },

  async updateQuestion(data: any) {
    try {
      // 预处理：保证单选题和判断题只有一个正确答案
      if (data.type === QuestionType.Single || data.type === QuestionType.Judge) {
        if (Array.isArray(data.answers) && data.answers.length > 0) {
          data.answers = [data.answers[0]];
        }
        if (Array.isArray(data.correctAnswer) && data.correctAnswer.length > 0) {
          data.correctAnswer = [data.correctAnswer[0]];
        }
      }

      // 验证题目数据
      const errors = validateQuestion(data)
      if (errors.length > 0) {
        throw new Error(errors.join(', '))
      }      // 转换为 API 格式
      const apiData = {
        id: data.id,
        type: data.type,
        content: data.question,
        optionsJson: data.options ? JSON.stringify(data.options) : null,
        answersJson: JSON.stringify(
          data.type === QuestionType.Judge 
            ? [data.correctAnswer] // 判断题直接使用"正确"/"错误"
            : data.type === QuestionType.Single
              ? [data.correctAnswer] // 单选题保持原样
              : Array.isArray(data.correctAnswer) ? data.correctAnswer : [data.correctAnswer]
        ),
        analysis: data.analysis,
        examplesJson: data.sampleInput && data.sampleOutput ?
          JSON.stringify({ input: data.sampleInput, output: data.sampleOutput }) : null,
        referenceAnswer: data.referenceAnswer,
        difficulty: data.difficulty || 1,
        category: data.category,
        tagsJson: data.tags ? JSON.stringify(data.tags) : null
      }

      const response = await updateQuestion(apiData) as SingleResponse<ApiQuestion>
      if (response.success) {
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
      const response = await deleteQuestion(id) as SingleResponse<boolean>
      if (response.success) {
        questions.value = questions.value.filter(q => q.id !== id)
        return true
      }
      throw new Error(response.message || '删除题目失败')
    } catch (error: any) {
      ElMessage.error(`删除题目失败: ${error.message}`)
      throw error
    }
  }
}