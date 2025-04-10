import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: {
    https: true,
    port: 5173,
    host: 'localhost',
    cors: true
  }
})
