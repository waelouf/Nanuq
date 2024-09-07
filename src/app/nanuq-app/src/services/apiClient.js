import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'http://192.168.50.101:5000', // process.env.NANUQ_SERVER_URL, // || 'http://api-server:5000',
  headers: {
    'Content-Type': 'application/json',
  },
});

export default apiClient;
