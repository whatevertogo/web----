<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { QuestionType } from '../types/question'
import { questionService } from '../services/questionService'
import { SingleChoice, JudgeQuestion, FillBlank, ProgramQuestion, ShortAnswerQuestion } from '../components/questions'

// 考试状态
const examStatus = ref<'ready' | 'ongoing' | 'finished'>('ready')
const examQuestions = ref<any[]>([]) // 添加考试题目数组
const currentQuestion = ref<any>(null)
const userAnswers = ref<Record<number, any>>({})
const examTime = ref(60) // 分钟
const remainingTime = ref(examTime.value * 60)
const timer = ref<ReturnType<typeof setInterval>>() // 修改 timer 类型

// 查看详情对话框
const detailDialogVisible = ref(false)
const detailQuestion = ref<any>(null)

// 结果统计
const examResults = ref({
  totalQuestions: 0,
  correctAnswers: 0,
  incorrectAnswers: 0,
  skippedQuestions: 0,
  score: 0,
  accuracy: 0,
  timeUsed: 0
})

// 显示答案解析
const showExplanation = ref(false)

// 当前题目索引
const currentQuestionIndex = computed(() => {
  if (!currentQuestion.value || !examQuestions.value.length) return 0
  return examQuestions.value.findIndex(q => q.id === currentQuestion.value.id)
})

// 添加 fillAnswers 的定义
const fillAnswers = ref<string[]>([])

// 题型映射
const typeMap = {
  single: '单选题',
  judge: '判断题',
  fill: '填空题',
  program: '编程题',
  shortAnswer: '简答题'
}

// 考试进度
const progress = computed(() => {
  const total = examQuestions.value.length
  const answered = Object.keys(userAnswers.value).length
  return total ? Math.round((answered / total) * 100) : 0
})

// 格式化剩余时间
const formattedTime = computed(() => {
  const hours = Math.floor(remainingTime.value / 3600)
  const minutes = Math.floor((remainingTime.value % 3600) / 60)
  const seconds = remainingTime.value % 60
  return {
    hours: hours.toString().padStart(2, '0'),
    minutes: minutes.toString().padStart(2, '0'),
    seconds: seconds.toString().padStart(2, '0')
  }
})

// 题型分类统计
const questionStats = computed(() => {
  const stats: Record<QuestionType, { total: number, answered: number, score: number }> = {
    [QuestionType.Single]: { total: 0, answered: 0, score: 0 },
    [QuestionType.Judge]: { total: 0, answered: 0, score: 0 },
    [QuestionType.Fill]: { total: 0, answered: 0, score: 0 },
    [QuestionType.Program]: { total: 0, answered: 0, score: 0 },
    [QuestionType.ShortAnswer]: { total: 0, answered: 0, score: 0 }
  }
  
  examQuestions.value.forEach(q => {
    stats[q.type].total++
    stats[q.type].score += 10
    if (userAnswers.value[q.id]) {
      stats[q.type].answered++
    }
  })
  return stats
})

// 监听当前题目变化，更新填空答案
watch(currentQuestion, (newQuestion) => {
  if (newQuestion?.type === 'fill') {
    // 初始化或更新填空答案
    fillAnswers.value = userAnswers.value[newQuestion.id] || 
      Array(newQuestion.answers?.length || 0).fill('')
    console.log('填空题答案初始化:', fillAnswers.value)
  }
})

// 处理填空答案变化
const handleFillAnswer = (index: number) => {
  if (!currentQuestion.value) return
  
  const questionId = currentQuestion.value.id
  if (!userAnswers.value[questionId]) {
    userAnswers.value[questionId] = Array(currentQuestion.value.answers.length).fill('')
  }
  userAnswers.value[questionId][index] = fillAnswers.value[index]
}

// 查看试题详情
const showQuestionDetail = (question) => {
  detailQuestion.value = question
  detailDialogVisible.value = true
}

