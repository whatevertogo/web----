<template>
  <div class="register-container">
    <el-tabs v-model="activeTab" class="register-tabs">
      <el-tab-pane label="单个注册" name="single">
        <el-card class="register-card">
          <template #header>
            <div class="card-header">
              <h2>注册学生账号</h2>
            </div>
          </template>

          <el-form
            ref="formRef"
            :model="registerForm"
            :rules="rules"
            label-position="top"
            @submit.prevent="handleRegister"
          >
            <el-form-item label="用户名" prop="username">
              <el-input
                v-model="registerForm.username"
                placeholder="请输入用户名"
                :prefix-icon="User"
              />
            </el-form-item>

            <el-form-item label="密码" prop="password">
              <el-input
                v-model="registerForm.password"
                type="password"
                placeholder="请输入密码"
                :prefix-icon="Lock"
                show-password
              />
            </el-form-item>

            <el-form-item label="确认密码" prop="confirmPassword">
              <el-input
                v-model="registerForm.confirmPassword"
                type="password"
                placeholder="请再次输入密码"
                :prefix-icon="Lock"
                show-password
              />
            </el-form-item>

            <el-form-item>
              <el-button
                type="primary"
                native-type="submit"
                :loading="loading"
                style="width: 100%"
              >
                注册
              </el-button>
            </el-form-item>
          </el-form>
        </el-card>
      </el-tab-pane>

      <el-tab-pane label="批量注册" name="batch">
        <el-card class="register-card">
          <template #header>
            <div class="card-header">
              <h2>批量注册学生账号</h2>
            </div>
          </template>

          <el-form
            ref="batchFormRef"
            :model="batchForm"
            :rules="batchRules"
            label-position="top"
            @submit.prevent="handleBatchRegister"
          >
            <el-form-item label="前缀" prop="prefix">
              <el-input
                v-model="batchForm.prefix"
                placeholder="用户名前缀，如 student"
              >
                <template #append>
                  <el-tooltip content="最终用户名将为前缀+序号，如 student1, student2" placement="top">
                    <el-icon><InfoFilled /></el-icon>
                  </el-tooltip>
                </template>
              </el-input>
            </el-form-item>

            <el-form-item label="起始序号" prop="startIndex">
              <el-input-number
                v-model="batchForm.startIndex"
                :min="1"
                :max="1000"
                style="width: 100%"
              />
            </el-form-item>

            <el-form-item label="数量" prop="count">
              <el-input-number
                v-model="batchForm.count"
                :min="1"
                :max="100"
                style="width: 100%"
              />
            </el-form-item>

            <el-form-item label="密码" prop="password">
              <el-input
                v-model="batchForm.password"
                type="password"
                placeholder="所有学生的初始密码"
                :prefix-icon="Lock"
                show-password
              />
            </el-form-item>

            <el-form-item>
              <el-button
                type="primary"
                native-type="submit"
                :loading="batchLoading"
                style="width: 100%"
              >
                批量注册
              </el-button>
            </el-form-item>
          </el-form>
        </el-card>
      </el-tab-pane>
    </el-tabs>

    <el-card v-if="registeredStudents.length > 0" class="registered-students-card">
      <template #header>
        <div class="card-header">
          <h3>最近注册的学生</h3>
          <el-button type="success" size="small" @click="exportStudentList">导出学生列表</el-button>
        </div>
      </template>

      <el-table :data="registeredStudents" style="width: 100%" row-key="username" max-height="400">
        <el-table-column prop="username" label="用户名" />
        <el-table-column prop="password" label="初始密码" />
        <el-table-column prop="time" label="注册时间" />
      </el-table>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import { User, Lock, InfoFilled } from '@element-plus/icons-vue'
import { authService } from '../services/AuthService'
import { useUserStore } from '../stores/userStore'

// 用户存储
const userStore = useUserStore()

// 激活的标签页
const activeTab = ref('single')

// 表单引用
const formRef = ref()
const batchFormRef = ref()

// 加载状态
const loading = ref(false)
const batchLoading = ref(false)

// 注册表单数据
const registerForm = reactive({
  username: '',
  password: '',
  confirmPassword: ''
})

// 批量注册表单数据
const batchForm = reactive({
  prefix: '23060',
  startIndex: 1,
  count: 10,
  password: '123456'
})

// 已注册学生列表
const registeredStudents = ref<{ username: string, password: string, time: string }[]>([])

// 表单验证规则
const rules = {
  username: [
    { required: true, message: '请输入用户名', trigger: 'blur' },
    { min: 3, max: 20, message: '用户名长度应为3-20个字符', trigger: 'blur' }
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, max: 20, message: '密码长度应为6-20个字符', trigger: 'blur' }
  ],
  confirmPassword: [
    { required: true, message: '请再次输入密码', trigger: 'blur' },
    {
      validator: (rule: any, value: string, callback: Function) => {
        if (value !== registerForm.password) {
          callback(new Error('两次输入的密码不一致'))
        } else {
          callback()
        }
      },
      trigger: 'blur'
    }
  ]
}

