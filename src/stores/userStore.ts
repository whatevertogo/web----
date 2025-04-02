import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { questionService } from '../services/questionService'

export type UserRole = 'admin' | 'user'

interface UserInfo {
  username: string
  role: UserRole
  avatar?: string
}

// 从本地存储加载用户信息
const loadUserInfo = (): UserInfo => {
  const stored = localStorage.getItem('userInfo')
  if (stored) {
    try {
      return JSON.parse(stored)
    } catch (e) {
      console.error('解析用户信息失败:', e)
    }
  }
  return { username: '访客', role: 'user' }
}

const storedUser = localStorage.getItem('user')
const userInfo = ref<UserInfo>(storedUser ? JSON.parse(storedUser) : {
  username: '访客',
  role: 'user'
})

// 保存用户信息到本地存储
const saveUserInfo = (info: UserInfo) => {
  localStorage.setItem('userInfo', JSON.stringify(info))
}

export const useUserStore = () => {
  const router = useRouter()

  const login = (role: UserRole) => {
    userInfo.value = {
      username: role === 'admin' ? '管理员' : '普通用户',
      role: role
    }
    localStorage.setItem('user', JSON.stringify(userInfo.value))
    questionService.initializeData(true)
  }

  const logout = () => {
    userInfo.value = {
      username: '访客',
      role: 'user'
    }
    saveUserInfo(userInfo.value)
    questionService.clearCache()
    router.push('/practice')
  }

  const isAdmin = () => userInfo.value.role === 'admin'

  const isLoggedIn = () => userInfo.value.role !== 'user'

  const updateUserInfo = (info: Partial<UserInfo>) => {
    Object.assign(userInfo.value, info)
    saveUserInfo(userInfo.value)
  }

  return {
    userInfo,
    login,
    logout,
    isAdmin,
    isLoggedIn,
    updateUserInfo
  }
} 