// 开始考试
const startExam = async () => {
  try {
    console.log("开始加载题目...")
    await loadQuestions()
    console.log("题目加载结果:", examQuestions.value)
    
    // 只有当成功加载题目后才设置状态
    if (examQuestions.value.length > 0) {
      examStatus.value = 'ongoing'
      startTimer()
    } else {
      ElMessage.warning('没有找到任何题目')
    }
  } catch (error) {
    console.error("加载题目失败:", error)
    ElMessage.error('题目加载失败')
  }
}

// 计时器
const startTimer = () => {
  const startTime = Date.now()
  const duration = examTime.value * 60 * 1000 // 转换为毫秒
  
  timer.value = setInterval(() => {
    const elapsed = Date.now() - startTime
    const remaining = Math.max(0, duration - elapsed)
    remainingTime.value = Math.floor(remaining / 1000)
    
    if (remaining <= 0) {
      finishExam()
    }
  }, 1000)
}

// 加载题目
const loadQuestions = async () => {
  try {
    // 重置现有数据
    examQuestions.value = []
    currentQuestion.value = null
    
    // 加载新数据
    const questions = await questionService.getQuestions()
    console.log('获取到题目:', questions)
    
    // 检查数据格式
    if (!Array.isArray(questions) || questions.length === 0) {
      console.warn('题目数据格式不正确或为空')
      return
    }
    
    // 确保每个题目都有完整的数据
    examQuestions.value = questions.map(q => ({
      ...q,
      options: q.options || [],
      correctAnswer: q.correctAnswer || '',
      analysis: q.analysis || '',
      answers: q.answers || [],
      sampleInput: q.sampleInput || '',
      sampleOutput: q.sampleOutput || '',
      referenceAnswer: q.referenceAnswer || ''
    }))
    
    currentQuestion.value = examQuestions.value[0]
  } catch (error) {
    console.error('题目加载出错:', error)
    throw error
  }
}

// 处理答案提交
const handleAnswerSubmit = (value: any) => {
  if (!currentQuestion.value) return
  
  userAnswers.value[currentQuestion.value.id] = value
  
  // 自动切换到下一题
  const currentIndex = examQuestions.value.findIndex(q => q.id === currentQuestion.value.id)
  if (currentIndex < examQuestions.value.length - 1) {
    currentQuestion.value = examQuestions.value[currentIndex + 1]
  }
}

// 切换题目
const switchQuestion = (index: number) => {
  if (index >= 0 && index < examQuestions.value.length) {
    currentQuestion.value = examQuestions.value[index]
  }
}

// 计算得分
const calculateScore = () => {
  let totalScore = 0
  let correctCount = 0
  
  examQuestions.value.forEach(question => {
    const userAnswer = userAnswers.value[question.id]
    
    // 如果用户没有回答，则不计分
    if (userAnswer === undefined || userAnswer === null || 
        (Array.isArray(userAnswer) && userAnswer.every(a => !a))) return
    
    let isCorrect = false
    
    // 根据题型比较答案
    switch (question.type) {
      case QuestionType.Single:
        // 单选题：直接比较选项
        isCorrect = userAnswer === question.correctAnswer
        break;
      case QuestionType.Judge:
        // 判断题：比较布尔值或字符串
        isCorrect = userAnswer === question.correctAnswer || 
                   userAnswer === String(question.correctAnswer)
        break;
      case QuestionType.Fill:
        // 填空题：需要比较每个空的答案
        if (Array.isArray(userAnswer) && Array.isArray(question.answers)) {
          // 必须所有空都正确才得分
          isCorrect = userAnswer.every((ans, index) => {
            const userAns = (ans || '').toString().trim().toLowerCase();
            const correctAns = (question.answers[index] || '').toString().trim().toLowerCase();
            return userAns === correctAns;
          });
        }
        break;
      case QuestionType.Program:
      case QuestionType.ShortAnswer:
        // 编程题和简答题：简单实现 - 包含关键词即可得分
        // 实际应用中可能需要更复杂的评分逻辑
        if (question.keywords && Array.isArray(question.keywords)) {
          const userAns = (userAnswer || '').toString().toLowerCase();
          isCorrect = question.keywords.some(keyword => 
            userAns.includes(keyword.toLowerCase())
          );
        }
        break;
    }
    
    // 更新得分统计
    if (isCorrect) {
      totalScore += 10 // 假设每题10分
      correctCount++
      
      // 记录正确答案，用于结果展示
      questionResults.value[question.id] = {
        isCorrect: true,
        score: 10
      }
    } else {
      questionResults.value[question.id] = {
        isCorrect: false,
        score: 0
      }
    }
  });
  
  // 更新总分和正确率
  examScore.value = totalScore
  examCorrectRate.value = examQuestions.value.length ? 
    (correctCount / examQuestions.value.length * 100).toFixed(1) + '%' : '0%'
}

