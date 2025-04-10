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
      const queryParams = params ? new URLSearchParams(params).toString() : '';
      const url = queryParams ? `/api${endpoint}?${queryParams}` : `/api${endpoint}`;
      const response = await fetch(url, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        }
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result = await response.json();      if (!result.success) {
        throw new Error(result.message || '请求失败');
      }
      return result;
    } catch (error) {
      console.error('API request failed:', error);
      throw new Error('请求失败: ' + (error as Error).message);
    }
  },
  
  async post<T>(endpoint: string, data: any): Promise<T> {
    try {
      await new Promise(resolve => setTimeout(resolve, 300))
      const response = await fetch(config.apiBaseUrl + endpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data)
      })

      // 204 No Content
      if (response.status === 204) {
        return {} as T
      }

      const result: ApiResponse<T> = await response.json()

      if (!response.ok) {
        throw new Error(result.message || result.error || `请求失败(${response.status})`)
      }

      if (!result.success) {
        throw new Error(result.message || result.error || '操作失败')
      }

      return result.data as T
    } catch (error: any) {
      console.error('POST请求失败:', error)
      throw new Error(error.message || '网络请求失败')
    }
  },
  async put<T>(endpoint: string, data: any): Promise<T> {
    try {
      await new Promise(resolve => setTimeout(resolve, 300))
      const response = await fetch(config.apiBaseUrl + endpoint, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data)
      })

      // 204 No Content
      if (response.status === 204) {
        return {} as T
      }

      const result: ApiResponse<T> = await response.json()
      
      if (!response.ok) {
        throw new Error(result.message || result.error || `请求失败(${response.status})`)
      }

      if (!result.success) {
        throw new Error(result.message || result.error || '操作失败')
      }

      return result.data as T
    } catch (error: any) {
      console.error('PUT请求失败:', error)
      throw new Error(error.message || '网络请求失败')
    }
  },
  async delete<T>(endpoint: string): Promise<T> {
    try {
      await new Promise(resolve => setTimeout(resolve, 300))
      const response = await fetch(config.apiBaseUrl + endpoint, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
        }
      })

      // 204 No Content
      if (response.status === 204) {
        return {} as T
      }

      const result: ApiResponse<T> = await response.json()
      
      if (!response.ok) {
        throw new Error(result.message || result.error || `请求失败(${response.status})`)
      }

      if (!result.success) {
        throw new Error(result.message || result.error || '操作失败')
      }

      return result.data as T
    } catch (error: any) {
      console.error('DELETE请求失败:', error)
      throw new Error(error.message || '网络请求失败')
    }
  }
}