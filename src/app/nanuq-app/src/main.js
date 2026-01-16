import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import store from './store';
import vuetify from './plugins/vuetify';
import logger from './utils/logger';

// Import Bootstrap styles locally instead of from CDN
import './css/styles.css';

// Initialize logger with store for notifications
logger.init(store);

createApp(App)
  .use(store)
  .use(vuetify)
  .use(router)
  .mount('#app');
