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
    examList.value = response.data
  } catch (error) {
    console.error('加载试卷失败:', error)
    ElMessage.error('加载试卷失败')
  } finally {
    loading.value = false
  }
}

// 加载学生列表
const loadStudents = async () => {
  try {
    const response = await examService.getStudents()
    studentList.value = response.data
  } catch (error) {
    console.error('加载学生列表失败:', error)
    ElMessage.error('加载学生列表失败')
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
const editExam = (exam) => {
  examForm.id = exam.id
  examForm.title = exam.title
  examForm.description = exam.description
  examForm.deadline = exam.deadline
  examForm.questions = [...exam.questions]
  selectedQuestions.value = [...exam.questions.map(q => q.question)]
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

    if (examForm.id) {
      await examService.updateExam(examData)
      ElMessage.success('更新试卷成功')
    } else {
      await examService.createExam(examData)
      ElMessage.success('创建试卷成功')
    }

    examDialogVisible.value = false
    loadExams()
  } catch (error) {
    console.error('保存试卷失败:', error)
    ElMessage.error('保存试卷失败')
  }
}

// 打开选择题目对话框
const openQuestionDialog = () => {
  questionService.loadQuestions()
  questionDialogVisible.value = true
}

// 选择题目
const selectQuestions = (questions) => {
  selectedQuestions.value = questions
  questionDialogVisible.value = false
}

// 移除选中的题目
const removeQuestion = (index) => {
  selectedQuestions.value.splice(index, 1)
}

// 发布试卷
const publishExam = (exam) => {
  selectedStudents.value = []
  loadStudents()
  studentDialogVisible.value = true
  examForm.id = exam.id
}

// 分配试卷给学生
const assignExam = async () => {
  if (selectedStudents.value.length === 0) {
    ElMessage.warning('请选择至少一名学生')
    return
  }

  try {
    await examService.assignExam(examForm.id, selectedStudents.value)
    ElMessage.success('发布试卷成功')
    studentDialogVisible.value = false
    loadExams()
  } catch (error) {
    console.error('发布试卷失败:', error)
    ElMessage.error('发布试卷失败')
  }
}

// 查看试卷统计
const viewStatistics = async (exam) => {
  try {
    const response = await examService.getExamStatistics(exam.id)
    currentExamStatistics.value = response.data
    statisticsDialogVisible.value = true
  } catch (error) {
    console.error('获取统计信息失败:', error)
    ElMessage.error('获取统计信息失败')
  }
}

// 导出成绩
const exportResults = (exam) => {
  // TODO: 实现导出成绩功能
  ElMessage.info('导出成绩功能开发中')
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
            {{ scope.row.questions.length }}
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
            <el-button-group>
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

            <el-table v-else :data="selectedQuestions" style="width: 100%">
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
              <el-table-column prop="question" label="题目内容" min-width="300" />
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
      <el-table
        :data="studentList"
        style="width: 100%"
        @selection-change="selectedStudents = $event.map(item => item.id)"
      >
        <el-table-column type="selection" width="55" />
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="username" label="用户名" min-width="150" />
        <el-table-column prop="name" label="姓名" min-width="150" />
      </el-table>

      <template #footer>
        <span class="dialog-footer">
          <el-button @click="studentDialogVisible = false">取消</el-button>
          <el-button type="primary" @click="assignExam">发布</el-button>
        </span>
      </template>
    </el-dialog>

    <!-- 统计信息对话框 -->
    <el-dialog
      v-model="statisticsDialogVisible"
      title="试卷统计"
      width="900px"
    >
      <div v-if="currentExamStatistics" class="statistics-container">
        <el-descriptions title="基本信息" :column="3" border>
          <el-descriptions-item label="试卷标题">{{ currentExamStatistics.examTitle }}</el-descriptions-item>
          <el-descriptions-item label="参与学生数">{{ currentExamStatistics.studentCount }}</el-descriptions-item>
          <el-descriptions-item label="已提交数">{{ currentExamStatistics.submittedCount }}</el-descriptions-item>
          <el-descriptions-item label="平均分">{{ currentExamStatistics.averageScore.toFixed(2) }}</el-descriptions-item>
          <el-descriptions-item label="最高分">{{ currentExamStatistics.highestScore }}</el-descriptions-item>
          <el-descriptions-item label="最低分">{{ currentExamStatistics.lowestScore }}</el-descriptions-item>
          <el-descriptions-item label="及格率">{{ (currentExamStatistics.passRate * 100).toFixed(2) }}%</el-descriptions-item>
        </el-descriptions>

        <h3>分数分布</h3>
        <div class="score-distribution">
          <el-progress
            v-for="(count, range) in currentExamStatistics.scoreDistribution"
            :key="range"
            :percentage="currentExamStatistics.studentCount ? (count / currentExamStatistics.studentCount) * 100 : 0"
            :color="getScoreColor(range as string)"
            :stroke-width="20"
            :format="() => `${range}: ${count}人 (${((count / currentExamStatistics.studentCount) * 100).toFixed(2)}%)`"
            style="margin-bottom: 10px"
          />
        </div>

        <h3>题目正确率</h3>
        <el-table :data="currentExamStatistics.questionStatistics" style="width: 100%">
          <el-table-column prop="order" label="序号" width="60" />
          <el-table-column label="题型" width="100">
            <template #default="scope">
              {{ typeMap[scope.row.type] }}
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

        <h3>学生成绩</h3>
        <el-table :data="currentExamStatistics.studentResults" style="width: 100%">
          <el-table-column prop="studentId" label="学生ID" width="80" />
          <el-table-column prop="studentName" label="学生姓名" min-width="150" />
          <el-table-column label="得分" width="100">
            <template #default="scope">
              {{ scope.row.score }}/{{ scope.row.totalScore }}
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
      </div>
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
  margin: 20px 0 10px;
  padding-bottom: 10px;
  border-bottom: 1px solid #EBEEF5;
}

.score-distribution {
  padding: 10px;
  background-color: #F5F7FA;
  border-radius: 4px;
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
