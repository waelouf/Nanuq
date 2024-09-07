const { defineConfig } = require('@vue/cli-service');

module.exports = defineConfig({
  transpileDependencies: true,
  devServer: {
    proxy: {
      '/': {
        target: 'http://192.168.50.101:5000',
        changeOrigin: true,
        ws: false, // Disable WebSocket proxying
      },
    },
  },
});
