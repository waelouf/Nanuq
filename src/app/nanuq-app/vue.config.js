// CommonJS syntax
module.exports = {
  transpileDependencies: [],
  lintOnSave: false, // Disable ESLint during serve

  devServer: {
    proxy: {
      // Proxy API requests to backend during development
      '^/(kafka|redis|rabbitmq|sqlite)': {
        target: process.env.VUE_APP_API_BASE_URL || 'http://localhost:5000',
        changeOrigin: true,
        ws: false, // Disable WebSocket
        logLevel: 'debug',
      },
    },
  },

  // Configure webpack optimization for production
  chainWebpack: (config) => {
    // Disable defer for script tags - load synchronously for faster initial render
    config.plugin('html').tap(args => {
      args[0].scriptLoading = 'blocking';
      return args;
    });
    if (process.env.NODE_ENV === 'production') {
      // Disable performance hints during development
      config.performance.hints(false);
      
      // Optimize splitChunks configuration
      config.optimization.splitChunks({
        chunks: 'all',
        maxInitialRequests: Infinity,
        minSize: 20000, // Lower the default size for creating chunks (20kb)
        cacheGroups: {
          // Extract vue and related libraries into a single chunk
          vueVendors: {
            name: 'chunk-vue-vendors',
            test: /[\\/]node_modules[\\/](vue|vue-router|vuex)[\\/]/,
            priority: 30,
            chunks: 'initial',
          },
          // Separate Vuetify into its own chunk
          vuetify: {
            name: 'chunk-vuetify',
            test: /[\\/]node_modules[\\/]vuetify[\\/]/,
            priority: 20,
            chunks: 'initial',
          },
          // Put common utilities in their own chunk
          utils: {
            name: 'chunk-utils',
            test: /[\\/]node_modules[\\/](axios|core-js)[\\/]/,
            priority: 10,
            chunks: 'initial',
          },
          // Other vendor dependencies
          vendors: {
            name: 'chunk-vendors',
            test: /[\\/]node_modules[\\/]/,
            priority: 5,
            chunks: 'initial',
          },
          // Extract common code between chunks
          common: {
            name: 'chunk-common',
            minChunks: 2,
            priority: 1,
            chunks: 'initial',
            reuseExistingChunk: true,
          },
        },
      });
    }
  },
  
  // Configure webpack performance settings
  configureWebpack: {
    performance: {
      hints: process.env.NODE_ENV === 'production' ? 'warning' : false,
      maxEntrypointSize: 512000, // Increase from default 244KiB to 500KiB
      maxAssetSize: 512000, // Increase from default 244KiB to 500KiB
    },
    optimization: {
      // Minimize the bundle size
      usedExports: true,
      // Better module concatenation
      concatenateModules: true,
    },
  },
};
