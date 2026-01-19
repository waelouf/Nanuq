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
  {
    path: '/aws',
    name: 'AWS',
    component: () => import(/* webpackChunkName: "aws" */ '@/aws/ListServers.vue'),
  },
  {
    path: '/aws/add',
    name: 'AWSAdd',
    component: () => import(/* webpackChunkName: "aws" */ '@/aws/AddServer.vue'),
  },
  {
    path: '/aws/manage/:serverId',
    name: 'ManageAWS',
    component: () => import(/* webpackChunkName: "aws" */ '@/aws/ManageAWS.vue'),
    props: true,
  },
  {
    path: '/aws/sqs/queue/:serverId',
    name: 'SQSQueueDetails',
    component: () => import(/* webpackChunkName: "aws" */ '@/aws/sqs/QueueDetails.vue'),
    props: true,
  },
  {
    path: '/aws/sns/topic/:serverId',
    name: 'SNSTopicDetails',
    component: () => import(/* webpackChunkName: "aws" */ '@/aws/sns/TopicDetails.vue'),
    props: true,
  },
  {
    path: '/azure',
    name: 'Azure',
    component: () => import(/* webpackChunkName: "azure" */ '@/azure/ListServers.vue'),
  },
  {
    path: '/azure/:serverId/manage',
    name: 'ManageAzure',
    component: () => import(/* webpackChunkName: "azure" */ '@/azure/ManageAzure.vue'),
    props: (route) => ({ serverId: route.params.serverId }),
  },
];

const router = createRouter({
  // history: createWebHistory(process.env.BASE_URL),
  history: createWebHashHistory(),
  routes,
});

export default router;
