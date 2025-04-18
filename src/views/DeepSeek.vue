<template>
  <div class="deepseek-container">
    <el-card class="chat-card">
      <template #header>
        <div class="card-header">
          <h2>DeepSeek AI 助手</h2>
          <el-select v-model="selectedModel" placeholder="选择模型" size="small">
            <el-option label="DeepSeek Chat" value="deepseek-chat" />
            <el-option label="DeepSeek Coder" value="deepseek-coder" />
          </el-select>
        </div>
      </template>

      <div class="chat-content" ref="chatContentRef">
        <div v-if="messages.length === 0" class="empty-chat">
          <el-empty description="开始与 AI 助手对话">
            <template #image>
              <img src="https://deepseek.com/favicon.ico" alt="DeepSeek Logo" class="deepseek-logo" />
            </template>
            <template #description>
              <p>您可以向 DeepSeek AI 助手提问任何问题</p>
            </template>
          </el-empty>
        </div>

        <div v-else class="message-list">
          <div v-for="(message, index) in messages" :key="index" :class="['message', message.role]">
            <div class="message-avatar">
              <el-avatar :size="40" :src="message.role === 'user' ? userAvatar : aiAvatar">
                {{ message.role === 'user' ? 'U' : 'AI' }}
              </el-avatar>
            </div>
            <div class="message-content">
              <div class="message-header">
                <span class="message-sender">{{ message.role === 'user' ? '用户' : 'DeepSeek AI' }}</span>
                <span class="message-time">{{ message.time }}</span>
              </div>
              <div class="message-text" v-html="formatMessage(message.content)"></div>
            </div>
          </div>
        </div>

        <div v-if="loading" class="loading-indicator">
          <el-skeleton :rows="3" animated />
        </div>
      </div>

      <div class="chat-input">
        <el-input
          v-model="userInput"
          type="textarea"
          :rows="3"
          placeholder="输入您的问题..."
          resize="none"
          @keydown.enter.exact.prevent="sendMessage"
        />
        <div class="input-actions">
          <el-button type="primary" :disabled="!userInput.trim() || loading" @click="sendMessage">
            <el-icon v-if="loading"><Loading /></el-icon>
            <span>{{ loading ? '请稍候...' : '发送' }}</span>
          </el-button>
          <el-button @click="clearChat" :disabled="messages.length === 0 || loading">清空对话</el-button>
        </div>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, nextTick, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { Loading } from '@element-plus/icons-vue'
import { deepSeekService } from '../services/deepSeekService'
import { marked } from 'marked'
import DOMPurify from 'dompurify'
import hljs from 'highlight.js'
import 'highlight.js/styles/github.css'

// 配置 marked
marked.setOptions({
  // @ts-ignore - highlight 属性在类型定义中不存在，但实际上是有效的
  highlight: function(code: string, lang: string) {
    const language = hljs.getLanguage(lang) ? lang : 'plaintext';
    return hljs.highlight(code, { language }).value;
  },
  langPrefix: 'hljs language-',
  gfm: true,
  breaks: true
});

// 用户头像和AI头像
const userAvatar = ref('')
const aiAvatar = ref('https://deepseek.com/favicon.ico')

// 聊天内容区域引用
const chatContentRef = ref<HTMLElement | null>(null)

// 用户输入
const userInput = ref('')

// 加载状态
const loading = ref(false)

// 选择的模型
const selectedModel = ref('deepseek-chat')

// 消息列表
interface Message {
  role: 'user' | 'assistant'
  content: string
  time: string
}

const messages = reactive<Message[]>([])

// 格式化消息内容（将Markdown转换为HTML）
const formatMessage = (content: string): string => {
  // 使用marked将Markdown转换为HTML
  // 使用类型断言确保返回的是字符串
  const html = marked(content) as string
  // 使用DOMPurify清理HTML，防止XSS攻击
  return DOMPurify.sanitize(html)
}

