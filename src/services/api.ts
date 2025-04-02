// 基础API服务，未来可替换为实际HTTP请求
export const api = {
  async get<T>(endpoint: string, params?: Record<string, any>): Promise<T> {
    // 模拟网络延迟
    await new Promise(resolve => setTimeout(resolve, 300))
    
    // 实际项目中这里会使用fetch或axios发送请求
    // 现在使用localStorage模拟
    const data = localStorage.getItem(endpoint)
    return data ? JSON.parse(data) : null
  },
  
  async post<T>(endpoint: string, data: any): Promise<T> {
    await new Promise(resolve => setTimeout(resolve, 300))
    localStorage.setItem(endpoint, JSON.stringify(data))
    return data
  }
} 