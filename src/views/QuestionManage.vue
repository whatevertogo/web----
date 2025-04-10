<script setup lang="ts">
import { ref, reactive, computed, onMounted, onActivated } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance } from 'element-plus'
import { QuestionType, TypeMap, TableQuestion } from '../types/question'
import { questionService } from '../services/questionService'
import { useUserStore } from '../stores/userStore'

// 获取路由器实例
const router = useRouter()

// 题型映射
const typeMap: TypeMap = {
  [QuestionType.Single]: '单选题',
  [QuestionType.Judge]: '判断题',
  [QuestionType.Fill]: '填空题',
  [QuestionType.Program]: '编程题',
  [QuestionType.ShortAnswer]: '简答题'
}

// 表格数据改为使用服务中的数据
const tableData = ref<TableQuestion[]>([])

// 表格加载状态
const loading = ref(false)

// 查看详情对话框
const detailDialogVisible = ref(false)
const currentQuestion = ref<any>(null)

// 修改搜索表单初始值
const searchForm = reactive({
  keyword: '',
  type: '',
  dateRange: [] as [Date, Date] | []
})

// 搜索表单引用
const searchFormRef = ref<FormInstance>()

const userStore = useUserStore()

// 搜索方法
const handleSearch = async () => {
  loading.value = true
  try {
    const questions = await questionService.getQuestions({
      keyword: searchForm.keyword,
      type: searchForm.type ? Number(searchForm.type) as QuestionType : undefined,
      dateRange: searchForm.dateRange.length ? 
        [searchForm.dateRange[0], searchForm.dateRange[1]] : undefined
    })
    tableData.value = questions
  } catch (error) {
    ElMessage.error('搜索失败')
  } finally {
    loading.value = false
  }
}

// 修改重置方法
const handleReset = () => {
  // 手动重置表单值
  searchForm.keyword = ''
  searchForm.type = ''
  searchForm.dateRange = []
  
  // 重新搜索
  handleSearch()
}

// 获取试题详情
const showDetail = async (row: TableQuestion) => {
  loading.value = true
  try {
    currentQuestion.value = {
      ...row,
      options: row.options ?? [],
      correctAnswer: row.correctAnswer ?? '',
      analysis: row.analysis ?? '',
      answers: row.answers ?? [],
      sampleInput: row.sampleInput ?? '',
      sampleOutput: row.sampleOutput ?? '',
      referenceAnswer: row.referenceAnswer ?? ''
    }
    detailDialogVisible.value = true
  } catch (error) {
    ElMessage.error('获取详情失败')
  } finally {
    loading.value = false
  }
}

// 删除试题
const handleDelete = (row: TableQuestion) => {
  ElMessageBox.confirm(
    '确定要删除这道试题吗？此操作不可恢复',
    '警告',
    {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    }
  ).then(() => {
    if (questionService.deleteQuestion(row.id)) {
      ElMessage.success('删除成功')
    } else {
      ElMessage.error('删除失败')
    }
  }).catch(() => {
    ElMessage.info('已取消删除')
  })
}

const isEditing = ref(false)
const editForm = ref<any>({})

const startEdit = () => {
  editForm.value = JSON.parse(JSON.stringify(currentQuestion.value))
  isEditing.value = true
}

const saveEdit = async () => {
  try {
    // 添加表单验证
    
    // 更新问题
    const updated = {
      ...editForm.value,
      id: currentQuestion.value.id,
      createTime: currentQuestion.value.createTime
    }
    
    // 调用更新方法
    await questionService.updateQuestion(updated)
    ElMessage.success('保存成功')
    isEditing.value = false  // 退出编辑模式
    await loadQuestions() // 确保重新加载最新数据
    detailDialogVisible.value = false
  } catch (error) {
    console.error('保存失败:', error)
    ElMessage.error('保存失败')
  }
}

// 添加分页相关数据
const pagination = reactive({
  currentPage: 1,
  pageSize: 10,
  total: computed(() => tableData.value.length)
})

// 分页变化处理
const handlePageChange = (page: number) => {
  pagination.currentPage = page
}

const handleSizeChange = (size: number) => {
  pagination.pageSize = size
  pagination.currentPage = 1
}

// 计算当前页数据
const currentPageData = computed(() => {
  const start = (pagination.currentPage - 1) * pagination.pageSize
  const end = start + pagination.pageSize
  return tableData.value.slice(start, end)
})

