const { defineConfig } = require('@vue/cli-service');

module.exports = defineConfig({
  transpileDependencies: true,
  devServer: {
    proxy: {
      '/': {
        target: 'http://localhost:5224/',
        changeOrigin: true,
        ws: false, // Disable WebSocket proxying
      },
    },
  },
});
