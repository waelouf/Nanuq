// CommonJS syntax
module.exports = {
  transpileDependencies: [],
  devServer: {
    proxy: {
      '/': {
        target: process.env.VUE_APP_API_BASE_URL || 'http://localhost:5000',
        changeOrigin: true,
        ws: false, // Disable WebSocket
      },
    },
  }
};
