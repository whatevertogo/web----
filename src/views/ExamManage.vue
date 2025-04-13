<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useUserStore } from '../stores/userStore'
import { QuestionType, TypeMap, TableQuestion } from '../types/question'
import { questionService } from '../services/questionService'
import { examService } from '../services/examService'
import { Plus, Delete, Edit, Upload, View, Download } from '@element-plus/icons-vue'
import QuestionSelector from '../components/QuestionSelector.vue'

// 获取用户信息
const userStore = useUserStore()
const router = useRouter()

// 检查权限
if (!userStore.isAdmin()) {
  ElMessage.error('您没有权限访问此页面')
  router.push('/practice')
}

// 题型映射
const typeMap: TypeMap = {
  [QuestionType.Single]: '单选题',
  [QuestionType.Judge]: '判断题',
  [QuestionType.Fill]: '填空题',
  [QuestionType.Program]: '编程题',
  [QuestionType.ShortAnswer]: '简答题'
}

// 试卷列表
const examList = ref([])
const loading = ref(false)
const currentPage = ref(1)
const pageSize = ref(10)

// 试卷表单
const examForm = reactive({
  id: 0,
  title: '',
  description: '',
  deadline: '',
  questions: []
})

// 选择的题目
const selectedQuestions = ref<TableQuestion[]>([])

// 学生列表
const studentList = ref([])
const selectedStudents = ref([])
const studentLoading = ref(false)

// 对话框控制
const examDialogVisible = ref(false)
const questionDialogVisible = ref(false)
const studentDialogVisible = ref(false)
const statisticsDialogVisible = ref(false)

// 当前查看的试卷统计信息
const currentExamStatistics = ref(null)

// 加载试卷列表
const loadExams = async () => {
  loading.value = true
  try {
    const response = await examService.getExams()
    console.log('原始试卷数据:', response)

    // 处理试卷数据
    if (response && response.data && Array.isArray(response.data)) {
      // 处理每个试卷的题目数据
      examList.value = response.data.map(exam => {
        // 确保 questions 存在且是数组
        const questions = Array.isArray(exam.questions) ? exam.questions : []

        // 返回处理后的试卷数据
        return {
          ...exam,
          questions: questions.map(q => ({
            ...q,
            // 如果 question 为 null，使用一个默认值
            question: q.question || `题目 ID: ${q.questionId}`
          }))
        }
      })

      console.log('处理后的试卷数据:', examList.value)
    } else {
      console.warn('试卷数据格式不符合预期:', response)
      examList.value = []
    }
  } catch (error) {
    console.error('加载试卷失败:', error)
    ElMessage.error('加载试卷失败')
    examList.value = []
  } finally {
    loading.value = false
  }
}

// 加载学生列表
const loadStudents = async () => {
  studentLoading.value = true
  studentList.value = [] // 重置学生列表

  try {
    console.log('开始加载学生列表...')
    const response = await examService.getStudents()
    console.log('学生数据响应:', response)

    // 处理学生数据
    if (response && response.data) {
      if (Array.isArray(response.data)) {
        studentList.value = response.data
        console.log('学生列表加载成功，共 ' + studentList.value.length + ' 名学生')
      } else {
        console.warn('学生数据不是数组:', response.data)
        ElMessage.warning('学生数据格式不正确')
      }
    } else {
      console.warn('学生数据为空')
      ElMessage.info('没有找到学生数据')
    }
  } catch (error) {
    console.error('加载学生列表失败:', error)
    ElMessage.error(`加载学生列表失败: ${error.message || '未知错误'}`)
  } finally {
    studentLoading.value = false
  }
}

// 创建新试卷
const createExam = () => {
  examForm.id = 0
  examForm.title = ''
  examForm.description = ''
  examForm.deadline = ''
  examForm.questions = []
  selectedQuestions.value = []
  examDialogVisible.value = true
}

