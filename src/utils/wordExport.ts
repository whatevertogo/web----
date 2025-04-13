import { saveAs } from 'file-saver'
import { QuestionType } from '../types/question'

// 导出为 Word
export const exportToWord = async (questions: any[], typeMap?: Record<string, string>) => {
  if (!questions || !Array.isArray(questions) || questions.length === 0) {
    throw new Error('没有可导出的题目')
  }

  try {
    // 生成 HTML 内容
    const htmlContent = generateHtmlContent(questions, typeMap)

    // 将 HTML 转换为 Blob
    const blob = new Blob([htmlContent], { type: 'application/msword' })

    // 下载文件
    const fileName = `题库导出_${new Date().toLocaleDateString('zh-CN').replace(/\//g, '-')}.doc`
    saveAs(blob, fileName)

    return true
  } catch (error) {
    console.error('Word导出失败:', error)
    throw new Error(`Word导出失败: ${error.message || '未知错误'}`)
  }
}

// 生成 HTML 内容
const generateHtmlContent = (questions: any[], typeMap?: Record<string, string>) => {
  // 添加基本的 HTML 结构和样式
  let html = `
    <html xmlns:v="urn:schemas-microsoft-com:vml" xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:w="urn:schemas-microsoft-com:office:word" xmlns:m="http://schemas.microsoft.com/office/omml/2004/12/core">
    <head>
      <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
      <title>题库导出</title>
      <style>
        body { font-family: SimSun, Arial, sans-serif; }
        .title { font-size: 18pt; font-weight: bold; text-align: center; margin: 20pt 0; }
        .date { font-size: 10pt; text-align: right; margin-bottom: 20pt; }
        .question { margin-bottom: 15pt; }
        .question-text { font-weight: bold; }
        .options { margin-left: 20pt; }
        .answer { margin-left: 20pt; }
        .analysis { margin-left: 20pt; color: #666; margin-bottom: 15pt; }
        table { border-collapse: collapse; width: 100%; }
        td, th { border: 1px solid #ddd; padding: 8px; text-align: left; }
      </style>
    </head>
    <body>
      <div class="title">试题导出</div>
      <div class="date">导出日期：${new Date().toLocaleDateString('zh-CN')}</div>
  `

  // 添加题目内容
  questions.forEach((question, index) => {
    const type = typeMap ? typeMap[question.type] : question.type
    html += `<div class="question">
      <div class="question-text">${index + 1}. [${type}] ${escapeHtml(question.question)}</div>
      ${formatQuestionContent(question)}
    </div>`
  })

  html += '</body></html>'
  return html
}

// HTML 转义
const escapeHtml = (text: string): string => {
  if (text === null || text === undefined) return ''
  return String(text)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;')
}

// 确保输入是字符串
const toString = (value: any): string => {
  if (value === null || value === undefined) return ''
  return String(value)
}

// 根据题型格式化内容
const formatQuestionContent = (question: any): string => {
  switch (question.type) {
    case QuestionType.Single:
      return formatSingleChoice(question)
    case QuestionType.Judge:
      return formatJudge(question)
    case QuestionType.Fill:
      return formatFill(question)
    case QuestionType.Program:
      return formatProgram(question)
    case QuestionType.ShortAnswer:
      return formatShortAnswer(question)
    default:
      return ''
  }
}

// 格式化各题型内容，使用 HTML 格式
const formatSingleChoice = (question: any): string => {
  if (!question.options || !Array.isArray(question.options)) {
    return '<div class="answer">选项数据无效</div>'
  }

  let html = '<div class="options">'
  question.options.forEach((opt: string, idx: number) => {
    html += `<div>${String.fromCharCode(65 + idx)}. ${escapeHtml(toString(opt))}</div>`
  })
  html += '</div>'
  html += `<div class="answer">正确答案：${escapeHtml(toString(question.correctAnswer || '未设置'))}</div>`

  if (question.analysis) {
    html += `<div class="analysis">解析：${escapeHtml(toString(question.analysis))}</div>`
  }

  return html
}

const formatJudge = (question: any): string => {
  let html = '<div class="options">'
  html += '<div>A. 正确</div>'
  html += '<div>B. 错误</div>'
  html += '</div>'
  html += `<div class="answer">正确答案：${question.correctAnswer}</div>`

  if (question.analysis) {
    html += `<div class="analysis">解析：${escapeHtml(toString(question.analysis))}</div>`
  }

  return html
}

const formatFill = (question: any): string => {
  if (!question.answers || !Array.isArray(question.answers)) {
    return '<div class="answer">答案数据无效</div>'
  }

  let html = '<div class="answer">答案：'
  question.answers.forEach((ans: string, idx: number) => {
    html += `<div>空${idx + 1}：${escapeHtml(toString(ans))}</div>`
  })
  html += '</div>'

  if (question.analysis) {
    html += `<div class="analysis">解析：${escapeHtml(toString(question.analysis))}</div>`
  }

  return html
}

const formatProgram = (question: any): string => {
  let html = ''
  html += `<div class="answer">示例输入：<pre>${escapeHtml(toString(question.sampleInput || '无'))}</pre></div>`
  html += `<div class="answer">示例输出：<pre>${escapeHtml(toString(question.sampleOutput || '无'))}</pre></div>`

  if (question.analysis) {
    html += `<div class="analysis">解析：${escapeHtml(toString(question.analysis))}</div>`
  }

  return html
}

const formatShortAnswer = (question: any): string => {
  let html = `<div class="answer">参考答案：<div>${escapeHtml(toString(question.referenceAnswer || '无'))}</div></div>`

  if (question.analysis) {
    html += `<div class="analysis">解析：${escapeHtml(toString(question.analysis))}</div>`
  }

  return html
}