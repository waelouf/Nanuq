import { createRouter, createWebHashHistory } from 'vue-router';
import HomePage from '@/home/HomePage.vue';
import ListServers from '@/kafka/ListServers.vue';
import AddServer from '@/kafka/AddServer.vue';
import KafkaConnect from '@/kafka/KafkaConnect.vue';
import ListRedisServers from '@/redis/ListRedisServers.vue';
import ManageDatabases
  from '@/redis/ManageDatabases.vue';

const routes = [
  {
    path: '/',
    name: 'home',
    component: HomePage,
  },
  {
    path: '/kafka',
    name: 'Kafka',
    component: ListServers,
    children: [],
  },
  {
    path: '/kafka/add',
    name: 'KafkaAdd',
    component: AddServer,
  },
  {
    path: '/kafka/list',
    name: 'KafkaList',
    component: ListServers,
  },
  {
    path: '/kafka/connect/:serverName',
    name: 'KafkaConnect',
    component: KafkaConnect,
    props: true,
  },
  {
    path: '/redis',
    name: 'Redis',
    component: ListRedisServers,
  },
  {
    path: '/redis/server/:serverUrl',
    name: 'ManageServer',
    component: ManageDatabases,
    props: true,
  },
];

const router = createRouter({
  // history: createWebHistory(process.env.BASE_URL),
  history: createWebHashHistory(),
  routes,
});

export default router;
