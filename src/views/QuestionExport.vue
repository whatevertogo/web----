<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { QuestionType, TableQuestion } from '../types/question'
import { questionService } from '../services/questionService'
import { exportToWord } from '../utils/wordExport.js'
import * as XLSX from 'xlsx'
import { Search, Refresh } from '@element-plus/icons-vue'

// 导出设置
const exportSettings = ref({
  format: 'excel', // 'excel' 或 'word'
  range: 'all', // 'all' 或 'filtered_selected'
  includeAnalysis: true
})

// 搜索条件
const searchForm = ref({
  keyword: '',
  type: '',
  dateRange: [] as [Date, Date] | []
})

// 题型映射
const typeMap = {
  [QuestionType.Single]: '单选题',
  [QuestionType.Judge]: '判断题',
  [QuestionType.Fill]: '填空题',
  [QuestionType.Program]: '编程题',
  [QuestionType.ShortAnswer]: '简答题'
}

// 获取筛选后的题目
const filteredQuestions = ref<TableQuestion[]>([])
// 选中的题目ID
const selectedQuestionIds = ref<number[]>([])
const loading = ref(false)

const updateFilteredQuestions = async () => {
  loading.value = true
  try {
    const questions = await questionService.getQuestions({
      keyword: searchForm.value.keyword,
      type: searchForm.value.type ? Number(searchForm.value.type) as QuestionType : undefined,
      dateRange: searchForm.value.dateRange.length ?
        [searchForm.value.dateRange[0], searchForm.value.dateRange[1]] : undefined
    })
    filteredQuestions.value = questions
    // 重置选中的题目
    selectedQuestionIds.value = []
  } catch (error: any) {
    console.error('获取筛选题目失败:', error)
    ElMessage.error(`获取题目失败: ${error.message || '未知错误'}`)
    filteredQuestions.value = []
  } finally {
    loading.value = false
  }
}

// 重置筛选条件
const resetFilter = () => {
  searchForm.value.keyword = ''
  searchForm.value.type = ''
  searchForm.value.dateRange = []
  updateFilteredQuestions()
}

// 导出题目数量
const questionCount = computed(() => {
  if (exportSettings.value.range === 'filtered_selected') {
    // 如果是筛选并选择模式，返回选中的题目数量
    return selectedQuestionIds.value.length
  } else {
    // 如果是全部题目模式，返回所有题目数量
    return questionService.questions.value.length
  }
})

// 获取要导出的题目
const getExportQuestions = () => {
  if (exportSettings.value.range === 'filtered_selected') {
    // 如果是筛选并选择模式，返回选中的题目
    return filteredQuestions.value.filter(q => selectedQuestionIds.value.includes(q.id))
  } else {
    // 如果是全部题目模式，返回所有题目
    return questionService.questions.value
  }
}

// 当导出范围变化时更新结果
watch(() => exportSettings.value.range, (newRange) => {
  if (newRange === 'filtered_selected') {
    // 如果切换到筛选并选择模式，自动加载题目
    updateFilteredQuestions()
  }
})

// 初始加载数据
onMounted(() => {
  // 检查是否有从 QuestionManage 传递过来的选中题目
  if (questionService.selectedQuestions && questionService.selectedQuestions.length > 0) {
    // 如果有，自动切换到筛选并选择模式
    exportSettings.value.range = 'filtered_selected'
    // 将选中的题目加载到筛选结果中
    filteredQuestions.value = questionService.questions.value
    // 设置选中的题目 ID
    selectedQuestionIds.value = questionService.selectedQuestions.map(q => q.id)
  } else {
    // 如果没有，加载所有题目
    updateFilteredQuestions()
  }
})

// 执行导出
const handleExport = async () => {
  if (questionCount.value === 0) {
    ElMessage.warning('没有可导出的题目')
    return
  }

  loading.value = true
  try {
    const questions = getExportQuestions()

    if (exportSettings.value.format === 'excel') {
      await exportToExcel(questions)
      ElMessage.success('Excel导出成功')
    } else {
      // Word导出进度提示
      ElMessage({
        type: 'info',
        message: '正在生成Word文档，请稍候...',
        duration: 2000
      })
      await exportToWord(questions, typeMap)
      ElMessage.success({
        message: 'Word导出成功，请查看下载的文件',
        duration: 3000
      })
    }
  } catch (error) {
    console.error('导出失败:', error)
    ElMessage.error({
      message: `导出失败: ${error.message || '未知错误'}`,
      duration: 5000
    })
  } finally {
    loading.value = false
  }
}

