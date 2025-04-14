<script setup lang="ts">
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useUserStore } from '../stores/userStore'
import { QuestionType, TypeMap } from '../types/question'
import { examService } from '../services/examService'
import { Timer, Check, Back } from '@element-plus/icons-vue'

interface Exam {
  id: number;
  title: string;
  description?: string;
  questions: Array<{
    questionId: number;
    score: number;
    question: {
      type: QuestionType;
      content: string;
      optionsJson?: string;
      answersJson?: string;
      analysis?: string;
    };
  }>;
  totalScore?: number;
  deadline?: string;
  isSubmitted?: boolean;
}

interface ExamResult {
  score: number;
  totalScore: number;
  correctCount: number;
  questionCount: number;
  completionTime: number;
  submittedAt: string;
  answers: Array<{
    questionId: number;
    answer: string;
    isCorrect: boolean;
    score: number;
  }>;
}

// 获取用户信息和路由
const userStore = useUserStore()
const router = useRouter()
const route = useRoute()

// 题型映射
const typeMap: TypeMap = {
  [QuestionType.Single]: '单选题',
  [QuestionType.Judge]: '判断题',
  [QuestionType.Fill]: '填空题',
  [QuestionType.Program]: '编程题',
  [QuestionType.ShortAnswer]: '简答题'
}

// 试卷列表
const examList = ref<Exam[]>([])
const loading = ref(false)

// 当前试卷
const currentExam = ref<Exam | null>(null)
const examLoading = ref(false)

// 答题状态
const answers = reactive<Record<number, string>>({})
const startTime = ref(Date.now())
const timeSpent = ref(0)
const timerInterval = ref<ReturnType<typeof setInterval> | null>(null)

// 结果
const examResult = ref<ExamResult | null>(null)
const showResult = ref(false)

// 加载试卷列表
const loadExams = async () => {
  loading.value = true
  try {
    const response = await examService.getStudentExams()
    // 确保每个试卷的questions字段都是数组
    examList.value = Array.isArray(response) ? response.map(exam => ({
      ...exam,
      questions: Array.isArray(exam.questions) ? exam.questions : []
    })) : []

    // 如果路由中有试卷ID，直接加载该试卷
    const examId = route.params.id
    if (examId) {
      await loadExam(Number(examId))
    }

    console.log('examList:', examList.value)
  } catch (error) {
    console.error('加载试卷列表失败:', error)
    ElMessage.error('加载试卷列表失败')
  } finally {
    loading.value = false
  }
}

// 加载试卷详情
const loadExam = async (id: number) => {
  examLoading.value = true
  try {
    const response = await examService.getExamById(id)
    currentExam.value = response
    // 初始化答案
    if (currentExam.value?.questions) {
      currentExam.value.questions.forEach((question) => {
        answers[question.questionId] = ''
      })
    }

    // 开始计时
    startTimer()
  } catch (error) {
    console.error('加载试卷失败:', error)
    ElMessage.error('加载试卷失败')
  } finally {
    examLoading.value = false
  }
}

// 开始计时
const startTimer = () => {
  startTime.value = Date.now()
  timerInterval.value = setInterval(() => {
    timeSpent.value = Math.floor((Date.now() - startTime.value) / 1000)
  }, 1000)
}

// 停止计时
const stopTimer = () => {
  if (timerInterval.value) {
    clearInterval(timerInterval.value)
    timerInterval.value = null
  }
}

