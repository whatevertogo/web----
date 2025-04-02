import { createRouter, createWebHistory } from 'vue-router'
import { useUserStore } from '../stores/userStore'
import { ElMessage } from 'element-plus'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      redirect: '/practice'
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
      path: '/:pathMatch(.*)*',
      name: 'NotFound',
      component: () => import('../views/NotFound.vue')
    }
  ]
})

router.beforeEach((to, from, next) => {
  const userStore = useUserStore()
  
  if (to.meta.requiresAdmin && !userStore.isAdmin()) {
    ElMessage.warning('需要管理员权限')
    next('/practice')
    return
  }

  if (to.meta.requiresAuth && !userStore.isLoggedIn()) {
    ElMessage.warning('请先登录')
    next('/login')
    return
  }

  next()
})

router.afterEach((to, from) => {
  if (from.name === 'QuestionManage' && to.name !== 'QuestionManage') {
    const { questionService } = require('../services/questionService')
    questionService.initializeData(true).catch(err => {
      console.error('初始化数据失败:', err)
    })
  }
})

export default router