// 结束考试
const finishExam = () => {
  if (timer.value) {
    clearInterval(timer.value)
  }
  
  examStatus.value = 'finished'
  
  // 计算结果
  calculateResults()
}

// 计算考试结果
const calculateResults = () => {
  const total = examQuestions.value.length
  let correct = 0
  let incorrect = 0
  let skipped = 0
  
  examQuestions.value.forEach(question => {
    const userAnswer = userAnswers.value[question.id]
    
    if (!userAnswer) {
      skipped++
      return
    }
    
    let isCorrect = false
    
    switch (question.type) {
      case QuestionType.Single:
        isCorrect = userAnswer === question.correctAnswer
        break
      case QuestionType.Judge:
        isCorrect = userAnswer === question.correctAnswer
        break
      case QuestionType.Fill:
        isCorrect = Array.isArray(userAnswer) && 
          userAnswer.every((ans, idx) => 
            ans.trim().toLowerCase() === question.answers[idx].trim().toLowerCase())
        break
      case QuestionType.Program:
      case QuestionType.ShortAnswer:
        // 这些题型需要人工评分，暂时标记为不正确
        isCorrect = false
        break
    }
    
    if (isCorrect) {
      correct++
    } else {
      incorrect++
    }
  })
  
  const timeUsed = examTime.value * 60 - remainingTime.value
  
  examResults.value = {
    totalQuestions: total,
    correctAnswers: correct,
    incorrectAnswers: incorrect,
    skippedQuestions: skipped,
    score: Math.round((correct / total) * 100),
    accuracy: total > 0 ? Math.round((correct / total) * 100) : 0,
    timeUsed
  }
}

// 确保添加相关的响应式变量
const questionResults = ref({})
const examScore = ref(0)
const examCorrectRate = ref('0%')

// 添加时间提醒功能
const timeWarning = computed(() => {
  return remainingTime.value <= 600; // 剩余10分钟以内
});

// 立即执行初始化，确保数据可用
onMounted(() => {
  console.log('组件已挂载，正在初始化...')
  loadQuestions().catch(err => {
    console.error('初始加载失败:', err)
  })
})

onUnmounted(() => {
  if (timer.value) {
    clearInterval(timer.value)
  }
})
</script>