// 编辑试卷
const editExam = async (exam) => {
  console.log('编辑试卷:', exam)
  examForm.id = exam.id
  examForm.title = exam.title
  examForm.description = exam.description
  examForm.deadline = exam.deadline
  examForm.questions = [...(Array.isArray(exam.questions) ? exam.questions : [])]

  // 如果题目数据不完整，需要从后端获取完整的题目数据
  try {
    // 先将现有的题目数据保存下来
    const questionIds = exam.questions.map(q => q.questionId)
    console.log('需要获取的题目 ID:', questionIds)

    // 获取所有题目
    const allQuestions = await questionService.getQuestions()
    console.log('获取到的所有题目:', allQuestions)

    // 筛选出试卷中的题目
    const examQuestions = allQuestions.filter(q => questionIds.includes(q.id))
    console.log('试卷中的题目:', examQuestions)

    // 更新选中的题目
    selectedQuestions.value = examQuestions.map(q => ({
      id: q.id,
      type: q.type,
      question: q.question || q.content,
      options: q.options || [],
      answers: q.answers || [],
      analysis: q.analysis || ''
    }))
  } catch (error) {
    console.error('获取题目数据失败:', error)
    ElMessage.warning('获取题目数据失败，请重新选择题目')
    selectedQuestions.value = []
  }

  examDialogVisible.value = true
}

// 删除试卷
const deleteExam = async (exam) => {
  try {
    await ElMessageBox.confirm(`确定要删除试卷 "${exam.title}" 吗？`, '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })

    await examService.deleteExam(exam.id)
    ElMessage.success('删除成功')
    loadExams()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('删除试卷失败:', error)
      ElMessage.error('删除试卷失败')
    }
  }
}

// 保存试卷
const saveExam = async () => {
  if (!examForm.title) {
    ElMessage.warning('请输入试卷标题')
    return
  }

  if (selectedQuestions.value.length === 0) {
    ElMessage.warning('请选择至少一道题目')
    return
  }

  try {
    // 检查题目是否都有有效的ID
    const invalidQuestions = selectedQuestions.value.filter(q => !q.id)
    if (invalidQuestions.length > 0) {
      console.error('存在无效的题目:', invalidQuestions)
      ElMessage.warning('部分题目数据不完整，请重新选择')
      return
    }

    const examData = {
      id: examForm.id,
      title: examForm.title,
      description: examForm.description,
      deadline: examForm.deadline,
      questions: selectedQuestions.value.map((q, index) => ({
        questionId: q.id,
        order: index + 1,
        score: 10 // 默认每题10分
      }))
    }

    console.log('准备保存的试卷数据:', examData)

    let response
    if (examForm.id) {
      response = await examService.updateExam(examData)
      console.log('更新试卷响应:', response)
      ElMessage.success('更新试卷成功')
    } else {
      response = await examService.createExam(examData)
      console.log('创建试卷响应:', response)
      ElMessage.success('创建试卷成功')
    }

    examDialogVisible.value = false

    // 添加更长的延时，确保后端处理完成
    setTimeout(() => {
      loadExams()
    }, 1000)
  } catch (error) {
    console.error('保存试卷失败:', error)
    ElMessage.error(`保存试卷失败: ${error.message || '未知错误'}`)
  }
}

// 打开选择题目对话框
const openQuestionDialog = async () => {
  try {
    await questionService.getQuestions()
    questionDialogVisible.value = true
  } catch (error) {
    console.error('加载题目失败:', error)
    ElMessage.error('加载题目失败')
  }
}

// 选择题目
const selectQuestions = (questions) => {
  // 确保题目有正确的属性
  selectedQuestions.value = questions.map(q => ({
    id: q.id,
    type: q.type,
    question: q.question || q.content,
    options: q.options || [],
    answers: q.answers || [],
    analysis: q.analysis || ''
  }))
  console.log('选中的题目:', selectedQuestions.value)
  questionDialogVisible.value = false
}

// 移除选中的题目
const removeQuestion = (index) => {
  selectedQuestions.value.splice(index, 1)
}

// 发布试卷
const publishExam = async (exam) => {
  if (!exam || !exam.id) {
    ElMessage.warning('试卷数据无效')
    return
  }

  console.log('准备发布试卷:', exam)
  selectedStudents.value = []
  examForm.id = exam.id

  try {
    // 先加载学生数据，然后再打开对话框
    await loadStudents()

    // 确保有学生数据
    if (studentList.value.length === 0) {
      ElMessage.warning('没有找到学生数据，请先添加学生')
      return
    }

    studentDialogVisible.value = true
  } catch (error) {
    console.error('加载学生数据失败:', error)
    ElMessage.error(`加载学生数据失败: ${error.message || '未知错误'}`)
  }
}

