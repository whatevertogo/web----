<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{
  question: any
  answers: string[]
  onAnswerChange: (index: number) => void
}>()

const emit = defineEmits(['update:answer'])

const handleInput = (index: number) => {
  if (props.onAnswerChange) {
    props.onAnswerChange(index)
  }
  emit('update:answer', index)
}
</script>

<template>
  <div v-for="(_, index) in question.answers" :key="index" class="fill-input">
    <span class="fill-label">第 {{ index + 1 }} 空：</span>
    <el-input 
      v-model="answers[index]"
      type="text"
      placeholder="请输入答案"
      @change="handleInput(index)"
    />
  </div>
</template>

<style scoped>
.fill-input {
  margin-bottom: 15px;
}

.fill-label {
  display: inline-block;
  width: 80px;
  margin-right: 10px;
}
</style> 