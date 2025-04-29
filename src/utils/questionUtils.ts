import { QuestionType } from '../types/question';

/**
 * 初始化题型特有字段
 * @param type 题型
 * @param isNew 是否是新题目
 * @param form 表单对象
 */
export function initializeQuestionFields(type: QuestionType, isNew: boolean, form: any): void {
  switch (type) {
    case QuestionType.Single:
      form.options = isNew ? ['选项A', '选项B', '选项C', '选项D'] : form.options || ['', ''];
      form.correctAnswer = isNew ? 'A' : form.correctAnswer || '';
      form.analysis = isNew ? '请输入解析' : form.analysis || '';
      break;
    case QuestionType.Judge:
      form.correctAnswer = form.correctAnswer || '正确';
      form.analysis = form.analysis || '请输入解析';
      break;
    case QuestionType.Fill:
      form.answers = isNew ? ['答案一', '答案二'] : form.answers || [''];
      form.analysis = isNew ? '请输入解析' : form.analysis || '';
      break;
    case QuestionType.Program:
      if (isNew) {
        form.sampleInput = 'function add(a, b) {\n  // 请实现该函数\n}';
        form.sampleOutput = 'function add(a, b) {\n  return a + b;\n}';
        form.referenceAnswer = 'function add(a, b) {\n  return a + b;\n}';
        form.analysis = '这是一个简单的加法函数实现';
      } else {
        form.sampleInput = form.sampleInput || '';
        form.sampleOutput = form.sampleOutput || '';
        form.referenceAnswer = form.referenceAnswer || '';
      }
      break;
    case QuestionType.ShortAnswer:
      form.referenceAnswer = isNew ? '参考答案内容' : form.referenceAnswer || '';
      form.analysis = isNew ? '解析说明' : form.analysis || '';
      break;
  }
}

/**
 * 验证表单字段
 * @param form 表单对象
 * @returns 是否通过验证
 */
export function validateForm(form: any): boolean {
  if (!form.question || form.question.trim() === '') {
    throw new Error('题目内容不能为空');
  }

  switch (form.type) {
    case QuestionType.Single:
      if (!form.options || form.options.length < 2) {
        throw new Error('单选题至少需要两个选项');
      }
      if (!form.correctAnswer) {
        throw new Error('请选择正确答案');
      }
      break;
    case QuestionType.Judge:
      if (!form.correctAnswer) {
        throw new Error('请选择正确答案');
      }
      break;
    case QuestionType.Fill:
      if (!form.answers || form.answers.length === 0) {
        throw new Error('填空题至少需要一个答案');
      }
      break;
    case QuestionType.Program:
      if (!form.sampleInput || !form.sampleOutput) {
        throw new Error('编程题需要提供示例输入和输出');
      }
      if (!form.referenceAnswer) {
        throw new Error('编程题需要提供参考答案');
      }
      break;
    case QuestionType.ShortAnswer:
      if (!form.referenceAnswer) {
        throw new Error('简答题需要提供参考答案');
      }
      break;
  }

  return true;
}