// Excel导出
const exportToExcel = async (questions: any[]) => {
  try {
    // 准备数据
    const data = questions.map(q => {
      // 基本信息
      const baseInfo = {
        题号: q.id,
        题型: typeMap[q.type],
        题目内容: q.question,
        创建时间: q.createTime,
      }

      // 根据题型添加特定字段
      let specificInfo = {}

      switch(q.type) {
        case QuestionType.Single:
          specificInfo = {
            选项A: q.options?.[0] || '',
            选项B: q.options?.[1] || '',
            选项C: q.options?.[2] || '',
            选项D: q.options?.[3] || '',
            正确答案: q.correctAnswer || ''
          }
          break
        case QuestionType.Judge:
          specificInfo = {
            正确答案: q.correctAnswer === 'true' ? '正确' : '错误'
          }
          break
        case QuestionType.Fill:
          specificInfo = {
            答案: q.answers?.join('，') || ''
          }
          break
        case QuestionType.Program:
          specificInfo = {
            示例输入: q.sampleInput || '',
            示例输出: q.sampleOutput || ''
          }
          break
        case QuestionType.ShortAnswer:
          specificInfo = {
            参考答案: q.referenceAnswer || ''
          }
          break
      }

      // 添加解析（如果需要）
      const analysisInfo = exportSettings.value.includeAnalysis ? { 解析: q.analysis || '' } : {}

      // 合并所有信息
      return {
        ...baseInfo,
        ...specificInfo,
        ...analysisInfo
      }
    })

    // 创建工作簿和工作表
    const worksheet = XLSX.utils.json_to_sheet(data)
    const workbook = XLSX.utils.book_new()
    XLSX.utils.book_append_sheet(workbook, worksheet, '题库')

    // 导出
    XLSX.writeFileXLSX(workbook, `题库导出_${new Date().toLocaleDateString()}.xlsx`)
    return true
  } catch (error) {
    console.error('Excel导出失败:', error)
    throw new Error('Excel导出失败: ' + error.message)
  }
}

// 处理选择变化
const handleSelectionChange = (selection: TableQuestion[]) => {
  selectedQuestionIds.value = selection.map(item => item.id)
}

// 全选/取消全选
const handleSelectAll = (val: boolean) => {
  if (val) {
    // 如果是筛选并选择模式，则全选筛选后的题目
    if (exportSettings.value.range === 'filtered_selected') {
      selectedQuestionIds.value = filteredQuestions.value.map(q => q.id)
    } else {
      selectedQuestionIds.value = questionService.questions.value.map(q => q.id)
    }
  } else {
    selectedQuestionIds.value = []
  }
}
</script>

