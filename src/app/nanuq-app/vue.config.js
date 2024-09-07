const { defineConfig } = require('@vue/cli-service');

module.exports = defineConfig({
  transpileDependencies: true,
  devServer: {
    proxy: {
      '/': {
        target: process.env.VUE_APP_NANUQ_SERVER_URL || 'http://localhost:5000',
        changeOrigin: true,
        ws: false, // Disable WebSocket
      },
    },
  },
});
