import { createStore } from 'vuex';
import sqlite from './sqlite';
import kafka from './kafka';

export default createStore({
  modules: {
    sqlite,
    kafka,
  },
});
