<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { QuestionType, TypeMap, TableQuestion } from '../types/question'
import { questionService } from '../services/questionService'
import { Search, Refresh } from '@element-plus/icons-vue'

const props = defineProps<{
  selectedQuestions: TableQuestion[]
}>()

const emit = defineEmits<{
  (e: 'select', questions: TableQuestion[]): void
}>()

// 题型映射
const typeMap: TypeMap = {
  [QuestionType.Single]: '单选题',
  [QuestionType.Judge]: '判断题',
  [QuestionType.Fill]: '填空题',
  [QuestionType.Program]: '编程题',
  [QuestionType.ShortAnswer]: '简答题'
}

// 搜索条件
const searchForm = reactive({
  keyword: '',
  type: '',
  dateRange: [] as [Date, Date] | []
})

// 题目列表
const questions = ref<TableQuestion[]>([])
const loading = ref(false)
const currentSelection = ref<TableQuestion[]>([...props.selectedQuestions])

// 监听 props 变化
watch(() => props.selectedQuestions, (newVal) => {
  currentSelection.value = [...newVal]
}, { deep: true })

// 加载题目
const loadQuestions = async () => {
  loading.value = true
  try {
    const response = await questionService.getQuestions({
      keyword: searchForm.keyword,
      type: searchForm.type ? Number(searchForm.type) as QuestionType : undefined,
      dateRange: searchForm.dateRange.length ?
        [searchForm.dateRange[0], searchForm.dateRange[1]] : undefined
    })
    questions.value = response
    // 加载完成后设置默认选中状态
    setDefaultSelection()
  } catch (error) {
    console.error('加载题目失败:', error)
    ElMessage.error('加载题目失败')
  } finally {
    loading.value = false
  }
}

// 重置筛选条件
const resetFilter = () => {
  searchForm.keyword = ''
  searchForm.type = ''
  searchForm.dateRange = []
  loadQuestions()
}

// 处理选择变化
const handleSelectionChange = (selection: TableQuestion[]) => {
  // 确保选中的题目有正确的属性
  currentSelection.value = selection.map(q => ({
    id: q.id,
    type: q.type,
    question: q.question || q.content,
    options: q.options || [],
    answers: q.answers || [],
    analysis: q.analysis || ''
  }))
  console.log('当前选中的题目:', currentSelection.value)
}

// 确认选择
const confirmSelection = () => {
  emit('select', currentSelection.value)
}

// 初始加载
onMounted(() => {
  loadQuestions()
})

// 设置默认选中状态
const setDefaultSelection = () => {
  // 在表格渲染完成后设置默认选中状态
  setTimeout(() => {
    const table = document.querySelector('.question-selector .el-table__body');
    if (table && props.selectedQuestions.length > 0) {
      props.selectedQuestions.forEach(selected => {
        const matchingQuestion = questions.value.find(q => q.id === selected.id);
        if (matchingQuestion) {
          // 这里只是模拟选中状态，实际上需要通过 Element Plus 的 API 来实现
          currentSelection.value = [...props.selectedQuestions];
        }
      });
    }
  }, 300);
}
</script>

<template>
  <div class="question-selector">
    <!-- 搜索区域 -->
    <div class="search-area">
      <el-form :model="searchForm" inline>
        <el-form-item label="关键词">
          <el-input
            v-model="searchForm.keyword"
            placeholder="题目内容"
            clearable
            @keyup.enter="loadQuestions"
          />
        </el-form-item>
        <el-form-item label="题型">
          <el-select v-model="searchForm.type" placeholder="请选择" clearable>
            <el-option
              v-for="(label, key) in typeMap"
              :key="key"
              :label="label"
              :value="Number(key)"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="创建时间">
          <el-date-picker
            v-model="searchForm.dateRange"
            type="daterange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :icon="Search" @click="loadQuestions">搜索</el-button>
          <el-button :icon="Refresh" @click="resetFilter">重置</el-button>
        </el-form-item>
      </el-form>
    </div>

    <!-- 题目表格 -->
    <div class="question-table">
      <el-table
        :data="questions"
        style="width: 100%"
        @selection-change="handleSelectionChange"
        v-loading="loading"
        row-key="id"
      >
        <el-table-column
          type="selection"
          width="55"
          :reserve-selection="true"
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
          label="题目内容"
          min-width="300"
        >
          <template #default="scope">
            <div class="question-content">{{ scope.row.question || scope.row.content }}</div>
          </template>
        </el-table-column>
        <el-table-column
          prop="createTime"
          label="创建时间"
          width="180"
        />
      </el-table>
    </div>

    <!-- 底部按钮 -->
    <div class="selector-footer">
      <el-button @click="emit('select', [])">取消</el-button>
      <el-button type="primary" @click="confirmSelection">确认选择 ({{ currentSelection.length }})</el-button>
    </div>
  </div>
</template>

<style scoped>
.question-selector {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.search-area {
  margin-bottom: 20px;
}

.question-table {
  margin-bottom: 20px;
  max-height: 400px;
  overflow-y: auto;
}

.question-content {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 500px;
}

.selector-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 20px;
}
</style>
