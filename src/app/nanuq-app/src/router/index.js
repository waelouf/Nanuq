import { createRouter, createWebHashHistory } from 'vue-router';

// Lazy load all components to reduce initial bundle size

const routes = [
  {
    path: '/',
    name: 'home',
    component: () => import(/* webpackChunkName: "dashboard" */ '@/home/Dashboard.vue'),
  },
  {
    path: '/dashboard',
    redirect: '/',
  },
  {
    path: '/old-home',
    name: 'OldHome',
    component: () => import(/* webpackChunkName: "home" */ '@/home/HomePage.vue'),
  },
  {
    path: '/kafka',
    name: 'Kafka',
    component: () => import(/* webpackChunkName: "kafka" */ '@/kafka/ListServers.vue'),
    children: [],
  },
  {
    path: '/kafka/add',
    name: 'KafkaAdd',
    component: () => import(/* webpackChunkName: "kafka" */ '@/kafka/AddServer.vue'),
  },
  {
    path: '/kafka/list',
    name: 'KafkaList',
    component: () => import(/* webpackChunkName: "kafka" */ '@/kafka/ListServers.vue'),
  },
  {
    path: '/kafka/connect/:serverName',
    name: 'KafkaConnect',
    component: () => import(/* webpackChunkName: "kafka" */ '@/kafka/KafkaConnect.vue'),
    props: true,
  },
  {
    path: '/redis',
    name: 'Redis',
    component: () => import(/* webpackChunkName: "redis" */ '@/redis/ListRedisServers.vue'),
  },
  {
    path: '/redis/server/:serverUrl',
    name: 'ManageServer',
    component: () => import(/* webpackChunkName: "redis" */ '@/redis/ManageDatabases.vue'),
    props: true,
  },
  {
    path: '/rabbitmq',
    name: 'RabbitMQ',
    component: () => import(/* webpackChunkName: "rabbitmq" */ '@/rabbitmq/ListServers.vue'),
  },
  {
    path: '/rabbitmq/manage/:serverUrl',
    name: 'ManageRabbitMQ',
    component: () => import(/* webpackChunkName: "rabbitmq" */ '@/rabbitmq/ManageRabbitMQ.vue'),
    props: true,
  },
  {
    path: '/activitylog',
    name: 'ActivityLog',
    component: () => import(/* webpackChunkName: "activitylog" */ '@/views/ActivityLog.vue'),
  },
];

const router = createRouter({
  // history: createWebHistory(process.env.BASE_URL),
  history: createWebHashHistory(),
  routes,
});

export default router;
