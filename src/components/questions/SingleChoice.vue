<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{
  question: any
  value: string
  onChange: (value: string) => void
}>()

const answer = ref(props.value)

// 监听 value 变化
watch(() => props.value, (newValue) => {
  answer.value = newValue
})

// 监听 question 变化，确保切换题目时重置答案
watch(() => props.question, () => {
  answer.value = props.value
})

const handleChange = (value: string) => {
  answer.value = value
  props.onChange(value)
}
</script>

<template>
  <div class="single-choice">
    <el-radio-group
      v-model="answer"
      @change="handleChange"
    >
      <el-radio
        v-for="(option, index) in question.options"
        :key="index"
        :label="String.fromCharCode(65 + index)"
      >
        {{ String.fromCharCode(65 + index) }}. {{ option }}
      </el-radio>
    </el-radio-group>
  </div>
</template>