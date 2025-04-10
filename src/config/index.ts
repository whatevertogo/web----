interface Config {
  apiBaseUrl: string;
  apiTimeout: number;
}

const config: Config = {
  apiBaseUrl: '/api',
  apiTimeout: 10000, // 10秒超时
}

export default config;