<template>
  <div class="exam-page">
    <div class="exam-header">
      <el-row :gutter="20">
        <el-col :span="16">
          <h2>试题练习</h2>
        </el-col>
        <el-col :span="8" class="text-right">
          <el-tag v-if="examStatus === 'ready'" type="info">准备开始</el-tag>
          <el-tag v-else-if="examStatus === 'ongoing'" type="success">考试中</el-tag>
          <el-tag v-else-if="examStatus === 'finished'" type="warning">已完成</el-tag>
          
          <el-tag v-if="examStatus !== 'ready'" type="info" class="ml-2">
            第 {{ currentQuestionIndex + 1 }}/{{ examQuestions.length }} 题
          </el-tag>
          
          <el-tag v-if="examStatus === 'ongoing'" type="danger" class="ml-2">
            剩余时间: {{ formattedTime.hours }}:{{ formattedTime.minutes }}:{{ formattedTime.seconds }}
          </el-tag>
        </el-col>
      </el-row>
    </div>

    <!-- 考试准备界面 -->
    <div v-if="examStatus === 'ready'" class="exam-ready">
      <h3>准备开始练习</h3>
      <p>练习时间: {{ examTime }}分钟</p>
      <el-button type="primary" @click="startExam">开始练习</el-button>
    </div>

    <!-- 练习中状态 - 当有题目时显示 -->
    <div v-if="examStatus === 'ongoing' && currentQuestion" class="active-question">
      <div class="question-text">
        <h3>{{ typeMap[currentQuestion.type] }}</h3>
        <p>{{ currentQuestion.question }}</p>
        <el-button type="text" size="small" @click="showQuestionDetail(currentQuestion)">查看详情</el-button>
      </div>
    
      <!-- 各种题型组件 -->
      <div class="answer-area">
        <!-- 单选题 -->
        <SingleChoice v-if="currentQuestion.type === QuestionType.Single"
          :question="currentQuestion" :value="userAnswers[currentQuestion.id] || ''"
          :onChange="handleAnswerSubmit" />
        
        <!-- 判断题 -->
        <JudgeQuestion v-if="currentQuestion.type === QuestionType.Judge"
          :question="currentQuestion" :value="userAnswers[currentQuestion.id] || ''"
          :onChange="handleAnswerSubmit" />
        
        <!-- 填空题 -->
        <FillBlank v-if="currentQuestion.type === QuestionType.Fill"
          :question="currentQuestion" 
          :answers="fillAnswers" 
          :onAnswerChange="handleFillAnswer" />
        
        <!-- 编程题 -->
        <ProgramQuestion v-if="currentQuestion.type === QuestionType.Program"
          :question="currentQuestion" 
          :value="userAnswers[currentQuestion.id] || ''" 
          :onChange="handleAnswerSubmit" />
        
        <!-- 简答题 -->
        <ShortAnswerQuestion v-if="currentQuestion.type === QuestionType.ShortAnswer"
          :question="currentQuestion" 
          :value="userAnswers[currentQuestion.id] || ''" 
          :onChange="handleAnswerSubmit" />
      </div>

      <!-- 导航按钮 -->
      <div class="nav-buttons">
        <el-button v-if="currentQuestionIndex > 0" 
          @click="switchQuestion(currentQuestionIndex - 1)">上一题</el-button>
        <el-button v-if="currentQuestionIndex < examQuestions.length - 1" 
          @click="switchQuestion(currentQuestionIndex + 1)">下一题</el-button>
        <el-button type="primary" @click="finishExam">提交答案</el-button>
        <el-button @click="startExam">重新开始</el-button>
      </div>
    </div>
    
    <!-- 未加载到题目时显示 -->
    <div v-if="examStatus === 'ongoing' && !currentQuestion" class="no-questions">
      <h3>加载题目中...</h3>
    </div>
    
    <!-- 题目详情对话框 -->
    <el-dialog
      v-model="detailDialogVisible"
      title="试题详情"
      width="50%"
    >
      <el-descriptions v-if="detailQuestion" :column="1" border>
        <el-descriptions-item label="题型">
          {{ typeMap[detailQuestion.type] }}
        </el-descriptions-item>
        <el-descriptions-item label="题目内容">
          {{ detailQuestion.question }}
        </el-descriptions-item>
        
        <!-- 单选题特有字段 -->
        <template v-if="detailQuestion.type === QuestionType.Single">
          <el-descriptions-item 
            v-for="(option, index) in detailQuestion.options"
            :key="index"
            :label="`选项${String.fromCharCode(65 + index)}`"
          >
            {{ option }}
          </el-descriptions-item>
          <el-descriptions-item label="正确答案">
            选项{{ detailQuestion.correctAnswer }}
          </el-descriptions-item>
        </template>

        <!-- 判断题特有字段 -->
        <template v-else-if="detailQuestion.type === QuestionType.Judge">
          <el-descriptions-item label="正确答案">
            {{ detailQuestion.correctAnswer === 'true' ? '正确' : '错误' }}
          </el-descriptions-item>
        </template>

        <!-- 填空题特有字段 -->
        <template v-else-if="detailQuestion.type === QuestionType.Fill">
          <el-descriptions-item 
            v-for="(answer, index) in detailQuestion.answers"
            :key="index"
            :label="`填空${index + 1}答案`"
          >
            {{ answer }}
          </el-descriptions-item>
        </template>

        <!-- 编程题特有字段 -->
        <template v-else-if="detailQuestion.type === QuestionType.Program">
          <el-descriptions-item label="示例输入">
            <pre>{{ detailQuestion.sampleInput }}</pre>
          </el-descriptions-item>
          <el-descriptions-item label="示例输出">
            <pre>{{ detailQuestion.sampleOutput }}</pre>
          </el-descriptions-item>
        </template>

        <!-- 简答题特有字段 -->
        <template v-else-if="detailQuestion.type === QuestionType.ShortAnswer">
          <el-descriptions-item label="参考答案">
            {{ detailQuestion.referenceAnswer }}
          </el-descriptions-item>
        </template>

        <!-- 所有题型都有的解析字段 -->
        <el-descriptions-item label="答案解析">
          {{ detailQuestion.analysis }}
        </el-descriptions-item>
      </el-descriptions>

      <template #footer>
        <el-button @click="detailDialogVisible = false">关闭</el-button>
      </template>
    </el-dialog>

    <!-- 考试完成状态 -->
    <div v-if="examStatus === 'finished'" class="exam-finished">
      <h3>练习完成</h3>
      
      <!-- 结果统计 -->
      <el-card class="result-card">
        <template #header>
          <div class="result-header">
            <h4>练习结果</h4>
          </div>
        </template>
        
        <el-row :gutter="20">
          <el-col :span="8">
            <div class="result-item">
              <div class="result-value">{{ examResults.totalQuestions }}</div>
              <div class="result-label">总题数</div>
            </div>
          </el-col>
          <el-col :span="8">
            <div class="result-item">
              <div class="result-value">{{ examResults.correctAnswers }}</div>
              <div class="result-label">正确</div>
            </div>
          </el-col>
          <el-col :span="8">
            <div class="result-item">
              <div class="result-value">{{ examResults.incorrectAnswers }}</div>
              <div class="result-label">错误</div>
            </div>
          </el-col>
        </el-row>
        
        <el-row :gutter="20" class="mt-20">
          <el-col :span="8">
            <div class="result-item">
              <div class="result-value">{{ examResults.skippedQuestions }}</div>
              <div class="result-label">未答</div>
            </div>
          </el-col>
          <el-col :span="8">
            <div class="result-item">
              <div class="result-value">{{ examResults.score }}%</div>
              <div class="result-label">得分率</div>
            </div>
          </el-col>
          <el-col :span="8">
            <div class="result-item">
              <div class="result-value">{{ Math.floor(examResults.timeUsed / 60) }}:{{ (examResults.timeUsed % 60).toString().padStart(2, '0') }}</div>
              <div class="result-label">用时</div>
            </div>
          </el-col>
        </el-row>
      </el-card>
      
      <!-- 题目回顾 -->
      <div class="review-section">
        <div class="review-header">
          <h4>题目回顾</h4>
          <el-switch
            v-model="showExplanation"
            active-text="显示解析"
            inactive-text="隐藏解析"
          />
        </div>
        
        <el-collapse>
          <el-collapse-item 
            v-for="(question, index) in examQuestions" 
            :key="question.id"
            :title="`第 ${index + 1} 题: ${question.question.substring(0, 30)}...`"
          >
            <div class="question-review">
              <div class="question-content">
                <h5>{{ typeMap[question.type] }}</h5>
                <p>{{ question.question }}</p>
                
                <!-- 单选题 -->
                <template v-if="question.type === QuestionType.Single">
                  <div 
                    v-for="(option, idx) in question.options" 
                    :key="idx"
                    :class="[
                      'option-item',
                      userAnswers[question.id] === String.fromCharCode(65 + idx) ? 'user-selected' : '',
                      question.correctAnswer === String.fromCharCode(65 + idx) ? 'correct-answer' : ''
                    ]"
                  >
                    {{ String.fromCharCode(65 + idx) }}. {{ option }}
                  </div>
                </template>
                
                <!-- 判断题 -->
                <template v-else-if="question.type === QuestionType.Judge">
                  <div 
                    :class="[
                      'option-item',
                      userAnswers[question.id] === 'true' ? 'user-selected' : '',
                      question.correctAnswer === 'true' ? 'correct-answer' : ''
                    ]"
                  >
                    正确
                  </div>
                  <div 
                    :class="[
                      'option-item',
                      userAnswers[question.id] === 'false' ? 'user-selected' : '',
                      question.correctAnswer === 'false' ? 'correct-answer' : ''
                    ]"
                  >
                    错误
                  </div>
                </template>
                
                <!-- 填空题 -->
                <template v-else-if="question.type === QuestionType.Fill">
                  <div v-for="(answer, idx) in question.answers" :key="idx" class="fill-review">
                    <div class="fill-label">空 {{ idx + 1 }}:</div>
                    <div class="fill-content">
                      <div class="user-answer">你的答案: {{ userAnswers[question.id]?.[idx] || '未作答' }}</div>
                      <div class="correct-answer">正确答案: {{ answer }}</div>
                    </div>
                  </div>
                </template>
                
                <!-- 其他题型 -->
                <template v-else>
                  <div class="other-answer">
                    <div>你的答案:</div>
                    <pre>{{ userAnswers[question.id] || '未作答' }}</pre>
                    <div>参考答案:</div>
                    <pre v-if="question.type === QuestionType.Program">{{ question.sampleOutput }}</pre>
                    <pre v-else-if="question.type === QuestionType.ShortAnswer">{{ question.referenceAnswer }}</pre>
                  </div>
                </template>
                
                <!-- 解析 -->
                <div v-if="showExplanation" class="explanation">
                  <h5>解析:</h5>
                  <p>{{ question.analysis }}</p>
                </div>
              </div>
            </div>
          </el-collapse-item>
        </el-collapse>
      </div>
      
      <div class="action-buttons">
        <el-button type="primary" @click="startExam">重新开始</el-button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.exam-page {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
  min-height: 500px;
}