// 分配试卷给学生
const assignExamToStudents = async () => {
  if (selectedStudents.value.length === 0) {
    ElMessage.warning('请至少选择一名学生')
    return
  }

  try {
    console.log('准备分配试卷:', {
      examId: examForm.id,
      studentIds: selectedStudents.value
    })

    const response = await examService.assignExam(examForm.id, selectedStudents.value)
    console.log('分配试卷响应:', response)

    // 修改这里的判断逻辑
    // 如果响应直接是 true，或者响应对象包含 success: true，都视为成功
    if (response === true || (response && response.success)) {
      // 尝试从响应对象获取消息，否则使用默认成功消息
      const successMessage = (response && typeof response === 'object' && response.message)
        ? response.message
        : '发布试卷成功';
      ElMessage.success(successMessage)
      studentDialogVisible.value = false

      // 添加延时，确保后端处理完成
      setTimeout(() => {
        loadExams()
      }, 1000)
    } else {
      // 如果响应是对象但 success 为 false，尝试获取错误消息
      const errorMessage = (response && typeof response === 'object' && (response.message || response.error))
        ? (response.message || response.error)
        : '未知错误';
      ElMessage.error(`发布试卷失败: ${errorMessage}，请重试`) // 显示更具体的错误或默认消息
    }
  } catch (error: any) {
    console.error('分配试卷时发生错误:', error)
    ElMessage.error(`分配试卷时发生错误: ${error.message || '请检查网络或联系管理员'}`)
  }
}

// 查看试卷统计
const statisticsLoading = ref(false)
const statisticsError = ref(false)

const viewStatistics = async (exam) => {
  if (!exam || !exam.id) {
    ElMessage.warning('试卷数据无效')
    return
  }

  statisticsLoading.value = true
  statisticsError.value = false
  currentExamStatistics.value = null
  statisticsDialogVisible.value = true

  try {
    console.log(`准备获取试卷 ${exam.id} 的统计信息`)
    const response = await examService.getExamStatistics(exam.id)
    console.log('获取的统计信息:', response)

    if (response && response.data) {
      currentExamStatistics.value = response.data

      // 如果没有试卷标题，使用当前试卷的标题
      if (!currentExamStatistics.value.examTitle) {
        currentExamStatistics.value.examTitle = exam.title
      }

      // 确保分数分布数据存在
      if (!currentExamStatistics.value.scoreDistribution) {
        currentExamStatistics.value.scoreDistribution = {}
        console.warn('缺少分数分布数据')
      }

      console.log('统计数据加载成功:', currentExamStatistics.value)
    } else {
      statisticsError.value = true
      ElMessage.warning('试卷统计数据为空，可能是因为还没有学生提交试卷')
    }
  } catch (error) {
    console.error('获取统计信息失败:', error)
    statisticsError.value = true
    ElMessage.error(`获取统计信息失败: ${error.message || '未知错误'}`)
  } finally {
    statisticsLoading.value = false
  }
}

// 导出成绩
const exportLoading = ref(false)

const exportResults = async (examId) => {
  if (!examId) {
    ElMessage.warning('试卷ID无效')
    return
  }

  exportLoading.value = true
  try {
    console.log(`准备导出试卷 ${examId} 的成绩`)

    // 如果有统计数据，直接生成CSV文件
    if (currentExamStatistics.value && currentExamStatistics.value.studentResults) {
      // 生成CSV内容
      const headers = ['\u5b66生ID', '\u5b66生姓名', '\u5f97分', '\u603b分', '\u6b63确题数', '\u9898目总数', '\u5b8c成时间(分钟)', '\u63d0交时间']
      const rows = currentExamStatistics.value.studentResults.map(student => [
        student.studentId,
        student.studentName,
        student.score,
        student.totalScore,
        student.correctCount,
        student.questionCount,
        student.completionTime,
        student.submittedAt
      ])

      // 添加标题行
      const csvContent = [
        headers.join(','),
        ...rows.map(row => row.join(','))
      ].join('\n')

      // 创建Blob对象
      const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
      const url = URL.createObjectURL(blob)

      // 创建下载链接
      const link = document.createElement('a')
      link.href = url
      link.setAttribute('download', `试卷成绩-${currentExamStatistics.value.examTitle || examId}-${new Date().toISOString().split('T')[0]}.csv`)
      document.body.appendChild(link)

      // 模拟点击下载
      link.click()

      // 清理
      document.body.removeChild(link)
      URL.revokeObjectURL(url)

      ElMessage.success('成绩导出成功')
    } else {
      // 如果没有统计数据，尝试从后端获取
      const response = await examService.exportExamResults(examId)
      console.log('导出成绩响应:', response)

      // 如果是Blob数据，直接下载
      if (response instanceof Blob) {
        const url = URL.createObjectURL(response)
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', `试卷成绩-${examId}-${new Date().toISOString().split('T')[0]}.csv`)
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        URL.revokeObjectURL(url)
        ElMessage.success('成绩导出成功')
      } else {
        ElMessage.success('成绩导出成功')
      }
    }
  } catch (error) {
    console.error('导出成绩失败:', error)
    ElMessage.error(`导出成绩失败: ${error.message || '未知错误'}`)
  } finally {
    exportLoading.value = false
  }
}

