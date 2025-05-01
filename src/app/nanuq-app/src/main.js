import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import store from './store';
import vuetify from './plugins/vuetify';

createApp(App)
  .use(store)
  .use(vuetify)
  .use(router)
  .mount('#app');