.exam-header {
  margin-bottom: 20px;
  padding-bottom: 10px;
  border-bottom: 1px solid #eee;
}

.exam-ready {
  text-align: center;
  padding: 50px 0;
}

.active-question {
  padding: 20px;
}

.question-text {
  margin-bottom: 20px;
  padding: 15px;
  background: #f5f7fa;
  border-radius: 4px;
}

.answer-area {
  margin-bottom: 20px;
}

.nav-buttons {
  margin-top: 20px;
  text-align: center;
  display: flex;
  justify-content: center;
  gap: 10px;
}

.text-right {
  text-align: right;
}

.ml-2 {
  margin-left: 8px;
}

/* 结果页面样式 */
.exam-finished {
  padding: 20px;
}

.result-card {
  margin-bottom: 20px;
}

.result-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.result-item {
  text-align: center;
  padding: 15px;
}

.result-value {
  font-size: 24px;
  font-weight: bold;
  color: var(--el-color-primary);
}

.result-label {
  margin-top: 5px;
  color: #606266;
}

.mt-20 {
  margin-top: 20px;
}

.review-section {
  margin-top: 30px;
}

.review-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.question-review {
  padding: 10px;
}

.option-item {
  padding: 8px 12px;
  margin: 5px 0;
  border-radius: 4px;
  border: 1px solid #dcdfe6;
}

.user-selected {
  background-color: #ecf5ff;
  border-color: #409eff;
}

.correct-answer {
  background-color: #f0f9eb;
  border-color: #67c23a;
}

.fill-review {
  display: flex;
  margin: 10px 0;
}

.fill-label {
  width: 60px;
  font-weight: bold;
}

.fill-content {
  flex: 1;
}

.user-answer {
  margin-bottom: 5px;
}

.correct-answer {
  color: #67c23a;
}

.other-answer pre {
  background-color: #f5f7fa;
  padding: 10px;
  border-radius: 4px;
  margin: 5px 0;
}

.explanation {
  margin-top: 15px;
  padding: 10px;
  background-color: #fdf6ec;
  border-radius: 4px;
}

.action-buttons {
  margin-top: 20px;
  text-align: center;
}
</style> 