<template>
  <div class="export-page">
    <h2>导出试题</h2>

    <el-card class="export-card">
      <div v-loading="loading">
        <div class="export-options">
          <el-form label-width="100px">
            <el-form-item label="导出格式">              <el-radio-group v-model="exportSettings.format">
                <el-radio value="excel">Excel表格</el-radio>
                <el-radio value="word">Word文档</el-radio>
              </el-radio-group>
            </el-form-item>

            <el-form-item label="导出范围">
              <el-radio-group v-model="exportSettings.range">
                <el-radio label="all">全部题目</el-radio>
                <el-radio label="filtered_selected">筛选并选择题目</el-radio>
              </el-radio-group>
            </el-form-item>

            <el-form-item label="包含解析">
              <el-switch v-model="exportSettings.includeAnalysis" />
            </el-form-item>
          </el-form>
        </div>

        <!-- 筛选并选择区域 -->
        <div v-if="exportSettings.range === 'filtered_selected'" class="filter-select-area">
          <!-- 筛选条件 -->
          <div class="filter-options">
            <h3>筛选条件</h3>
            <el-form :model="searchForm" label-width="100px">
              <el-form-item label="关键词">
                <el-input v-model="searchForm.keyword" placeholder="搜索题目内容" />
              </el-form-item>

              <el-form-item label="题型">
                <el-select v-model="searchForm.type" placeholder="选择题型" clearable>
                  <el-option
                    v-for="(label, value) in typeMap"
                    :key="value"
                    :label="label"
                    :value="Number(value)"
                  />
                </el-select>
              </el-form-item>

              <el-form-item label="创建日期">
                <el-date-picker
                  v-model="searchForm.dateRange"
                  type="daterange"
                  range-separator="至"
                  start-placeholder="开始日期"
                  end-placeholder="结束日期"
                  format="YYYY-MM-DD"
                />
              </el-form-item>

              <el-form-item>
                <el-button type="primary" @click="updateFilteredQuestions">
                  <el-icon><Search /></el-icon> 筛选
                </el-button>
                <el-button @click="resetFilter">
                  <el-icon><Refresh /></el-icon> 重置
                </el-button>
              </el-form-item>
            </el-form>
          </div>

          <!-- 题目选择表格 -->
          <div class="question-table">
            <h3>选择题目</h3>
            <el-table
              :data="filteredQuestions"
              style="width: 100%"
              @selection-change="handleSelectionChange"
              v-loading="loading"
            >
              <el-table-column
                type="selection"
                width="55"
              />
              <el-table-column
                prop="id"
                label="ID"
                width="80"
              />
              <el-table-column
                prop="type"
                label="题型"
                width="100"
              >
                <template #default="scope">
                  {{ typeMap[scope.row.type] }}
                </template>
              </el-table-column>
              <el-table-column
                prop="question"
                label="题目内容"
              >
                <template #default="scope">
                  <div class="question-content">{{ scope.row.question }}</div>
                </template>
              </el-table-column>
              <el-table-column
                prop="createTime"
                label="创建时间"
                width="180"
              />
            </el-table>

            <div class="table-footer">
              <el-checkbox
                :indeterminate="selectedQuestionIds.length > 0 && selectedQuestionIds.length < filteredQuestions.length"
                :checked="selectedQuestionIds.length === filteredQuestions.length && filteredQuestions.length > 0"
                @change="handleSelectAll"
              >
                全选
              </el-checkbox>

              <div>已选择 {{ selectedQuestionIds.length }} 道题目</div>
            </div>
          </div>
        </div>

        <!-- 预览选中的题目 -->
        <div v-if="exportSettings.range === 'all' && questionCount > 0" class="preview-questions">
          <h3>将导出以下题目:</h3>
          <el-table
            :data="getExportQuestions().slice(0, 5)"
            style="width: 100%"
          >
            <el-table-column
              prop="id"
              label="ID"
              width="80"
            />
            <el-table-column
              prop="type"
              label="题型"
              width="100"
            >
              <template #default="scope">
                {{ typeMap[scope.row.type] }}
              </template>
            </el-table-column>
            <el-table-column
              prop="question"
              label="题目内容"
            >
              <template #default="scope">
                <div class="question-content">{{ scope.row.question }}</div>
              </template>
            </el-table-column>
          </el-table>
          <div v-if="questionCount > 5" class="more-questions">
            ...以及其他 {{ questionCount - 5 }} 道题目
          </div>
        </div>

        <div class="export-summary">
          <p>将导出 <strong>{{ questionCount }}</strong> 道试题</p>
        </div>

        <div class="export-actions">
          <el-button
            type="primary"
            :loading="loading"
            @click="handleExport"
            :disabled="questionCount === 0"
          >
            导出试题
          </el-button>
        </div>
      </div>
    </el-card>
  </div>
</template>

<style scoped>
.export-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.export-card {
  margin-bottom: 20px;
}

.export-options {
  margin-bottom: 20px;
}

.question-list {
  margin-bottom: 20px;
}

.question-content {
  max-width: 400px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.table-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 10px;
  padding: 10px 0;
}

.filter-select-area {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.filter-options {
  margin-bottom: 20px;
  border: 1px solid #ebeef5;
  border-radius: 4px;
  padding: 15px;
}

.filter-options h3,
.question-table h3 {
  margin-top: 0;
  margin-bottom: 15px;
  font-size: 16px;
  color: #606266;
}

.question-table {
  margin: 20px 0;
  border: 1px solid #ebeef5;
  border-radius: 4px;
  padding: 15px;
}

.preview-questions {
  margin: 20px 0;
  border: 1px solid #ebeef5;
  border-radius: 4px;
  padding: 15px;
}

.preview-questions h3 {
  margin-top: 0;
  margin-bottom: 15px;
  font-size: 16px;
  color: #606266;
}

.more-questions {
  text-align: center;
  color: #909399;
  padding: 10px;
}

.export-actions {
  text-align: center;
  margin-top: 20px;
}
</style>