<script setup lang="ts">
import { useRouter } from 'vue-router'
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import {
  QuestionType,
  SingleQuestionForm,
  JudgeQuestionForm,
  FillQuestionForm,
  ProgramQuestionForm,
  ShortAnswerQuestionForm
} from '../types/question'
import { questionService } from '../services/questionService'

const router = useRouter()
const activeTab = ref('single')

const singleForm = reactive<SingleQuestionForm>({
  type: QuestionType.Single,
  question: '',
  options: ['', '', '', ''],
  correctAnswer: '',
  analysis: ''
})

const judgeForm = reactive<JudgeQuestionForm>({
  type: QuestionType.Judge,
  question: '',
  correctAnswer: '正确',
  analysis: ''
})

const fillForm = reactive<FillQuestionForm>({
  type: QuestionType.Fill,
  question: '',
  answers: [''],
  analysis: ''
})

const programForm = reactive<ProgramQuestionForm>({
  type: QuestionType.Program,
  question: '',
  sampleInput: '',
  sampleOutput: '',
  referenceAnswer: '',
  analysis: ''
})

const shortAnswerForm = reactive<ShortAnswerQuestionForm>({
  type: QuestionType.ShortAnswer,
  question: '',
  referenceAnswer: '',
  analysis: ''
})

const addOption = () => {
  if (singleForm.options.length < 6) {
    singleForm.options.push('')
  }
}

const removeOption = (index: number) => {
  if (singleForm.options.length > 2) {
    singleForm.options.splice(index, 1)
  }
}

const addFillBlank = () => {
  if (fillForm.answers.length < 5) {
    fillForm.answers.push('')
  }
}

const removeFillBlank = (index: number) => {
  if (fillForm.answers.length > 1) {
    fillForm.answers.splice(index, 1)
  }
}

const updateFillAnswer = (index: number, value: string) => {
  fillForm.answers[index] = value
}

// 添加表单验证规则
const rules = {
  single: {
    question: [{ required: true, message: '请输入题目内容', trigger: 'blur' }],
    options: [{ required: true, message: '请输入选项内容', trigger: 'blur' }],
    correctAnswer: [{ required: true, message: '请选择正确答案', trigger: 'change' }]
  },
  judge: {
    question: [{ required: true, message: '请输入题目内容', trigger: 'blur' }],
    correctAnswer: [{ required: true, message: '请选择正确答案', trigger: 'change' }]
  },
  fill: {
    question: [{ required: true, message: '请输入题目内容', trigger: 'blur' }],
    answers: [{ required: true, message: '请输入填空答案', trigger: 'blur' }]
  },
  program: {
    question: [{ required: true, message: '请输入题目内容', trigger: 'blur' }],
    sampleInput: [{ required: true, message: '请输入示例输入', trigger: 'blur' }],
    sampleOutput: [{ required: true, message: '请输入示例输出', trigger: 'blur' }],
    referenceAnswer: [{ required: true, message: '请输入参考答案', trigger: 'blur' }]
  },
  shortAnswer: {
    question: [{ required: true, message: '请输入题目内容', trigger: 'blur' }],
    referenceAnswer: [{ required: true, message: '请输入参考答案', trigger: 'blur' }]
  }
}

// 表单引用
const singleFormRef = ref()
const judgeFormRef = ref()
const fillFormRef = ref()
const programFormRef = ref()
const shortAnswerFormRef = ref()

