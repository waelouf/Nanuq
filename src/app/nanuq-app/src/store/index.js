import { createStore } from 'vuex';
import sqlite from './sqlite';

export default createStore({
  modules: {
    sqlite,
  },
});
