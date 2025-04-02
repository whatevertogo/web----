<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { questionService } from '../services/questionService'
import { QuestionType } from '../types/question'
import * as echarts from 'echarts'

const typeStats = ref({})
const dateStats = ref([])
const chartInstance = ref<echarts.ECharts | null>(null)

// 获取统计数据
const loadStats = async () => {
  const questions = await questionService.getQuestions()
  
  // 按题型统计
  const typeCounts = {}
  questions.forEach(q => {
    typeCounts[q.type] = (typeCounts[q.type] || 0) + 1
  })
  typeStats.value = typeCounts
  
  // 按日期统计
  // ...其他统计逻辑
}

onMounted(() => {
  loadStats()
  initChart()
})

const initChart = () => {
  const chartDom = document.getElementById('stats-chart')
  if (chartDom) {
    chartInstance.value = echarts.init(chartDom)
    updateChart()
  }
}

const updateChart = () => {
  if (!chartInstance.value) return
  
  // 配置图表
  const option = {
    // 饼图配置...
  }
  
  chartInstance.value.setOption(option)
}
</script>

<template>
  <div class="statistics page-container">
    <div class="page-header">
      <h1 class="page-title">题库统计</h1>
    </div>
    
    <el-row :gutter="20">
      <el-col :span="12">
        <el-card>
          <template #header>
            <div>题型分布</div>
          </template>
          <div id="stats-chart" style="height: 300px;"></div>
        </el-card>
      </el-col>
      <!-- 其他统计卡片 -->
    </el-row>
  </div>
</template> 