// 基础API服务，未来可替换为实际HTTP请求
export const api = {
  async get<T>(endpoint: string, params?: Record<string, any>): Promise<T> {
    // 模拟网络延迟
    await new Promise(resolve => setTimeout(resolve, 300))

    // 实际项目中这里会使用fetch或axios等库进行HTTPS请求
    const response = await fetch('https://localhost:7289' + endpoint, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      ...(params && { body: JSON.stringify(params) })
    })
    return await response.json()
  },

  async post<T>(endpoint: string, data: any): Promise<T> {
    await new Promise(resolve => setTimeout(resolve, 300))
    const response = await fetch('https://localhost:7289' + endpoint, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data)
    })
    if (!response.ok) {
      throw new Error('Network response was not ok: ' + response.statusText)
    } 
    return await response.json()
  }
} 