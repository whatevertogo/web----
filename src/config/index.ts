/**
 * 全局配置文件
 */

interface AppConfig {
  apiBaseUrl: string;
  apiTimeout: number;
  defaultPageSize: number;
}

const config: AppConfig = {
  // API基础URL，根据环境区分
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL || 'http://localhost:7289/api',
  
  // API请求超时时间(毫秒)
  apiTimeout: 10000,
  
  // 分页默认每页条数
  defaultPageSize: 10
};

export default config;
