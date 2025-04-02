<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{
  question: any
  value: string
  onChange: (value: string) => void
}>()

const answer = ref(props.value || '')

watch(() => props.value, (newValue) => {
  answer.value = newValue || ''
})

// 实时更新答案
const handleInput = (val) => {
  props.onChange(val)
}
</script>

<template>
  <div class="program-question">
    <div class="sample-block">
      <h4>示例输入:</h4>
      <pre>{{ question.sampleInput }}</pre>
    </div>
    <div class="sample-block">
      <h4>示例输出:</h4>
      <pre>{{ question.sampleOutput }}</pre>
    </div>
    <el-input
      v-model="answer"
      type="textarea"
      rows="6"
      placeholder="请输入你的代码..."
      @input="handleInput"
    />
  </div>
</template>

<style scoped>
.program-question {
  margin-bottom: 20px;
}
.sample-block {
  background: #f5f7fa;
  padding: 10px;
  border-radius: 4px;
  margin-bottom: 15px;
}
pre {
  white-space: pre-wrap;
  margin: 0;
}
</style> 