// 批量注册表单验证规则
const batchRules = {
  prefix: [
    { required: true, message: '请输入用户名前缀', trigger: 'blur' },
    { min: 2, max: 10, message: '前缀长度应为2-10个字符', trigger: 'blur' }
  ],
  startIndex: [
    { required: true, message: '请输入起始序号', trigger: 'blur' }
  ],
  count: [
    { required: true, message: '请输入数量', trigger: 'blur' }
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, max: 20, message: '密码长度应为6-20个字符', trigger: 'blur' }
  ]
}

// 处理单个注册
const handleRegister = async () => {
  if (!userStore.isAdmin()) {
    ElMessage.error('只有管理员可以注册学生账号')
    return
  }

  await formRef.value.validate(async (valid: boolean) => {
    if (!valid) {
      return false
    }

    loading.value = true

    try {
      // 调用注册API
      const success = await authService.register(
        registerForm.username,
        registerForm.password,
        'Student' // 固定为学生角色
      )

      if (success) {
        ElMessage.success('学生账号注册成功')

        // 添加到已注册学生列表
        registeredStudents.value.unshift({
          username: registerForm.username,
          password: registerForm.password,
          time: new Date().toLocaleString()
        })

        // 重置表单
        registerForm.username = ''
        registerForm.password = ''
        registerForm.confirmPassword = ''
        formRef.value.resetFields()
      } else {
        ElMessage.error('注册失败，请检查用户名是否已存在')
      }
    } catch (error: any) {
      ElMessage.error(error.message || '注册失败，请稍后重试')
    } finally {
      loading.value = false
    }
  })
}

// 处理批量注册
const handleBatchRegister = async () => {
  if (!userStore.isAdmin()) {
    ElMessage.error('只有管理员可以注册学生账号')
    return
  }

  await batchFormRef.value.validate(async (valid: boolean) => {
    if (!valid) {
      return false
    }

    batchLoading.value = true

    try {
      const { prefix, startIndex, count, password } = batchForm
      const successCount = 0
      const failedCount = 0
      const newStudents = []

      // 创建进度提示
      const loadingMessage = ElMessage({
        type: 'info',
        message: `正在批量注册学生账号 (0/${count})...`,
        duration: 0
      })

      // 批量注册
      for (let i = 0; i < count; i++) {
        const index = startIndex + i
        const username = `${prefix}${index}`

        try {
          // 更新进度提示
          loadingMessage.message = `正在批量注册学生账号 (${i+1}/${count})...`

          // 调用注册API
          const success = await authService.register(username, password, 'Student')

          if (success) {
            // 添加到已注册学生列表
            const student = {
              username,
              password,
              time: new Date().toLocaleString()
            }
            newStudents.push(student)
          }

          // 添加小延迟，避免请求过快
          await new Promise(resolve => setTimeout(resolve, 300))
        } catch (error) {
          console.error(`注册学生 ${username} 失败:`, error)
        }
      }

      // 关闭进度提示
      loadingMessage.close()

      // 添加到已注册学生列表
      registeredStudents.value = [...newStudents, ...registeredStudents.value]

      ElMessage.success(`批量注册完成，成功注册 ${newStudents.length} 个学生账号`)

      // 重置表单
      batchForm.startIndex = startIndex + count
    } catch (error: any) {
      ElMessage.error(error.message || '批量注册失败，请稍后重试')
    } finally {
      batchLoading.value = false
    }
  })
}

// 导出学生列表
const exportStudentList = () => {
  if (registeredStudents.value.length === 0) {
    ElMessage.warning('没有可导出的学生数据')
    return
  }

  // 创建CSV内容
  const headers = ['用户名', '密码', '注册时间']
  const rows = registeredStudents.value.map(student => [
    student.username,
    student.password,
    student.time
  ])

  // 添加标题行
  const csvContent = [
    headers.join(','),
    ...rows.map(row => row.join(','))
  ].join('\n')

  // 创建Blob对象
  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)

  // 创建下载链接
  const link = document.createElement('a')
  link.href = url
  link.setAttribute('download', `学生账号列表-${new Date().toISOString().split('T')[0]}.csv`)
  document.body.appendChild(link)

  // 模拟点击下载
  link.click()

  // 清理
  document.body.removeChild(link)
  URL.revokeObjectURL(url)

  ElMessage.success('学生列表导出成功')
}
</script>

<style scoped>
.register-container {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.register-tabs {
  width: 100%;
}

.register-card {
  width: 100%;
  margin-bottom: 0;
}

.registered-students-card {
  width: 100%;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

h3 {
  margin: 0;
  font-size: 16px;
  color: #606266;
}

/* 响应式设计 */
@media (min-width: 768px) {
  .register-container {
    padding: 30px;
  }
}
</style>
