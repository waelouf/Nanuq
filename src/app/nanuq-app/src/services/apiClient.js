import axios from 'axios';

// In production (Docker), nginx will proxy API calls to backend
// In development, vue.config.js devServer proxy will handle it
const apiClient = axios.create({
  baseURL: process.env.VUE_APP_API_BASE_URL || '',
  headers: {
    'Content-Type': 'application/json',
  },
});

export default apiClient;
