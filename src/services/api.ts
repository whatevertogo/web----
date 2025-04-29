interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  error?: string;
}

import config from '@/config'

// 基础API服务，实现前后端分离的HTTP请求
export const api = {
  async get<T>(endpoint: string, params?: Record<string, any>): Promise<T> {
    try {
      // todo-可以加人为延迟(滑稽)
      // await new Promise(resolve => setTimeout(resolve, 300)); // 等待300毫秒，模拟网络延迟
      //1.构建完整的URL
      const queryParams = params ? new URLSearchParams(params).toString() : '';
      const url = queryParams ? `${config.apiBaseUrl}${endpoint}?${queryParams}` : `${config.apiBaseUrl}${endpoint}`;

      //2. 准备请求头
      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
      };

      //3. 添加认证令牌
      const token = localStorage.getItem('auth_token');
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }

      //4. 发送fetch请求
      const response = await fetch(url, {
        method: 'GET',
        headers
      });

      //5.如果没有数据，处理特定的204状态码204 No Content
      if (response.status === 204) {
        return {} as T
      }

      //6. 检查响应是否为空
      const text = await response.text();
      console.log('原始响应文本:', text);
      if (!text || text.trim() === '') {
        console.warn('API返回空响应:', endpoint);
        return {} as T;
      }

      //7. 尝试解析JSON
      let result: any;
      try {
        result = JSON.parse(text);
      } catch (parseError) {
        console.error('JSON解析失败:', text, parseError);
        throw new Error('服务器返回的数据格式无效');
      }

      //8. 处理不同的响应格式
      if (Array.isArray(result)) {
        // 如果直接返回数组，直接使用
        return result as T;
      } else if (result && typeof result === 'object') {
        // 如果是标准的API响应格式
        if ('success' in result) {
          if (!result.success) {
            throw new Error(result.message || result.error || '操作失败');
          }
          return result.data as T;
        }
        // 如果是其他对象格式，直接返回
        return result as T;
      }

      //9.处理非成功的HTTP状态码
      if (!response.ok) {
        throw new Error(`请求失败(${response.status})`);
      }

      //10.未知格式处理
      //如果代码走到这里说明响应格式无法识别
      throw new Error('无法识别的响应格式');

    } catch (error: any) {
      //11.统一错误处理
      console.error('GET请求失败:', error);
      //抛出更具体的错误信息
      throw new Error(error.message || '网络请求失败');
    }
  },

  async post<T>(endpoint: string, data: any): Promise<T> {
    try {
      //错误捕捉
      //人为延迟(滑稽)
      // await new Promise(resolve => setTimeout(resolve, 300))

      //1. 准备请求头
      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
      };

      //2.添加认证令牌
      const token = localStorage.getItem('auth_token');
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }

      //3. 发送fetch请求
      let response;
      try {
        response = await fetch(config.apiBaseUrl + endpoint, { // 拼接完整的 API URL
          method: 'POST', // 指定请求方法为 POST
          headers, // 将准备好的请求头对象放进去
          // **核心：将要发送的 JavaScript 数据 (data) 转换为 JSON 字符串**
          body: JSON.stringify(data),
          // 'include' 表示允许请求携带凭据（例如 Cookies），
          // 这需要后端 CORS 配置中也允许 AllowCredentials()
          credentials: 'include'
        });
      } catch (fetchError: any) { // 捕获 fetch 本身可能发生的错误 (如网络不通、DNS 解析失败等)
        console.error('网络请求错误:', fetchError);
        // 抛出一个更具体的网络错误信息
        throw new Error(`无法连接到服务器(${config.apiBaseUrl})，请检查网络连接或服务器状态`);
      }

      //4. 检查HTTP响应状态码
      if (!response.ok) {
        const errorText = await response.text();
        console.error('服务器响应错误:', response.status, errorText);
        throw new Error(`请求失败(${response.status}): ${errorText}`);
      }

      //5. 尝试解析响应体
      const text = await response.text();
      // console.log('原始响应文本:', text);//调试用，打印原始响应文本

      if (!text || text.trim() === '') {
        // 如果服务器成功响应 (2xx) 但没有返回任何内容
        return {} as T; // 返回一个空对象，并断言其类型为 T
      }

      // 9. 解析 JSON 响应体
      try {
        const result = JSON.parse(text); // 尝试将响应文本解析为 JavaScript 对象/数组
        // **假设** POST 请求成功后，后端直接返回所需的数据 (类型为 T)
        // 注意：这里没有像 get 方法那样检查 'success' 字段，
        // 它假设成功的 POST 请求直接返回数据。如果你的后端 POST 也返回 ApiResponse 结构，
        // 你需要加入类似 get 方法中的判断逻辑。
        return result as T;
      } catch (parseError) { // 如果解析失败 (说明返回的不是有效的 JSON)
        console.error('JSON解析失败:', text, parseError);
        throw new Error('服务器返回的数据格式无效'); // 抛出格式错误
      }

    } catch (error: any) { // 10. 捕获所有错误
      // 捕获上面任何步骤中抛出的错误 (网络错误、状态码错误、解析错误等)
      console.error('POST请求失败:', error); // 打印最终的错误信息
      // **重新抛出错误**：这很重要，让调用 api.post 的地方也能知道发生了错误，并进行处理 (例如给用户提示)
      throw error;
    }
  },

  async put<T>(endpoint: string, data: any): Promise<T> {
    try {
      await new Promise(resolve => setTimeout(resolve, 300))

      // 准备请求头
      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
      };

      // 添加认证令牌
      const token = localStorage.getItem('auth_token');
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }

      const response = await fetch(config.apiBaseUrl + endpoint, {
        method: 'PUT',
        headers,
        body: JSON.stringify(data)
      })

      // 204 No Content
      if (response.status === 204) {
        return {} as T
      }

      // 检查响应是否为空
      const text = await response.text();
      if (!text || text.trim() === '') {
        console.warn('API返回空响应:', endpoint);
        return {} as T;
      }

      // 尝试解析JSON
      let result: any;
      try {
        result = JSON.parse(text);
      } catch (parseError) {
        console.error('JSON解析失败:', text, parseError);
        throw new Error('服务器返回的数据格式无效');
      }

      // 处理不同的响应格式
      if (Array.isArray(result)) {
        // 如果直接返回数组，直接使用
        return result as T;
      } else if (result && typeof result === 'object') {
        // 如果是标准的API响应格式
        if ('success' in result) {
          if (!result.success) {
            throw new Error(result.message || result.error || '操作失败');
          }
          return result.data as T;
        }
        // 如果是其他对象格式，直接返回
        return result as T;
      }

      if (!response.ok) {
        throw new Error(`请求失败(${response.status})`);
      }

      throw new Error('无法识别的响应格式');
    } catch (error: any) {
      console.error('PUT请求失败:', error)
      throw new Error(error.message || '网络请求失败')
    }
  },
  async delete<T>(endpoint: string): Promise<T> {
    try {
      await new Promise(resolve => setTimeout(resolve, 300))

      // 准备请求头
      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
      };

      // 添加认证令牌
      const token = localStorage.getItem('auth_token');
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }

      const response = await fetch(config.apiBaseUrl + endpoint, {
        method: 'DELETE',
        headers
      })

      // 204 No Content
      if (response.status === 204) {
        return {} as T
      }

      // 检查响应是否为空
      const text = await response.text();
      if (!text || text.trim() === '') {
        console.warn('API返回空响应:', endpoint);
        return {} as T;
      }

      // 尝试解析JSON
      let result: any;
      try {
        result = JSON.parse(text);
      } catch (parseError) {
        console.error('JSON解析失败:', text, parseError);
        throw new Error('服务器返回的数据格式无效');
      }

      // 处理不同的响应格式
      if (Array.isArray(result)) {
        // 如果直接返回数组，直接使用
        return result as T;
      } else if (result && typeof result === 'object') {
        // 如果是标准的API响应格式
        if ('success' in result) {
          if (!result.success) {
            throw new Error(result.message || result.error || '操作失败');
          }
          return result.data as T;
        }
        // 如果是其他对象格式，直接返回
        return result as T;
      }

      if (!response.ok) {
        throw new Error(`请求失败(${response.status})`);
      }

      throw new Error('无法识别的响应格式');
    } catch (error: any) {
      console.error('DELETE请求失败:', error)
      throw new Error(error.message || '网络请求失败')
    }
  }
}