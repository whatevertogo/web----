<script setup lang="ts">
import { ref } from 'vue'

const props = defineProps<{
  question: any
  value: string
  onChange: (value: string) => void
}>()

const answer = ref(props.value || '')

const handleChange = (event) => {
  answer.value = event.target.value
  props.onChange(answer.value)
}
</script>

<template>
  <div class="short-answer-question">
    <el-input
      type="textarea"
      v-model="answer"
      :rows="5"
      placeholder="请输入您的答案..."
      @input="handleChange"
    />
  </div>
</template>

<style scoped>
.short-answer-question {
  margin-bottom: 20px;
}
.question-text {
  margin-bottom: 10px;
}
</style> 