const submitQuestion = async (formType: string) => {
  let formRef: any
  let formData: any

  switch (formType) {
    case 'single':
      formRef = singleFormRef.value
      formData = { ...singleForm, type: QuestionType.Single }
      break
    case 'judge':
      formRef = judgeFormRef.value
      formData = { ...judgeForm, type: QuestionType.Judge }
      break
    case 'fill':
      formRef = fillFormRef.value
      formData = { ...fillForm, type: QuestionType.Fill }
      break
    case 'program':
      formRef = programFormRef.value
      formData = { ...programForm, type: QuestionType.Program }
      break
    case 'shortAnswer':
      formRef = shortAnswerFormRef.value
      formData = { ...shortAnswerForm, type: QuestionType.ShortAnswer }
      break
  }

  try {
    if (!formRef) return

    // 表单验证
    const valid = await formRef.validate().catch((err: any) => {
      console.error('表单验证错误:', err)
      return false
    })

    if (!valid) {
      ElMessage.error('表单验证失败，请检查必填项')
      return
    }

    // 根据题型进行额外验证
    if (formType === 'single') {
      // 验证单选题
      if (!formData.options || formData.options.some((opt: string) => !opt || opt.trim() === '')) {
        ElMessage.error('选项内容不能为空')
        return
      }
      if (!formData.correctAnswer) {
        ElMessage.error('请选择正确答案')
        return
      }
    } else if (formType === 'fill') {
      // 验证填空题
      if (!formData.answers || formData.answers.some((ans: string) => !ans || ans.trim() === '')) {
        ElMessage.error('填空题答案不能为空')
        return
      }
    } else if (formType === 'program') {
      // 验证编程题
      if (!formData.sampleInput || !formData.sampleOutput) {
        ElMessage.error('编程题需要提供示例输入和输出')
        return
      }
      if (!formData.referenceAnswer) {
        ElMessage.error('编程题需要提供参考答案')
        return
      }
    } else if (formType === 'shortAnswer') {
      // 验证简答题
      if (!formData.referenceAnswer) {
        ElMessage.error('简答题需要提供参考答案')
        return
      }
    }

    // 保存到服务
    try {
      const savedQuestion = await questionService.addQuestion(formData)
      console.log('保存的试题：', savedQuestion)
      ElMessage.success('试题保存成功！')

      // 重置表单
      formRef.resetFields()
    } catch (error: any) {
      console.error('保存试题失败:', error)
      ElMessage.error(`添加题目失败: ${error.message || '未知错误'}`)
    }
  } catch (error: any) {
    console.error('表单处理错误:', error)
    ElMessage.error(`表单验证失败: ${error.message || '请检查必填项'}`)
  }
}
</script>