// 发送消息
const sendMessage = async () => {
  const message = userInput.value.trim()
  if (!message || loading.value) return

  // 添加用户消息
  messages.push({
    role: 'user',
    content: message,
    time: new Date().toLocaleTimeString()
  })

  // 清空输入框
  userInput.value = ''

  // 滚动到底部
  await nextTick()
  scrollToBottom()

  // 设置加载状态
  loading.value = true

  try {
    // 调用DeepSeek服务
    const response = await deepSeekService.chat(message, selectedModel.value)

    // 添加AI回复
    messages.push({
      role: 'assistant',
      content: response.reply,
      time: new Date().toLocaleTimeString()
    })
  } catch (error: any) {
    console.error('发送消息失败:', error)
    ElMessage.error(`发送消息失败: ${error.message || '未知错误'}`)

    // 添加错误消息
    messages.push({
      role: 'assistant',
      content: '抱歉，我遇到了一些问题，无法回复您的消息。请稍后再试。',
      time: new Date().toLocaleTimeString()
    })
  } finally {
    // 关闭加载状态
    loading.value = false

    // 滚动到底部
    await nextTick()
    scrollToBottom()
  }
}

// 清空聊天记录
const clearChat = () => {
  messages.length = 0
}

// 滚动到底部
const scrollToBottom = () => {
  if (chatContentRef.value) {
    chatContentRef.value.scrollTop = chatContentRef.value.scrollHeight
  }
}

// 监听消息变化，自动滚动到底部
watch(
  () => messages.length,
  () => {
    nextTick(() => {
      scrollToBottom()
    })
  }
)

// 组件挂载后滚动到底部
onMounted(() => {
  scrollToBottom()
})
</script>

<style scoped>
.deepseek-container {
  max-width: 1000px;
  margin: 0 auto;
  padding: 20px;
}

.chat-card {
  width: 100%;
  height: calc(100vh - 120px);
  display: flex;
  flex-direction: column;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-header h2 {
  margin: 0;
  font-size: 20px;
  color: #303133;
}

.chat-content {
  flex: 1;
  overflow-y: auto;
  padding: 10px;
  background-color: #f9f9f9;
  border-radius: 4px;
  margin-bottom: 15px;
}

.empty-chat {
  height: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
}

.deepseek-logo {
  width: 80px;
  height: 80px;
}

.message-list {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.message {
  display: flex;
  gap: 10px;
  padding: 10px;
  border-radius: 8px;
}

.message.user {
  background-color: #e8f4ff;
}

.message.assistant {
  background-color: #f0f9eb;
}

.message-avatar {
  flex-shrink: 0;
}

.message-content {
  flex: 1;
}

.message-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 5px;
}

.message-sender {
  font-weight: bold;
  color: #303133;
}

.message-time {
  font-size: 12px;
  color: #909399;
}

.message-text {
  line-height: 1.5;
  white-space: pre-wrap;
  word-break: break-word;
}

.loading-indicator {
  padding: 10px;
  background-color: #f0f9eb;
  border-radius: 8px;
  margin-top: 15px;
}

.chat-input {
  margin-top: auto;
}

.input-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 10px;
}

/* 全局样式，用于Markdown渲染 */
:deep(pre) {
  background-color: #f6f8fa;
  border-radius: 6px;
  padding: 16px;
  overflow: auto;
}

:deep(code) {
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
  font-size: 14px;
}

:deep(p) {
  margin: 8px 0;
}

:deep(ul), :deep(ol) {
  padding-left: 20px;
}

:deep(blockquote) {
  border-left: 4px solid #dfe2e5;
  padding-left: 16px;
  margin-left: 0;
  color: #6a737d;
}

:deep(table) {
  border-collapse: collapse;
  width: 100%;
  margin: 12px 0;
}

:deep(th), :deep(td) {
  border: 1px solid #dfe2e5;
  padding: 8px 12px;
  text-align: left;
}

:deep(th) {
  background-color: #f6f8fa;
}
</style>