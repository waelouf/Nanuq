import { createStore } from 'vuex';
import sqlite from './sqlite';
import kafka from './kafka';
import redis from './redis';

export default createStore({
  modules: {
    sqlite,
    kafka,
    redis,
  },
});
