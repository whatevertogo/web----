<script setup lang="ts">
import { ref } from 'vue'

const props = defineProps<{
  question: any
  value: string
  onChange: (value: string) => void
}>()

const answer = ref(props.value)

const handleChange = (value: string) => {
  answer.value = value
  props.onChange(value)
}
</script>

<template>
  <el-radio-group 
    v-model="answer"
    @change="handleChange"
  >
    <el-radio label="true">正确</el-radio>
    <el-radio label="false">错误</el-radio>
  </el-radio-group>
</template> 