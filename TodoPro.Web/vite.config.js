import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: {
    proxy: {
      // 將以 /api 開頭的請求在開發時轉發到後端
      '/api': {
        target: 'http://localhost:5120',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