// 页面加载时获取试卷列表
onMounted(() => {
  loadExams()
})
</script>

<template>
  <div class="exam-manage-container">
    <div class="page-header">
      <h2>试卷管理</h2>
      <el-button type="primary" :icon="Plus" @click="createExam">创建试卷</el-button>
    </div>

    <el-card class="exam-list" v-loading="loading">
      <template v-if="!examList || examList.length === 0">
        <div class="empty-data">
          <el-empty description="暂无试卷数据" />
        </div>
      </template>

      <el-table v-else :data="examList" style="width: 100%" row-key="id">
        <el-table-column prop="title" label="试卷标题" min-width="200" />
        <el-table-column prop="description" label="描述" min-width="200" />
        <el-table-column label="题目数量" width="100">
          <template #default="scope">
            {{ Array.isArray(scope.row.questions) ? scope.row.questions.length : 0 }}
          </template>
        </el-table-column>
        <el-table-column label="总分" width="80">
          <template #default="scope">
            {{ scope.row.totalScore }}
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100">
          <template #default="scope">
            <el-tag :type="scope.row.status === 0 ? 'info' : scope.row.status === 1 ? 'success' : 'warning'">
              {{ scope.row.status === 0 ? '草稿' : scope.row.status === 1 ? '已发布' : '已结束' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="截止日期" width="180">
          <template #default="scope">
            {{ scope.row.deadline || '无' }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="280">
          <template #default="scope">
            <el-button-group style="display: flex; flex-wrap: wrap;">
              <el-button type="primary" :icon="Edit" size="small" @click="editExam(scope.row)">
                编辑
              </el-button>
              <el-button type="success" :icon="Upload" size="small" @click="publishExam(scope.row)">
                发布
              </el-button>
              <el-button type="info" :icon="View" size="small" @click="viewStatistics(scope.row)">
                统计
              </el-button>
              <el-button type="warning" :icon="Download" size="small" @click="exportResults(scope.row)">
                导出
              </el-button>
              <el-button type="danger" :icon="Delete" size="small" @click="deleteExam(scope.row)">
                删除
              </el-button>
            </el-button-group>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-container">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          :total="examList.length"
        />
      </div>
    </el-card>

    <!-- 试卷编辑对话框 -->
    <el-dialog
      v-model="examDialogVisible"
      :title="examForm.id ? '编辑试卷' : '创建试卷'"
      width="800px"
    >
      <el-form :model="examForm" label-width="100px">
        <el-form-item label="试卷标题" required>
          <el-input v-model="examForm.title" placeholder="请输入试卷标题" />
        </el-form-item>

        <el-form-item label="试卷描述">
          <el-input v-model="examForm.description" type="textarea" placeholder="请输入试卷描述" />
        </el-form-item>

        <el-form-item label="截止日期">
          <el-date-picker
            v-model="examForm.deadline"
            type="datetime"
            placeholder="选择截止日期"
            format="YYYY-MM-DD HH:mm:ss"
          />
        </el-form-item>

        <el-form-item label="试卷题目" required>
          <div class="question-list">
            <div v-if="selectedQuestions.length === 0" class="empty-questions">
              <el-empty description="暂无题目" />
            </div>

            <el-table v-else :data="selectedQuestions" style="width: 100%" row-key="id">
              <el-table-column label="序号" width="60">
                <template #default="scope">
                  {{ scope.$index + 1 }}
                </template>
              </el-table-column>
              <el-table-column label="题型" width="100">
                <template #default="scope">
                  {{ typeMap[scope.row.type] }}
                </template>
              </el-table-column>
              <el-table-column label="题目内容" min-width="300">
                <template #default="scope">
                  {{ scope.row.question || scope.row.content }}
                </template>
              </el-table-column>
              <el-table-column label="操作" width="80">
                <template #default="scope">
                  <el-button type="danger" :icon="Delete" circle size="small" @click="removeQuestion(scope.$index)" />
                </template>
              </el-table-column>
            </el-table>
          </div>

          <div class="add-question-btn">
            <el-button type="primary" :icon="Plus" @click="openQuestionDialog">添加题目</el-button>
          </div>
        </el-form-item>
      </el-form>

      <template #footer>
        <span class="dialog-footer">
          <el-button @click="examDialogVisible = false">取消</el-button>
          <el-button type="primary" @click="saveExam">保存</el-button>
        </span>
      </template>
    </el-dialog>

    <!-- 选择题目对话框 -->
    <el-dialog
      v-model="questionDialogVisible"
      title="选择题目"
      width="900px"
    >
      <!-- 这里可以复用题目管理中的题目选择组件 -->
      <question-selector
        :selected-questions="selectedQuestions"
        @select="selectQuestions"
      />
    </el-dialog>

    <!-- 选择学生对话框 -->
    <el-dialog
      v-model="studentDialogVisible"
      title="选择学生"
      width="600px"
    >
      <div v-loading="studentLoading">
        <template v-if="!studentList || studentList.length === 0">
          <div class="empty-data">
            <el-empty description="暂无学生数据" />
          </div>
        </template>

        <el-table
          v-else
          :data="studentList"
          style="width: 100%"
          @selection-change="selectedStudents = $event.map(item => item.id)"
          row-key="id"
        >
          <el-table-column type="selection" width="55" />
          <el-table-column prop="id" label="ID" width="80" />
          <el-table-column prop="username" label="用户名" min-width="150" />
          <el-table-column prop="name" label="姓名" min-width="150" />
        </el-table>
      </div>

      <template #footer>
        <span class="dialog-footer">
          <el-button @click="studentDialogVisible = false">取消</el-button>
          <el-button type="primary" @click="assignExamToStudents" :disabled="!studentList || studentList.length === 0 || selectedStudents.length === 0">发布</el-button>
        </span>
      </template>
    </el-dialog>

    <!-- 统计信息对话框 -->
    <el-dialog
      v-model="statisticsDialogVisible"
      title="试卷统计"
      width="900px"
      destroy-on-close
    >
      <div v-loading="statisticsLoading" class="statistics-container">
        <!-- 错误提示 -->
        <div v-if="statisticsError" class="statistics-error">
          <el-empty description="获取统计数据失败" :image-size="200">
            <template #description>
              <p>获取统计数据失败，请稍后重试</p>
            </template>
          </el-empty>
        </div>

        <!-- 空数据提示 -->
        <div v-else-if="!statisticsLoading && !currentExamStatistics" class="statistics-empty">
          <el-empty description="暂无统计数据" :image-size="200">
            <template #description>
              <p>试卷暂无统计数据，可能是因为还没有学生提交试卷</p>
            </template>
          </el-empty>
        </div>

        <!-- 统计数据内容 -->
        <div v-else-if="currentExamStatistics" class="statistics-content">
          <!-- 基本信息卡片 -->
          <el-card class="statistics-card">
            <template #header>
              <div class="card-header">
                <h3>基本信息</h3>
              </div>
            </template>
            <el-descriptions :column="3" border>
              <el-descriptions-item label="试卷标题">{{ currentExamStatistics.examTitle }}</el-descriptions-item>
              <el-descriptions-item label="参与学生数">{{ currentExamStatistics.studentCount }}</el-descriptions-item>
              <el-descriptions-item label="已提交数">{{ currentExamStatistics.submittedCount }}</el-descriptions-item>
              <el-descriptions-item label="平均分">{{ currentExamStatistics.averageScore ? currentExamStatistics.averageScore.toFixed(2) : '0.00' }}</el-descriptions-item>
              <el-descriptions-item label="最高分">{{ currentExamStatistics.highestScore || '0' }}</el-descriptions-item>
              <el-descriptions-item label="最低分">{{ currentExamStatistics.lowestScore || '0' }}</el-descriptions-item>
              <el-descriptions-item label="及格率">{{ currentExamStatistics.passRate ? (currentExamStatistics.passRate * 100).toFixed(2) : '0.00' }}%</el-descriptions-item>
            </el-descriptions>
          </el-card>

          <!-- 分数分布卡片 -->
          <el-card class="statistics-card">
            <template #header>
              <div class="card-header">
                <h3>分数分布</h3>
              </div>
            </template>
            <div class="score-distribution">
              <el-progress
                v-for="(count, range) in currentExamStatistics.scoreDistribution"
                :key="range"
                :percentage="currentExamStatistics.studentCount ? (count / currentExamStatistics.studentCount) * 100 : 0"
                :color="getScoreColor(range as string)"
                :stroke-width="20"
                :format="() => `${range}: ${count}人 (${currentExamStatistics.studentCount ? ((count / currentExamStatistics.studentCount) * 100).toFixed(2) : '0.00'}%)`"
                style="margin-bottom: 10px"
              />
            </div>
          </el-card>

          <!-- 题目正确率卡片 -->
          <el-card class="statistics-card" v-if="currentExamStatistics.questionStatistics && currentExamStatistics.questionStatistics.length > 0">
            <template #header>
              <div class="card-header">
                <h3>题目正确率</h3>
              </div>
            </template>
            <el-table :data="currentExamStatistics.questionStatistics" style="width: 100%" row-key="order">
              <el-table-column prop="order" label="序号" width="60" />
              <el-table-column label="题型" width="100">
                <template #default="scope">
                  {{ typeMap[scope.row.type] || '未知题型' }}
                </template>
              </el-table-column>
              <el-table-column prop="content" label="题目内容" min-width="300" />
              <el-table-column label="正确率" width="180">
                <template #default="scope">
                  <el-progress
                    :percentage="scope.row.correctRate * 100"
                    :color="scope.row.correctRate > 0.6 ? '#67C23A' : scope.row.correctRate > 0.3 ? '#E6A23C' : '#F56C6C'"
                  />
                </template>
              </el-table-column>
              <el-table-column label="正确/错误" width="100">
                <template #default="scope">
                  {{ scope.row.correctCount }}/{{ scope.row.incorrectCount }}
                </template>
              </el-table-column>
            </el-table>
          </el-card>

          <!-- 学生成绩卡片 -->
          <el-card class="statistics-card" v-if="currentExamStatistics.studentResults && currentExamStatistics.studentResults.length > 0">
            <template #header>
              <div class="card-header">
                <h3>学生成绩</h3>
                <el-button type="primary" size="small" @click="exportResults(currentExamStatistics.examId)" :loading="exportLoading">导出成绩</el-button>
              </div>
            </template>
            <el-table :data="currentExamStatistics.studentResults" style="width: 100%" row-key="studentId">
              <el-table-column prop="studentId" label="学生ID" width="80" />
              <el-table-column prop="studentName" label="学生姓名" min-width="150" />
              <el-table-column label="得分" width="100">
                <template #default="scope">
                  <span :class="{ 'score-pass': scope.row.score >= 60, 'score-fail': scope.row.score < 60 }">
                    {{ scope.row.score }}/{{ scope.row.totalScore }}
                  </span>
                </template>
              </el-table-column>
              <el-table-column label="正确题数" width="100">
                <template #default="scope">
                  {{ scope.row.correctCount }}/{{ scope.row.questionCount }}
                </template>
              </el-table-column>
              <el-table-column prop="completionTime" label="完成时间(分钟)" width="150" />
              <el-table-column prop="submittedAt" label="提交时间" min-width="180" />
            </el-table>
          </el-card>
        </div>
      </div>

      <template #footer>
        <span class="dialog-footer">
          <el-button @click="statisticsDialogVisible = false">关闭</el-button>
          <el-button type="primary" @click="exportResults(currentExamStatistics?.examId)" :loading="exportLoading" :disabled="!currentExamStatistics || !currentExamStatistics.studentResults || currentExamStatistics.studentResults.length === 0">导出成绩</el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.exam-manage-container {
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

.pagination-container {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}

.question-list {
  margin-bottom: 15px;
  border: 1px solid #EBEEF5;
  border-radius: 4px;
  padding: 10px;
  max-height: 300px;
  overflow-y: auto;
}

.empty-questions {
  padding: 20px 0;
}

.add-question-btn {
  margin-top: 10px;
}

.statistics-container {
  max-height: 600px;
  overflow-y: auto;
}

.statistics-container h3 {
  margin: 0;
  padding: 0;
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}

.statistics-error,
.statistics-empty {
  padding: 40px 0;
  text-align: center;
}

.statistics-content {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.statistics-card {
  margin-bottom: 0;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.score-distribution {
  padding: 15px;
  background-color: #F5F7FA;
  border-radius: 4px;
}

.score-pass {
  color: #67C23A;
  font-weight: bold;
}

.score-fail {
  color: #F56C6C;
  font-weight: bold;
}
</style>

<script lang="ts">
// 获取分数区间的颜色
function getScoreColor(range: string): string {
  if (range === '90-100') return '#67C23A'
  if (range === '80-89') return '#85CE61'
  if (range === '70-79') return '#E6A23C'
  if (range === '60-69') return '#F56C6C'
  return '#909399'
}
</script>
