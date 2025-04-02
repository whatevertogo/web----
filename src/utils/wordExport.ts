import Docxtemplater from 'docxtemplater'
import PizZip from 'pizzip'
import { saveAs } from 'file-saver'
import { QuestionType } from '../types/question'

// 加载 Word 模板
const loadTemplate = async () => {
  try {
    const response = await fetch('/templates/question_template.docx')
    if (!response.ok) {
      throw new Error('模板文件加载失败')
    }
    const blob = await response.blob()
    return await blob.arrayBuffer()
  } catch (error) {
    console.error('加载模板失败:', error)
    throw error
  }
}

// 格式化题目数据
const formatQuestionData = (questions: any[], typeMap?: Record<string, string>) => {
  return questions.map((q, index) => ({
    index: index + 1,
    type: q.type,
    question: q.question,
    content: formatQuestionContent(q, typeMap),
    analysis: q.analysis || ''
  }))
}

// 根据题型格式化内容
const formatQuestionContent = (question: any, typeMap?: Record<string, string>) => {
  const questionType = typeMap ? typeMap[question.type] : question.type;
  switch (questionType) {
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

// 格式化各题型内容
const formatSingleChoice = (question: any) => {
  const options = question.options.map((opt: string, idx: number) => 
    `${String.fromCharCode(65 + idx)}. ${opt}`
  ).join('\n')
  return `${options}\n正确答案：${question.correctAnswer}`
}

const formatJudge = (question: any) => 
  `正确答案：${question.correctAnswer === 'true' ? '正确' : '错误'}`

const formatFill = (question: any) => 
  `答案：${question.answers.join('，')}`

const formatProgram = (question: any) => 
  `示例输入：\n${question.sampleInput}\n\n示例输出：\n${question.sampleOutput}`

const formatShortAnswer = (question: any) => 
  `参考答案：\n${question.referenceAnswer}`

// 导出为 Word
export const exportToWord = async (questions: any[], typeMap?: Record<string, string>) => {
  try {
    const template = await loadTemplate()
    const zip = new PizZip(template)
    
    const doc = new Docxtemplater(zip, {
      paragraphLoop: true,
      linebreaks: true
    })

    // 准备数据
    const data = {
      title: '试题导出',
      date: new Date().toLocaleDateString(),
      questions: formatQuestionData(questions, typeMap)
    }

    // 渲染文档
    doc.render(data)

    // 生成 blob
    const blob = doc.getZip().generate({
      type: 'blob',
      mimeType: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document'
    })

    // 下载文件
    saveAs(blob, `试题导出_${new Date().toLocaleDateString()}.docx`)
    
    return true
  } catch (error) {
    console.error('Word导出失败:', error)
    if (error.properties && error.properties.errors) {
      console.error('模板错误:', error.properties.errors)
    }
    throw error
  }
} 