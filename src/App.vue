<script setup lang="ts">
import { ref, onMounted, getCurrentInstance, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useUserStore } from './stores/userStore'
import { ElMessage } from 'element-plus'
import { createApp } from 'vue'
import App from './App.vue'

const router = useRouter()
const route = useRoute()
const activeIndex = ref('1')
const userStore = useUserStore()

// 根据当前路由设置激活的菜单项
const updateActiveIndex = (path: string) => {
  switch (path) {
    case '/practice':
      activeIndex.value = 'practice'
      break
    case '/input':
      activeIndex.value = '1'
      break
    case '/manage':
      activeIndex.value = '2'
      break
    case '/export':
      activeIndex.value = '3'
      break
    default:
      activeIndex.value = ''
  }
}

onMounted(() => {
  updateActiveIndex(route.path)
})

// 监听路由变化
watch(() => route.path, (newPath) => {
  updateActiveIndex(newPath)
})

const handleSelect = (key: string) => {
  switch (key) {
    case 'practice':
      router.push('/practice')
      break
    case '1':
      router.push('/input')
      break
    case '2':
      router.push('/manage')
      break
    case '3':
      router.push('/export')
      break
  }
}

const handleCommand = (command: string) => {
  switch (command) {
    case 'logout':
      userStore.logout()
      router.push('/practice')
      break
    case 'switchRole':
      const currentPath = route.path
      userStore.login(userStore.userInfo.value.role === 'admin' ? 'user' : 'admin')
      router.push('/').then(() => {
        router.push(currentPath)
      })
      break
  }
}

// 在 App.vue 中添加全局错误处理
const instance = getCurrentInstance()
if (instance) {
  instance.appContext.app.config.errorHandler = (err, vm, info) => {
    console.error('全局错误:', err, info)
    ElMessage.error('操作出错，请刷新页面重试')
  }
}
</script>

<template>
  <div class="app-container">
    <el-container class="layout-container">
      <el-header class="app-header">
        <div class="header-content">
          <div class="logo">
            <!-- <img src="/src/assets/logo.png" alt="Logo" class="logo-image"> -->
            <span class="logo-text">题库管理系统</span>
          </div>
          <el-menu
            :default-active="activeIndex"
            class="el-menu-demo"
            mode="horizontal"
            @select="handleSelect"
            :ellipsis="false"
          >
            <el-menu-item index="practice">
              <el-icon><Reading /></el-icon>
              <span>试题练习</span>
            </el-menu-item>
            <template v-if="userStore.isAdmin()">
              <el-menu-item index="1">
                <el-icon><Edit /></el-icon>
                <span>新增试题</span>
              </el-menu-item>
              <el-menu-item index="2">
                <el-icon><List /></el-icon>
                <span>管理试题</span>
              </el-menu-item>
              <el-menu-item index="3">
                <el-icon><Download /></el-icon>
                <span>导出试题</span>
              </el-menu-item>
            </template>
          </el-menu>
          <div class="user-info">
            <el-dropdown @command="handleCommand">
              <span class="user-dropdown">
                <el-avatar :size="32" :src="userStore.userInfo.value.avatar">
                  {{ userStore.userInfo.value.username.charAt(0) }}
                </el-avatar>
                <span class="username">{{ userStore.userInfo.value.username }}</span>
              </span>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item command="switchRole">
                    <el-icon><Switch /></el-icon>切换身份
                  </el-dropdown-item>
                  <el-dropdown-item divided command="logout">
                    <el-icon><SwitchButton /></el-icon>退出登录
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>
        </div>
      </el-header>
      <el-main>
        <router-view v-slot="{ Component }">
          <keep-alive :include="['QuestionManage', 'QuestionInput', 'QuestionExport']">
            <transition name="fade" mode="out-in">
              <component :is="Component" />
            </transition>
          </keep-alive>
        </router-view>
      </el-main>
    </el-container>
  </div>
</template>

<style>
:root {
  --primary-color: #409EFF;
  --header-height: 60px;
  --menu-text-color: #303133;
  --menu-active-color: var(--primary-color);
}

.layout-container {
  height: 100vh;
}

.app-header {
  padding: 0;
  height: var(--header-height);
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  background-color: #fff;
}

.header-content {
  max-width: 1400px;
  margin: 0 auto;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
}

.logo {
  display: flex;
  align-items: center;
  margin-right: 40px;
}

.logo-image {
  height: 40px;
  margin-right: 10px;
}

.logo-text {
  font-size: 20px;
  font-weight: bold;
  color: var(--menu-text-color);
}

.el-menu-demo {
  flex: 1;
  border: none;
}

.el-menu--horizontal > .el-menu-item {
  height: var(--header-height);
  line-height: var(--header-height);
  padding: 0 20px;
}

.el-menu-item [class^="el-icon"] {
  margin-right: 5px;
  font-size: 18px;
}

.user-info {
  margin-left: 20px;
}

.user-dropdown {
  display: flex;
  align-items: center;
  cursor: pointer;
}

.username {
  margin-left: 8px;
  color: var(--menu-text-color);
}

.el-main {
  padding: 20px;
  background-color: #f5f7fa;
}

/* 页面切换动画 */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* 全局样式 */
.page-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
  background-color: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
}

.page-header {
  margin-bottom: 20px;
  padding-bottom: 20px;
  border-bottom: 1px solid #ebeef5;
}

.page-title {
  font-size: 24px;
  color: #303133;
  margin: 0;
}

/* Element Plus 组件样式优化 */
.el-card {
  border-radius: 8px;
  border: none;
  transition: box-shadow 0.3s ease;
}

.el-card:hover {
  box-shadow: 0 4px 16px rgba(0,0,0,0.1);
}

.el-button {
  border-radius: 4px;
}

.el-input {
  --el-input-hover-border-color: var(--primary-color);
}

.el-select {
  width: 100%;
}

.app-container {
  width: 100%;
  min-height: 100vh;
  padding: 0;
}
</style>