<template>
  <div class="question-input">
    <el-tabs v-model="activeTab" class="question-tabs">
      <el-tab-pane label="单选题" name="single">
        <el-form
          ref="singleFormRef"
          :model="singleForm"
          :rules="rules.single"
          label-width="100px"
        >
          <el-form-item label="题目内容" prop="question" required>
            <el-input v-model="singleForm.question" type="textarea" :rows="3" placeholder="请输入题目内容"/>
          </el-form-item>
          <el-form-item
            v-for="(_, index) in singleForm.options"
            :key="index"
            :label="`选项${String.fromCharCode(65 + index)}`"
            required
          >
            <el-input v-model="singleForm.options[index]" placeholder="请输入选项内容">
              <template #append>
                <el-button v-if="index === singleForm.options.length - 1" @click="addOption" :disabled="singleForm.options.length >= 6">
                  <el-icon><Plus /></el-icon>
                </el-button>
                <el-button v-if="singleForm.options.length > 2" @click="removeOption(index)" type="danger">
                  <el-icon><Delete /></el-icon>
                </el-button>
              </template>
            </el-input>
          </el-form-item>
          <el-form-item label="正确答案" prop="correctAnswer" required>
            <el-select v-model="singleForm.correctAnswer" placeholder="请选择正确答案">
              <el-option
                v-for="(_, index) in singleForm.options"
                :key="index"
                :label="String.fromCharCode(65 + index)"
                :value="String.fromCharCode(65 + index)"
              />
            </el-select>
          </el-form-item>
          <el-form-item label="答案解析">
            <el-input v-model="singleForm.analysis" type="textarea" :rows="2" placeholder="请输入答案解析"/>
          </el-form-item>
        </el-form>
      </el-tab-pane>

      <el-tab-pane label="判断题" name="judge">
        <el-form
          ref="judgeFormRef"
          :model="judgeForm"
          :rules="rules.judge"
          label-width="100px"
        >
          <el-form-item label="题目内容" prop="question" required>
            <el-input v-model="judgeForm.question" type="textarea" :rows="3" placeholder="请输入题目内容"/>
          </el-form-item>
          <el-form-item label="正确答案" prop="correctAnswer" required>
            <el-radio-group v-model="judgeForm.correctAnswer">
              <el-radio value="正确">正确</el-radio>
              <el-radio value="错误">错误</el-radio>
            </el-radio-group>
          </el-form-item>
          <el-form-item label="答案解析">
            <el-input v-model="judgeForm.analysis" type="textarea" :rows="2" placeholder="请输入答案解析"/>
          </el-form-item>
        </el-form>
      </el-tab-pane>

      <el-tab-pane label="填空题" name="fill">
        <el-form
          ref="fillFormRef"
          :model="fillForm"
          :rules="rules.fill"
          label-width="100px"
        >
          <el-form-item label="题目内容" prop="question" required>
            <el-input v-model="fillForm.question" type="textarea" :rows="3" placeholder="请输入题目内容，使用下划线'_'表示填空位置"/>
          </el-form-item>
          <el-form-item
            v-for="(_, index) in fillForm.answers"
            :key="index"
            :label="`填空${index + 1}`"
            :prop="`answers.${index}`"
            required
          >
            <el-input
              :model-value="fillForm.answers[index]"
              @input="updateFillAnswer(index, $event)"
              placeholder="请输入答案"
            >
              <template #append>
                <el-button v-if="index === fillForm.answers.length - 1" @click="addFillBlank" :disabled="fillForm.answers.length >= 5">
                  <el-icon><Plus /></el-icon>
                </el-button>
                <el-button v-if="fillForm.answers.length > 1" @click="removeFillBlank(index)" type="danger">
                  <el-icon><Delete /></el-icon>
                </el-button>
              </template>
            </el-input>
          </el-form-item>
          <el-form-item label="答案解析">
            <el-input v-model="fillForm.analysis" type="textarea" :rows="2" placeholder="请输入答案解析"/>
          </el-form-item>
        </el-form>
      </el-tab-pane>

      <el-tab-pane label="编程题" name="program">
        <el-form
          ref="programFormRef"
          :model="programForm"
          :rules="rules.program"
          label-width="100px"
        >
          <el-form-item label="题目内容" prop="question" required>
            <el-input v-model="programForm.question" type="textarea" :rows="4" placeholder="请输入题目内容、要求和限制条件"/>
          </el-form-item>
          <el-form-item label="示例输入" prop="sampleInput" required>
            <el-input v-model="programForm.sampleInput" type="textarea" :rows="2" placeholder="请输入示例输入数据"/>
          </el-form-item>
          <el-form-item label="示例输出" prop="sampleOutput" required>
            <el-input v-model="programForm.sampleOutput" type="textarea" :rows="2" placeholder="请输入示例输出结果"/>
          </el-form-item>
          <el-form-item label="参考答案" prop="referenceAnswer" required>
            <el-input v-model="programForm.referenceAnswer" type="textarea" :rows="4" placeholder="请输入参考答案代码"/>
          </el-form-item>
          <el-form-item label="解题思路">
            <el-input v-model="programForm.analysis" type="textarea" :rows="3" placeholder="请输入解题思路和关键点"/>
          </el-form-item>
        </el-form>
      </el-tab-pane>

      <el-tab-pane label="简答题" name="shortAnswer">
        <el-form
          ref="shortAnswerFormRef"
          :model="shortAnswerForm"
          :rules="rules.shortAnswer"
          label-width="100px"
        >
          <el-form-item label="题目内容" prop="question" required>
            <el-input v-model="shortAnswerForm.question" type="textarea" :rows="3" placeholder="请输入题目内容"/>
          </el-form-item>
          <el-form-item label="参考答案" prop="referenceAnswer" required>
            <el-input v-model="shortAnswerForm.referenceAnswer" type="textarea" :rows="4" placeholder="请输入参考答案"/>
          </el-form-item>
          <el-form-item label="答案要点">
            <el-input v-model="shortAnswerForm.analysis" type="textarea" :rows="2" placeholder="请输入答案要点和评分标准"/>
          </el-form-item>
        </el-form>
      </el-tab-pane>
    </el-tabs>

    <div class="form-actions">
      <el-button type="primary" @click="submitQuestion(activeTab)">
        <el-icon><Check /></el-icon>保存试题
      </el-button>
      <el-button @click="router.push('/manage')">
        <el-icon><Back /></el-icon>返回列表
      </el-button>
    </div>
  </div>
</template>

<style scoped>
.question-input {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
  background-color: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
}

.question-tabs {
  margin-bottom: 20px;
}

.form-actions {
  margin-top: 30px;
  text-align: center;
}

.form-actions .el-button {
  margin: 0 10px;
}

.el-button [class^="el-icon"] {
  margin-right: 5px;
}
</style>