import { createStore } from 'vuex';
import sqlite from './sqlite';
import kafka from './kafka';
import redis from './redis';
import credentials from './credentials';
import rabbitmq from './rabbitmq';
import notifications from './notifications';
import activityLog from './activityLog';

export default createStore({
  modules: {
    sqlite,
    kafka,
    redis,
    credentials,
    rabbitmq,
    notifications,
    activityLog,
  },
});
