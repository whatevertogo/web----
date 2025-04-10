import { createApp } from 'vue'
import ElementPlus from 'element-plus'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import { createPinia } from 'pinia'
import 'element-plus/dist/index.css'
import router from './router'
import App from './App.vue'
import './style.css'
import { questionService } from './services/questionService'

const app = createApp(App)

// 全局错误处理
app.config.errorHandler = (err, vm, info) => {
  console.error('全局错误:', err)
  console.error('错误信息:', info)
}

// 性能监控
if (process.env.NODE_ENV === 'production') {
  // 初始化性能监控
}

// 注册所有Element Plus图标
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

app.use(createPinia())  
app.use(ElementPlus)
app.use(router)

app.mount('#app')
