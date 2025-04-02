import type { FormRules } from 'element-plus'

// 通用验证规则
const commonRules = {
  question: [
    { required: true, message: '请输入题目内容', trigger: 'blur' },
    { min: 2, message: '题目内容至少2个字符', trigger: 'blur' }
  ],
  analysis: [
    { required: true, message: '请输入答案解析', trigger: 'blur' }
  ]
}

// 各题型特定验证规则
export const formRules: Record<string, FormRules> = {
  single: {
    question: commonRules.question,
    options: [
      { required: true, message: '请输入选项内容', trigger: 'blur' },
      { 
        validator: (_, value, callback) => {
          if (value.some(v => !v.trim())) {
            callback(new Error('选项内容不能为空'))
          } else if (new Set(value).size !== value.length) {
            callback(new Error('选项内容不能重复'))
          } else {
            callback()
          }
        },
        trigger: 'blur'
      }
    ],
    correctAnswer: [
      { required: true, message: '请选择正确答案', trigger: 'change' }
    ],
    analysis: commonRules.analysis
  },
  
  judge: {
    question: commonRules.question,
    correctAnswer: [
      { required: true, message: '请选择正确答案', trigger: 'change' }
    ],
    analysis: commonRules.analysis
  },
  
  fill: {
    question: [
      ...commonRules.question,
      {
        validator: (_, value, callback) => {
          if (!value.includes('_')) {
            callback(new Error('题目内容必须包含填空符号"_"'))
          } else {
            callback()
          }
        },
        trigger: 'blur'
      }
    ],
    answers: [
      { required: true, message: '请输入答案', trigger: 'blur' },
      {
        validator: (_, value, callback) => {
          if (value.some(v => !v.trim())) {
            callback(new Error('答案不能为空'))
          } else {
            callback()
          }
        },
        trigger: 'blur'
      }
    ],
    analysis: commonRules.analysis
  },
  
  program: {
    question: commonRules.question,
    sampleInput: [
      { required: true, message: '请输入示例输入', trigger: 'blur' }
    ],
    sampleOutput: [
      { required: true, message: '请输入示例输出', trigger: 'blur' }
    ],
    analysis: commonRules.analysis
  },
  
  shortAnswer: {
    question: commonRules.question,
    referenceAnswer: [
      { required: true, message: '请输入参考答案', trigger: 'blur' },
      { min: 10, message: '参考答案至少10个字符', trigger: 'blur' }
    ],
    analysis: commonRules.analysis
  }
} 