import { api } from './api'

// DeepSeek 服务接口
export interface DeepSeekResponse {
  reply: string
}

// DeepSeek 服务
export const deepSeekService = {
  /**
   * 发送消息到 DeepSeek AI
   * @param message 用户消息
   * @param model 模型名称
   * @returns AI 回复
   */
  async chat(message: string, model: string = 'deepseek-chat'): Promise<DeepSeekResponse> {
    try {
      console.log(`发送消息到 DeepSeek AI，模型: ${model}，消息: ${message.substring(0, 50)}${message.length > 50 ? '...' : ''}`)

      try {
        // 尝试调用后端 API
        const response = await api.post<DeepSeekResponse>('/deepseek/chat', {
          message,
          model
        })

        console.log('DeepSeek AI 响应:', response)

        // 如果响应格式不正确，抛出错误
        if (!response || typeof response.reply !== 'string') {
          console.error('无效的 DeepSeek 响应格式:', response)
          throw new Error('无效的响应格式')
        }

        return response
      } catch (apiError: any) {
        // 如果是 401 未授权错误，返回模拟响应
        if (apiError.message && apiError.message.includes('401')) {
          console.warn('未授权错误，返回模拟响应')
          return {
            reply: `这是一个模拟响应。您发送的消息是: "${message}"。由于授权问题，无法连接到后端服务。`
          }
        }

        // 其他错误继续抛出
        throw apiError
      }
    } catch (error: any) {
      console.error('DeepSeek 聊天请求失败:', error)
      throw error
    }
  }
}
