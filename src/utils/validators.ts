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
    if (!question.answers || !Array.isArray(question.answers) || question.answers.length !== 1) {
      errors.push('单选题必须有且仅有一个正确答案')
    }
    if (question.answers && question.options && !question.options.includes(question.answers[0])) {
      errors.push('正确答案必须是选项之一')
    }
    return errors
  },
  // 判断题验证
  [QuestionType.Judge]: (question: any) => {
    const errors: string[] = []
    if (!question.question) {
      errors.push('题目内容不能为空')
    }
    if (!question.answers || !Array.isArray(question.answers) || question.answers.length !== 1) {
      errors.push('判断题必须有且仅有一个答案')
    }
    if (question.answers && !['正确', '错误'].includes(question.answers[0])) {
      errors.push('判断题答案必须是"正确"或"错误"')
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
