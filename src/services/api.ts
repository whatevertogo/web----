interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  error?: string;
}

import config from '../config/index'

// 基础API服务，实现前后端分离的HTTP请求
export const api = {
  async get<T>(endpoint: string, params?: Record<string, any>): Promise<T> {
    try {
      await new Promise(resolve => setTimeout(resolve, 300))
      const queryParams = params ? new URLSearchParams(params).toString() : '';
      const url = queryParams ? `${config.apiBaseUrl}${endpoint}?${queryParams}` : `${config.apiBaseUrl}${endpoint}`;

      // 准备请求头
      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
      };

      // 添加认证令牌
      const token = localStorage.getItem('auth_token');
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }

      const response = await fetch(url, {
        method: 'GET',
        headers
      });

      // 204 No Content
      if (response.status === 204) {
        return {} as T
      }

      // 检查响应是否为空
      const text = await response.text();
      console.log('原始响应文本:', text);
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
      console.error('GET请求失败:', error);
      throw new Error(error.message || '网络请求失败');
    }
  },

  async post<T>(endpoint: string, data: any): Promise<T> {
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
        method: 'POST',
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
      console.error('POST请求失败:', error)
      throw new Error(error.message || '网络请求失败')
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