import { createRouter, createWebHistory } from 'vue-router'
import { useUserStore } from '../stores/userStore'
import { ElMessage } from 'element-plus'
import { authService } from '../services/AuthService'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      redirect: '/practice'
    },
    {
      path: '/login',
      name: 'Login',
      component: () => import('../views/Login.vue'),
      meta: { allowAnonymous: true }
    },
    {
      path: '/practice',
      name: 'QuestionPractice',
      component: () => import('../views/QuestionPractice.vue')
    },
    {
      path: '/input',
      name: 'QuestionInput',
      component: () => import('../views/QuestionInput.vue'),
      meta: { requiresAdmin: true }
    },
    {
      path: '/manage',
      name: 'QuestionManage',
      component: () => import('../views/QuestionManage.vue')
    },
    {
      path: '/export',
      name: 'QuestionExport',
      component: () => import('../views/QuestionExport.vue'),
      meta: { requiresAdmin: true }
    },
    {
      path: '/exam-manage',
      name: 'ExamManage',
      component: () => import('../views/ExamManage.vue'),
      meta: { requiresAdmin: true }
    },
    {
      path: '/exam-taking',
      name: 'ExamTaking',
      component: () => import('../views/ExamTaking.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/exam-taking/:id',
      name: 'ExamTakingDetail',
      component: () => import('../views/ExamTaking.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'NotFound',
      component: () => import('../views/NotFound.vue')
    }
  ]
})

router.beforeEach(async (to, from, next) => {
  const userStore = useUserStore()

  // 检查是否已登录
  const isLoggedIn = authService.isLoggedIn() && userStore.isLoggedIn()

  // 如果路由允许匿名访问，直接通过
  if (to.meta.allowAnonymous) {
    // 如果已登录且尝试访问登录页，重定向到首页
    if (to.path === '/login' && isLoggedIn) {
      next('/')
      return
    }
    next()
    return
  }

  // 如果未登录，重定向到登录页
  if (!isLoggedIn) {
    // 尝试初始化用户状态
    const initialized = await userStore.initUserState()

    if (!initialized) {
      ElMessage.warning('请先登录')
      next({ path: '/login', query: { redirect: to.fullPath } })
      return
    }
  }

  // 检查管理员权限
  if (to.meta.requiresAdmin && !userStore.isAdmin()) {
    ElMessage.warning('需要管理员权限')
    next('/practice')
    return
  }

  // 通过所有检查，允许访问
  next()
})

router.afterEach((to, from) => {
  // Empty implementation, kept for future use
})

export default router
