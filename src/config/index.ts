interface Config {
  apiBaseUrl: string;
  apiTimeout: number;
}

const config: Config = {
  apiBaseUrl: 'http://localhost:7289/api',
  apiTimeout: 10000, // 10秒超时
}

export default config;
