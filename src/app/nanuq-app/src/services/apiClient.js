import axios from 'axios';

const apiClient = axios.create({
  baseURL: process.env.VUE_APP_NANUQ_SERVER_URL || 'http://localhost:5000',
  headers: {
    'Content-Type': 'application/json',
  },
});

export default apiClient;
