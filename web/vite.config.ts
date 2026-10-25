/// <reference types="vitest/config" />
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // The API runs separately in development; proxying keeps everything same-origin so the auth cookie just works
    proxy: {
      '/api': 'http://localhost:5157',
      '/hubs': { target: 'http://localhost:5157', ws: true },
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
  },
})
