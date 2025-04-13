<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useUserStore } from '../stores/userStore'
import { User, Lock } from '@element-plus/icons-vue'

const router = useRouter()
const route = useRoute()
const userStore = useUserStore()

// 登录表单数据
const loginForm = reactive({
  username: '',
  password: '',
  rememberMe: false
})

// 表单验证规则
const rules = {
  username: [
    { required: true, message: '请输入用户名', trigger: 'blur' },
    { min: 3, max: 20, message: '用户名长度应为3-20个字符', trigger: 'blur' }
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, max: 20, message: '密码长度应为6-20个字符', trigger: 'blur' }
  ]
}

const loading = ref(false)
const formRef = ref()

// 处理登录
const handleLogin = async () => {
  if (!formRef.value) return

  await formRef.value.validate(async (valid: boolean) => {
    if (!valid) {
      return false
    }

    loading.value = true

    try {
      // 调用登录API
      const success = await userStore.login(loginForm.username, loginForm.password)

      if (success) {
        ElMessage.success('登录成功')

        // 如果选择了"记住我"，则保存登录状态
        if (loginForm.rememberMe) {
          localStorage.setItem('remember_me', 'true')
          localStorage.setItem('last_username', loginForm.username)
        } else {
          localStorage.removeItem('remember_me')
          localStorage.removeItem('last_username')
        }

        // 登录成功后跳转到首页或重定向页面
        const redirectPath = route.query.redirect as string || '/'
        router.push(redirectPath)
      } else {
        ElMessage.error('登录失败，请检查用户名和密码')
      }
    } catch (error: any) {
      ElMessage.error(error.message || '登录失败，请检查用户名和密码')
    } finally {
      loading.value = false
    }
  })
}

// 检查是否有记住的登录状态
const checkRememberedLogin = () => {
  const remembered = localStorage.getItem('remember_me')
  if (remembered === 'true') {
    loginForm.rememberMe = true
    // 这里可以从localStorage加载上次的用户名
    const savedUsername = localStorage.getItem('last_username')
    if (savedUsername) {
      loginForm.username = savedUsername
    }
  }
}

// 组件挂载时检查记住的登录状态
checkRememberedLogin()
</script>

<template>
  <div class="login-container">
    <div class="login-box">
      <div class="login-header">
        <h2>题库管理系统</h2>
        <p>请登录您的账号</p>
      </div>

      <el-form ref="formRef" :model="loginForm" :rules="rules" label-position="top" class="login-form">
        <el-form-item label="用户名" prop="username">
          <el-input v-model="loginForm.username" placeholder="请输入用户名" :prefix-icon="User" @keyup.enter="handleLogin" />
        </el-form-item>

        <el-form-item label="密码" prop="password">
          <el-input v-model="loginForm.password" type="password" placeholder="请输入密码" :prefix-icon="Lock" show-password
            @keyup.enter="handleLogin" />
        </el-form-item>

        <el-form-item>
          <div class="remember-forgot">
            <el-checkbox v-model="loginForm.rememberMe">记住我</el-checkbox>
          </div>
        </el-form-item>

        <el-form-item>
          <el-button type="primary" :loading="loading" class="login-button" @click="handleLogin">
            登录
          </el-button>
        </el-form-item>
      </el-form>
    </div>
  </div>
</template>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: flex-center;
  padding-top: 125px;/* 添加适当的上边距 */
  min-height: 40vh;
  background-color: #f5f7fa;
}

.login-box {
  width: 400px;
  padding: 30px;
  background-color: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
}

.login-header {
  text-align: center;
  margin-bottom: 30px;
}

.login-header h2 {
  font-size: 24px;
  color: var(--el-color-primary);
  margin-bottom: 10px;
}

.login-header p {
  color: #606266;
  font-size: 14px;
}

.login-form {
  margin-bottom: 20px;
}

.remember-forgot {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.login-tips {
  color: #909399;
  font-size: 12px;
}

.login-button {
  width: 100%;
}
</style>
