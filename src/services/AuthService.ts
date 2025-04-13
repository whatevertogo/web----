import { api } from './api';

// 登录请求接口
interface LoginRequest {
  username: string;
  password: string;
}

// 登录响应接口
interface LoginResponse {
  token: string;
}

// 用户信息接口
export interface UserInfo {
  userId: string;
  username: string;
  role: string;
}

// 认证服务
export const authService = {
  // 登录方法
  async login(username: string, password: string): Promise<UserInfo> {
    try {
      const response = await api.post<LoginResponse>('/auth/login', { username, password });

      if (response && response.token) {
        // 保存令牌到本地存储
        localStorage.setItem('auth_token', response.token);

        // 解析令牌获取用户信息
        const userInfo = this.parseToken(response.token);
        return userInfo;
      }

      throw new Error('登录失败：未收到有效的令牌');
    } catch (error: any) {
      console.error('登录失败:', error);
      throw new Error(`登录失败: ${error.message || '未知错误'}`);
    }
  },

  // 注册方法 (仅管理员可用)
  async register(username: string, password: string, role: string = 'Student'): Promise<boolean> {
    try {
      await api.post('/auth/register', { username, password, role });
      return true;
    } catch (error: any) {
      console.error('注册失败:', error);
      throw new Error(`注册失败: ${error.message || '未知错误'}`);
    }
  },

  // 获取当前用户信息
  async getCurrentUser(): Promise<UserInfo | null> {
    try {
      const token = this.getToken();
      if (!token) return null;

      // 尝试从令牌解析用户信息
      const userInfo = this.parseToken(token);

      // 验证令牌是否有效
      const response = await api.get<UserInfo>('/auth/me');
      return response || userInfo;
    } catch (error) {
      console.error('获取用户信息失败:', error);
      this.clearToken();
      return null;
    }
  },

  // 解析JWT令牌
  parseToken(token: string): UserInfo {
    try {
      // 将 JWT 令牌分成三部分：头部、负载和签名
      const parts = token.split('.');
      if (parts.length !== 3) {
        throw new Error('无效的 JWT 格式');
      }

      // 解码负载部分（第二部分）
      const payload = JSON.parse(this.base64UrlDecode(parts[1]));

      return {
        userId: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload.nameid || payload.sub,
        username: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || payload.name || payload.unique_name,
        role: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role
      };
    } catch (error) {
      console.error('解析令牌失败:', error);
      throw new Error('无效的令牌');
    }
  },

  // Base64Url 解码函数
  base64UrlDecode(input: string): string {
    // 替换 URL 安全的字符
    input = input.replace(/-/g, '+').replace(/_/g, '/');

    // 添加填充
    const pad = input.length % 4;
    if (pad) {
      if (pad === 1) {
        throw new Error('非法的 base64url 字符串');
      }
      input += new Array(5 - pad).join('=');
    }

    // 解码
    return decodeURIComponent(atob(input).split('').map(function(c) {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
  },

  // 获取当前令牌
  getToken(): string | null {
    return localStorage.getItem('auth_token');
  },

  // 清除令牌
  clearToken(): void {
    localStorage.removeItem('auth_token');
  },

  // 检查是否已登录
  isLoggedIn(): boolean {
    return !!this.getToken();
  },

  // 检查是否是管理员
  isAdmin(): boolean {
    try {
      const token = this.getToken();
      if (!token) return false;

      const userInfo = this.parseToken(token);
      return userInfo.role === 'Admin';
    } catch (error) {
      return false;
    }
  }
};
