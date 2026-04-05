import axios from 'axios';
import Cookies from 'js-cookie';

const api = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_URL || 'https://localhost:5001/api',
    headers: {
        'Content-Type': 'application/json',
    },
});

// Tự động thêm Token vào Header trước khi gửi đi
api.interceptors.request.use((config) => {
    const token = Cookies.get('access_token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

export default api;