// 添加 loadQuestions 函数
const loadQuestions = async () => {
  loading.value = true
  try {
    // 强制重新加载数据，而不是使用缓存
    const questions = await questionService.getQuestions() as TableQuestion[]
    tableData.value = questions
    console.log('加载到的题目数量:', questions.length)
  } catch (error) {
    console.error('加载题目失败:', error)
    ElMessage.error('加载题目失败')
  } finally {
    loading.value = false
  }
}

// 初始加载
onMounted(() => {
  loadQuestions()
})

onActivated(() => {
  loadQuestions()
})
</script>

<template>
  <div class="question-manage">
    <!-- 搜索区域 -->
    <el-card class="search-card">
      <el-form
        ref="searchFormRef"
        :model="searchForm"
        inline
      >
        <el-form-item label="关键词" prop="keyword">
          <el-input
            v-model="searchForm.keyword"
            placeholder="题目内容"
            clearable
            @keyup.enter="handleSearch"
          />
        </el-form-item>
        <el-form-item label="题型" prop="type">
          <el-select v-model="searchForm.type" placeholder="请选择" clearable>
            <el-option
              v-for="(label, key) in typeMap"
              :key="key"
              :label="label"
              :value="key"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="创建时间" prop="dateRange">
          <el-date-picker
            v-model="searchForm.dateRange"
            type="daterange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            value-format="YYYY-MM-DD"
            :shortcuts="[
              {
                text: '最近一周',
                value: () => {
                  const end = new Date()
                  const start = new Date()
                  start.setTime(start.getTime() - 3600 * 1000 * 24 * 7)
                  return [start, end]
                }
              },
              {
                text: '最近一个月',
                value: () => {
                  const end = new Date()
                  const start = new Date()
                  start.setMonth(start.getMonth() - 1)
                  return [start, end]
                }
              }
            ]"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">
            <el-icon><Search /></el-icon>搜索
          </el-button>
          <el-button @click="handleReset">
            <el-icon><Refresh /></el-icon>重置
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 表格区域 -->
    <el-card class="table-card">
      <template #header>
        <div class="table-header">
          <span>试题列表</span>
          <el-button 
            v-if="userStore.isAdmin()"
            type="primary" 
            @click="router.push('/input')"
          >
            <el-icon><Plus /></el-icon>新增试题
          </el-button>
        </div>
      </template>

      <el-table
        :data="currentPageData"
        v-loading="loading"
        style="width: 100%"
      >
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="type" label="题型" width="100">
          <template #default="{ row }">
            <el-tag>{{ typeMap[row.type] }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="question" label="题目内容">
          <template #default="{ row }">
            <el-text
              class="question-content"
              truncated
            >
              {{ row.question }}
            </el-text>
          </template>
        </el-table-column>
        <el-table-column prop="createTime" label="创建时间" width="180" />
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button-group>
              <el-button size="small" @click="showDetail(row)">
                <el-icon><View /></el-icon>查看
              </el-button>
              <template v-if="userStore.isAdmin()">
                <el-button size="small" type="danger" @click="handleDelete(row)">
                  <el-icon><Delete /></el-icon>删除
                </el-button>
              </template>
            </el-button-group>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination">
        <el-pagination
          v-model:current-page="pagination.currentPage"
          v-model:page-size="pagination.pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="pagination.total"
          @size-change="handleSizeChange"
          @current-change="handlePageChange"
          layout="total, sizes, prev, pager, next"
          background
        />
      </div>
    </el-card>

    <!-- 详情对话框 -->
    <el-dialog
      v-model="detailDialogVisible"
      :title="isEditing ? '编辑试题' : '试题详情'"
      width="50%"
    >
      <template v-if="currentQuestion">
        <el-form v-if="isEditing" :model="editForm" label-width="100px">
          <el-form-item label="题型">
            <el-select v-model="editForm.type">
              <el-option
                v-for="(label, type) in typeMap"
                :key="type"
                :label="label"
                :value="type"
              />
            </el-select>
          </el-form-item>

          <el-form-item label="题目内容">
            <el-input
              v-model="editForm.question"
              type="textarea"
              :rows="3"
            />
          </el-form-item>

          <!-- 单选题选项 -->
          <template v-if="editForm.type === 'single'">
            <el-form-item
              v-for="(option, index) in editForm.options"
              :key="index"
              :label="`选项${String.fromCharCode(65 + index)}`"
            >
              <el-input v-model="editForm.options[index]">
                <template #append>
                  <el-radio
                    v-model="editForm.correctAnswer"
                    :label="String.fromCharCode(65 + index)"
                  />
                </template>
              </el-input>
            </el-form-item>
            <el-button @click="editForm.options.push('')">添加选项</el-button>
          </template>

          <!-- 判断题 -->
          <template v-if="editForm.type === 'judge'">
            <el-form-item label="正确答案">              <el-radio-group v-model="editForm.correctAnswer">
                <el-radio value="正确">正确</el-radio>
                <el-radio value="错误">错误</el-radio>
              </el-radio-group>
            </el-form-item>
          </template>

          <!-- 填空题 -->
          <template v-if="editForm.type === 'fill'">
            <el-form-item
              v-for="(answer, index) in editForm.answers"
              :key="index"
              :label="`空${index + 1}答案`"
            >
              <el-input v-model="editForm.answers[index]" />
            </el-form-item>
            <el-button @click="editForm.answers.push('')">添加空</el-button>
          </template>

          <el-form-item label="解析">
            <el-input
              v-model="editForm.analysis"
              type="textarea"
              :rows="3"
            />
          </el-form-item>
        </el-form>

        <el-descriptions v-else :column="1" border>
          <el-descriptions-item label="题型">
            {{ typeMap[currentQuestion.type] }}
          </el-descriptions-item>
          <el-descriptions-item label="题目内容">
            {{ currentQuestion.question }}
          </el-descriptions-item>
          
          <!-- 单选题特有字段 -->
          <template v-if="currentQuestion.type === QuestionType.Single">
            <el-descriptions-item 
              v-for="(option, index) in currentQuestion.options"
              :key="index"
              :label="`选项${String.fromCharCode(65 + index)}`"
            >
              {{ option }}
            </el-descriptions-item>
            <el-descriptions-item label="正确答案">
              选项{{ currentQuestion.correctAnswer }}
            </el-descriptions-item>
          </template>

          <!-- 判断题特有字段 -->
          <template v-else-if="currentQuestion.type === QuestionType.Judge">
            <el-descriptions-item label="正确答案">
              {{ currentQuestion.correctAnswer === 'true' ? '正确' : '错误' }}
            </el-descriptions-item>
          </template>

          <!-- 填空题特有字段 -->
          <template v-else-if="currentQuestion.type === QuestionType.Fill">
            <el-descriptions-item 
              v-for="(answer, index) in currentQuestion.answers"
              :key="index"
              :label="`填空${index + 1}答案`"
            >
              {{ answer }}
            </el-descriptions-item>
          </template>

          <!-- 编程题特有字段 -->
          <template v-else-if="currentQuestion.type === QuestionType.Program">
            <el-descriptions-item label="示例输入">
              <pre>{{ currentQuestion.sampleInput }}</pre>
            </el-descriptions-item>
            <el-descriptions-item label="示例输出">
              <pre>{{ currentQuestion.sampleOutput }}</pre>
            </el-descriptions-item>
          </template>

          <!-- 简答题特有字段 -->
          <template v-else-if="currentQuestion.type === QuestionType.ShortAnswer">
            <el-descriptions-item label="参考答案">
              {{ currentQuestion.referenceAnswer }}
            </el-descriptions-item>
          </template>

          <!-- 所有题型都有的解析字段 -->
          <el-descriptions-item label="答案解析">
            {{ currentQuestion.analysis }}
          </el-descriptions-item>
        </el-descriptions>
      </template>

      <template #footer>
        <el-button @click="detailDialogVisible = false">关闭</el-button>
        <template v-if="isEditing">
          <el-button type="primary" @click="saveEdit">保存</el-button>
        </template>
        <template v-else-if="userStore.isAdmin()">
          <el-button type="primary" @click="startEdit">编辑</el-button>
        </template>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.question-manage {
  padding: 20px;
}

.search-card {
  margin-bottom: 20px;
}

.table-card {
  margin-bottom: 20px;
}

.table-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.question-content {
  display: inline-block;
  max-width: 400px;
}

.pagination {
  margin-top: 20px;
  text-align: right;
}

.el-button [class^="el-icon"] {
  margin-right: 5px;
}

:deep(.el-descriptions__label) {
  width: 120px;
}
</style>