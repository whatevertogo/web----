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
      component: () => import('../views/QuestionPractice.vue'),
      meta: { requiresAuth: true }
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
      component: () => import('../views/QuestionManage.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/export',
      name: 'QuestionExport',
      component: () => import('../views/QuestionExport.vue'),
      meta: { requiresAdmin: true }
    },
    // 暂时注释掉试卷管理相关路由
    /*
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
    */
    {
      path: '/student-register',
      name: 'StudentRegister',
      component: () => import('../views/StudentRegister.vue'),
      meta: { requiresAdmin: true }
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'NotFound',
      component: () => import('../views/NotFound.vue'),
      meta: { allowAnonymous: true }
    },
    {
      path: '/deepseek',
      name: 'DeepSeek',
      component: () => import('../views/DeepSeek.vue'),
      meta: { requiresAdmin: true }
    }
  ]
})

router.beforeEach(async (to, from, next) => {
  const userStore = useUserStore()

  // 检查是否已登录
  const isLoggedIn = authService.isLoggedIn() && userStore.isLoggedIn()

  // 如果路由需要管理员权限
  if (to.meta.requiresAdmin) {
    if (!isLoggedIn) {
      ElMessage.warning('请先登录')
      next('/login')
      return
    }
    if (!userStore.isAdmin()) {
      ElMessage.warning('您没有权限访问此页面')
      next('/practice')
      return
    }
  }

  // 如果路由需要认证
  if (to.meta.requiresAuth && !isLoggedIn) {
    ElMessage.warning('请先登录')
    next('/login')
    return
  }

  // 如果路由允许匿名访问
  if (to.meta.allowAnonymous) {
    // 如果已登录且尝试访问登录页，重定向到首页
    if (to.path === '/login' && isLoggedIn) {
      if (userStore.isAdmin()) {
        next('/input')
      } else {
        next('/practice')
      }
      return
    }
    next()
    return
  }

  next()
})

router.afterEach((to, from) => {
  // Empty implementation, kept for future use
})

export default router
