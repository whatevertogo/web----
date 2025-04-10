import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { questionService } from '../services/questionService'

export type UserRole = 'admin' | 'user'
export type UserPermission = 'read' | 'create' | 'update' | 'delete'

interface UserInfo {
  username: string
  role: UserRole
  permissions?: UserPermission[]
  avatar?: string
}

// 角色权限映射
const rolePermissions: Record<UserRole, UserPermission[]> = {
  admin: ['read', 'create', 'update', 'delete'],
  user: ['read']
}

// 从本地存储加载用户信息
const loadUserInfo = (): UserInfo => {
  const stored = localStorage.getItem('userInfo')
  if (stored) {
    try {
      const info = JSON.parse(stored)
      // 确保角色有效
      if (!['admin', 'user'].includes(info.role)) {
        info.role = 'user'
      }
      // 根据角色设置权限
      info.permissions = rolePermissions[info.role]
      return info
    } catch (e) {
      console.error('解析用户信息失败:', e)
    }
  }
  return { 
    username: '访客', 
    role: 'user',
    permissions: rolePermissions.user
  }
}

// 保存用户信息到本地存储
const saveUserInfo = (info: UserInfo) => {
  localStorage.setItem('userInfo', JSON.stringify({
    ...info,
    permissions: rolePermissions[info.role] // 确保权限与角色匹配
  }))
}

const userInfo = ref<UserInfo>(loadUserInfo())

export const useUserStore = () => {
  const router = useRouter()

  const login = (role: UserRole) => {
    userInfo.value = {
      username: role === 'admin' ? '管理员' : '普通用户',
      role: role,
      permissions: rolePermissions[role]
    }
    saveUserInfo(userInfo.value)
  }

  const logout = () => {
    userInfo.value = {
      username: '访客',
      role: 'user',
      permissions: rolePermissions.user
    }
    saveUserInfo(userInfo.value)
    router.push('/practice')
  }

  const hasPermission = (permission: UserPermission): boolean => {
    return userInfo.value.permissions?.includes(permission) ?? false
  }

  const isAdmin = () => userInfo.value.role === 'admin'

  const isLoggedIn = () => userInfo.value.role !== 'user'

  const updateUserInfo = (info: Partial<UserInfo>) => {
    Object.assign(userInfo.value, info)
    // 如果更新了角色，同时更新权限
    if (info.role) {
      userInfo.value.permissions = rolePermissions[info.role]
    }
    saveUserInfo(userInfo.value)
  }

  return {
    userInfo,
    login,
    logout,
    isAdmin,
    isLoggedIn,
    updateUserInfo,
    hasPermission
  }
}