// 格式化时间
const formattedTime = computed(() => {
  const hours = Math.floor(timeSpent.value / 3600)
  const minutes = Math.floor((timeSpent.value % 3600) / 60)
  const seconds = timeSpent.value % 60

  return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`
})

// 返回试卷列表
const backToList = () => {
  if (Object.values(answers).some(a => a)) {
    ElMessageBox.confirm('您有未提交的答案，确定要返回吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    }).then(() => {
      stopTimer()
      currentExam.value = null
      showResult.value = false
    }).catch(() => {})
  } else {
    stopTimer()
    currentExam.value = null
    showResult.value = false
  }
}

// 提交试卷
const submitExam = async () => {
  try {
    if (!currentExam.value) {
      ElMessage.error('试卷数据不存在')
      return
    }

    await ElMessageBox.confirm('确定要提交试卷吗？提交后将无法修改答案。', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })

    // 检查是否有空答案，提醒用户
    const unansweredCount = currentExam.value.questions.filter(
      q => !answers[q.questionId]
    ).length;
    
    if (unansweredCount > 0) {
      const confirm = await ElMessageBox.confirm(
        `您有 ${unansweredCount} 道题目尚未作答，确定要提交吗？`, 
        '未完成答题', 
        {
          confirmButtonText: '仍然提交',
          cancelButtonText: '继续作答',
          type: 'warning'
        }
      ).catch(() => false);
      
      if (!confirm) return;
    }

    const answerList = Object.entries(answers).map(([questionId, answer]) => ({
      questionId: parseInt(questionId),
      answer: answer || ''
    }))

    const completionTime = Math.ceil(timeSpent.value / 60) // 转换为分钟

    // 显示加载中
    const loading = ElMessage({
      message: '正在提交试卷，请稍候...',
      type: 'info',
      duration: 0
    });
      
    try {
      const response = await examService.submitExam(currentExam.value.id, {
        answers: answerList,
        completionTime
      });
      
      // 关闭加载提示
      loading.close();
      
      stopTimer();
      
      // 处理响应数据
      if (response && response.data) {
        examResult.value = response.data;
      } else if (typeof response === 'object') {
        examResult.value = response as any;
      } else {
        throw new Error('服务器返回的数据格式不正确');
      }
      
      showResult.value = true;
      ElMessage.success('试卷提交成功');
    } catch (submitError: any) {
      // 关闭加载提示
      loading.close();
      
      console.error('提交试卷失败:', submitError);
      
      // 显示详细错误信息
      ElMessageBox.alert(
        `提交失败: ${submitError.message || '未知错误'}
        
        如果问题持续存在，请尝试:
        1. 检查网络连接
        2. 确认服务器是否正常运行
        3. 联系管理员处理
        `, 
        '提交错误', 
        {
          type: 'error',
          confirmButtonText: '知道了'
        }
      );
    }
  } catch (error) {
    if (error !== 'cancel') {
      console.error('提交试卷操作取消:', error);
    }
  }
};

// 获取选项标签
const getOptionLabel = (index: number): string => {
  return String.fromCharCode(65 + index) // A, B, C, D...
}

// 页面加载时获取试卷列表
onMounted(() => {
  loadExams()
})

// 组件销毁时停止计时器
onUnmounted(() => {
  stopTimer()
})

// 获取答案结果
const getAnswerResult = (questionId: number) => {
  if (!examResult.value) return { isCorrect: false, score: 0, answer: '' }

  const answer = examResult.value.answers.find(a => a.questionId === questionId)
  return answer || { isCorrect: false, score: 0, answer: '' }
}
</script>

<template>
  <div class="exam-taking-container">
    <!-- 试卷列表 -->
    <div v-if="!currentExam && !showResult" class="exam-list-container">
      <div class="page-header">
        <h2>我的试卷</h2>
      </div>

      <el-card class="exam-list" v-loading="loading">
        <template v-if="examList.length === 0">
          <div class="empty-data">
            <el-empty description="暂无试卷数据" />
          </div>
        </template>

        <el-table v-else :data="examList" style="width: 100%">
          <el-table-column prop="title" label="试卷标题" min-width="200" />
          <el-table-column prop="description" label="描述" min-width="200" />
          <el-table-column label="题目数量" width="100">
            <template #default="scope">
              {{ scope.row?.questions?.length ?? 0 }}
            </template>
          </el-table-column>
          <el-table-column label="总分" width="80">
            <template #default="scope">
              {{ (scope.row && scope.row.totalScore !== undefined) ? scope.row.totalScore : 0 }}
            </template>
          </el-table-column>
          <el-table-column label="状态" width="100">
            <template #default="scope">
              <el-tag :type="scope.row.isSubmitted ? 'success' : 'warning'">
                {{ scope.row.isSubmitted ? '已提交' : '未提交' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="截止日期" width="180">
            <template #default="scope">
              {{ scope.row.deadline || '无' }}
            </template>
          </el-table-column>
          <el-table-column label="操作" width="120">
            <template #default="scope">
              <el-button
                type="primary"
                size="small"
                :disabled="scope.row.isSubmitted"
                @click="loadExam(scope.row.id)"
              >
                {{ scope.row.isSubmitted ? '查看结果' : '开始答题' }}
              </el-button>
            </template>
          </el-table-column>
        </el-table>
      </el-card>
    </div>

    <!-- 答题界面 -->
    <div v-else-if="currentExam && !showResult" class="exam-content-container" v-loading="examLoading">
      <div class="exam-header">
        <div class="exam-title">
          <h2>{{ currentExam.title }}</h2>
          <div class="exam-info">
            <span>总分: {{ currentExam.totalScore }}分</span>
            <span>题目数: {{ currentExam.questions.length }}</span>
            <span v-if="currentExam.deadline">截止日期: {{ currentExam.deadline }}</span>
          </div>
        </div>

        <div class="exam-timer">
          <el-tag size="large" type="warning">
            <el-icon><Timer /></el-icon>
            用时: {{ formattedTime }}
          </el-tag>
        </div>
      </div>

      <div class="exam-description" v-if="currentExam.description">
        <p>{{ currentExam.description }}</p>
      </div>

      <div class="questions-container">
        <div
          v-for="(question, index) in currentExam.questions"
          :key="question.questionId"
          class="question-item"
        >
          <div class="question-header">
            <div class="question-title">
              <span class="question-number">{{ index + 1 }}.</span>
              <span class="question-type">[{{ typeMap[question.question.type] }}]</span>
              <span class="question-content">{{ question.question.content }}</span>
            </div>
            <div class="question-score">
              {{ question.score }}分
            </div>
          </div>

          <!-- 单选题 -->
          <div v-if="question.question.type === QuestionType.Single" class="question-options">
            <el-radio-group v-model="answers[question.questionId]">
              <el-radio
                v-for="(option, optIndex) in JSON.parse(question.question.optionsJson || '[]')"
                :key="optIndex"
                :value="getOptionLabel(optIndex)"
              >
                {{ getOptionLabel(optIndex) }}. {{ option }}
              </el-radio>
            </el-radio-group>
          </div>

          <!-- 判断题 -->
          <div v-else-if="question.question.type === QuestionType.Judge" class="question-options">
            <el-radio-group v-model="answers[question.questionId]">
              <el-radio :value="'T'">正确</el-radio>
              <el-radio :value="'F'">错误</el-radio>
            </el-radio-group>
          </div>

          <!-- 填空题 -->
          <div v-else-if="question.question.type === QuestionType.Fill" class="question-answer">
            <el-input
              v-model="answers[question.questionId]"
              type="text"
              placeholder="请输入答案"
            />
          </div>

          <!-- 简答题 -->
          <div v-else-if="question.question.type === QuestionType.ShortAnswer" class="question-answer">
            <el-input
              v-model="answers[question.questionId]"
              type="textarea"
              :rows="4"
              placeholder="请输入答案"
            />
          </div>

          <!-- 编程题 -->
          <div v-else-if="question.question.type === QuestionType.Program" class="question-answer">
            <el-input
              v-model="answers[question.questionId]"
              type="textarea"
              :rows="8"
              placeholder="请输入代码"
              font-family="monospace"
            />
          </div>
        </div>
      </div>

      <div class="exam-actions">
        <el-button :icon="Back" @click="backToList">返回</el-button>
        <el-button type="primary" :icon="Check" @click="submitExam">提交试卷</el-button>
      </div>
    </div>

    <!-- 结果界面 -->
    <div v-else-if="showResult && examResult" class="exam-result-container">
      <div class="result-header">
        <h2>{{ currentExam.title }} - 答题结果</h2>
        <el-button :icon="Back" @click="backToList">返回试卷列表</el-button>
      </div>

      <el-card class="result-summary">
        <el-descriptions title="成绩信息" :column="3" border>
          <el-descriptions-item label="得分">
            <span class="score">{{ examResult.score }}</span> / {{ examResult.totalScore }}
          </el-descriptions-item>
          <el-descriptions-item label="正确题数">
            {{ examResult.correctCount }} / {{ examResult.questionCount }}
          </el-descriptions-item>
          <el-descriptions-item label="用时">
            {{ examResult.completionTime }} 分钟
          </el-descriptions-item>
          <el-descriptions-item label="提交时间">
            {{ examResult.submittedAt }}
          </el-descriptions-item>
        </el-descriptions>
      </el-card>

      <div class="result-questions">
        <div
          v-for="(question, index) in currentExam.questions"
          :key="question.questionId"
          class="question-item"
        >
          <div class="question-header">
            <div class="question-title">
              <span class="question-number">{{ index + 1 }}.</span>
              <span class="question-type">[{{ typeMap[question.question.type] }}]</span>
              <span class="question-content">{{ question.question.content }}</span>
            </div>
            <div class="question-result">
              <el-tag
                :type="getAnswerResult(question.questionId as number).isCorrect ? 'success' : 'danger'"
              >
                {{ getAnswerResult(question.questionId as number).isCorrect ? '正确' : '错误' }}
              </el-tag>
              <span class="question-score">
                {{ getAnswerResult(question.questionId as number).score }} / {{ question.score }}分
              </span>
            </div>
          </div>

          <div class="answer-content">
            <div class="your-answer">
              <strong>你的答案:</strong> {{ getAnswerResult(question.questionId as number).answer || '未作答' }}
            </div>
            <div class="correct-answer">
              <strong>正确答案:</strong> {{ JSON.parse(question.question.answersJson || '[]').join(', ') }}
            </div>
            <div class="answer-analysis" v-if="question.question.analysis">
              <strong>解析:</strong> {{ question.question.analysis }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.exam-taking-container {
  padding: 20px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.exam-list {
  margin-bottom: 20px;
}

.empty-data {
  padding: 40px 0;
}

.exam-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 15px;
  border-bottom: 1px solid #EBEEF5;
}

.exam-info {
  display: flex;
  gap: 20px;
  margin-top: 10px;
  color: #606266;
}

.exam-description {
  margin-bottom: 20px;
  padding: 15px;
  background-color: #F5F7FA;
  border-radius: 4px;
}

.questions-container {
  margin-bottom: 30px;
}

.question-item {
  margin-bottom: 30px;
  padding: 20px;
  border: 1px solid #EBEEF5;
  border-radius: 4px;
  background-color: #fff;
}

.question-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 15px;
}

.question-number {
  font-weight: bold;
  margin-right: 5px;
}

.question-type {
  color: #409EFF;
  margin-right: 10px;
}

.question-score {
  color: #F56C6C;
  font-weight: bold;
}

.question-options {
  padding-left: 20px;
}

.question-answer {
  margin-top: 15px;
}

.exam-actions {
  display: flex;
  justify-content: space-between;
  margin-top: 30px;
}

.result-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.result-summary {
  margin-bottom: 30px;
}

.score {
  font-size: 18px;
  font-weight: bold;
  color: #F56C6C;
}

.question-result {
  display: flex;
  align-items: center;
  gap: 10px;
}

.answer-content {
  margin-top: 15px;
  padding: 15px;
  background-color: #F5F7FA;
  border-radius: 4px;
}

.your-answer, .correct-answer, .answer-analysis {
  margin-bottom: 10px;
}

.answer-analysis {
  color: #67C23A;
}
</style>


