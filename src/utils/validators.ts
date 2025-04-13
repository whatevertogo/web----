import { QuestionType } from '../types/question'

// 针对不同题目类型的验证规则
export const questionValidators = {  // 单选题验证
  [QuestionType.Single]: (question: any) => {
    const errors: string[] = []
    if (!question.question) {
      errors.push('题目内容不能为空')
    }
    if (!question.options || !Array.isArray(question.options) || question.options.length < 2) {
      errors.push('单选题至少需要2个选项')
    }

    // 检查是否有空选项
    if (question.options && question.options.some((opt: string) => !opt || opt.trim() === '')) {
      errors.push('选项内容不能为空')
    }

    // 检查correctAnswer属性
    if (!question.correctAnswer) {
      errors.push('单选题必须有正确答案')
    }

    // 如果是字母形式的答案（A, B, C），检查是否有效
    if (question.correctAnswer && /^[A-Z]$/.test(question.correctAnswer)) {
      const index = question.correctAnswer.charCodeAt(0) - 65
      if (!question.options || index < 0 || index >= question.options.length) {
        errors.push('正确答案必须是有效的选项索引')
      }
    }
    // 兼容性检查answers数组
    else if (question.answers && Array.isArray(question.answers) && question.answers.length === 1) {
      if (question.options && !question.options.includes(question.answers[0])) {
        errors.push('正确答案必须是选项之一')
      }
    }

    return errors
  },
  // 判断题验证
  [QuestionType.Judge]: (question: any) => {
    const errors: string[] = []
    if (!question.question) {
      errors.push('题目内容不能为空')
    }
    // 检查 correctAnswer 属性
    if (!question.correctAnswer || !['正确', '错误'].includes(question.correctAnswer)) {
      errors.push('判断题答案必须是"正确"或"错误"')
    }
    // 兼容性检查 answers 数组
    if (question.answers && Array.isArray(question.answers) && question.answers.length > 0) {
      if (!['正确', '错误'].includes(question.answers[0])) {
        errors.push('判断题答案必须是"正确"或"错误"')
      }
    }
    return errors
  },

  // 填空题验证
  [QuestionType.Fill]: (question: any) => {
    const errors: string[] = []
    if (!question.question) {
      errors.push('题目内容不能为空')
    }
    if (!question.answers || !Array.isArray(question.answers) || question.answers.length === 0) {
      errors.push('填空题必须至少有一个答案')
    }
    return errors
  },
  // 编程题验证
  [QuestionType.Program]: (question: any) => {
    const errors: string[] = []
    if (!question.question) {
      errors.push('题目内容不能为空')
    }
    if (!question.sampleInput || !question.sampleOutput) {
      errors.push('编程题必须包含示例输入和输出')
    }
    if (!question.referenceAnswer) {
      errors.push('编程题必须包含参考答案')
    }
    return errors
  },
  // 简答题验证
  [QuestionType.ShortAnswer]: (question: any) => {
    const errors: string[] = []
    if (!question.question) {
      errors.push('题目内容不能为空')
    }
    if (!question.referenceAnswer) {
      errors.push('简答题必须包含参考答案')
    }
    return errors
  }
}

// 通用的题目验证函数
export const validateQuestion = (question: any) => {
  const validator = questionValidators[question.type]
  if (!validator) {
    return ['未知的题目类型']
  }
  return validator(question)
}
