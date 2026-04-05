import axios from 'axios';
import Cookies from 'js-cookie';

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Cho phép truyền Cookie/Auth nếu cần
});

// Interceptor cho Request: Đính kèm Token
api.interceptors.request.use((config) => {
  const token = Cookies.get('access_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor cho Response: Xử lý lỗi toàn cục (Ví dụ: Token hết hạn)
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
       // Xử lý khi Token hết hạn (Logout hoặc Refresh Token ở đây)
       Cookies.remove('access_token');
       window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
