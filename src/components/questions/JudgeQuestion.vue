<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{
  question: any
  value: string
  onChange: (value: string) => void
}>()

const selectedAnswer = ref(props.value)

watch(() => props.value, (newValue) => {
  selectedAnswer.value = newValue
})

const handleChange = (value: string) => {
  selectedAnswer.value = value
  props.onChange(value)
}
</script>

<template>
  <div class="judge-question">
    <el-radio-group v-model="selectedAnswer" @change="handleChange">
      <el-radio label="true">正确</el-radio>
      <el-radio label="false">错误</el-radio>
    </el-radio-group>
  </div>
</template>

<style scoped>
.judge-question {
  margin-bottom: 20px;
}
.question-text {
  margin-bottom: 10px;
}
</style> 