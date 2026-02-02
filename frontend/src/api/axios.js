import axios from 'axios';

const BASE_URL = 'http://localhost:5176/api';

const axiosClient = axios.create({
    baseURL: BASE_URL,
    withCredentials: true,
    headers: {
        'Content-Type': 'application/json',
    }
});

let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
    failedQueue.forEach(prom => {
        if (error) {
            prom.reject(error);
        } else {
            prom.resolve(token);
        }
    });
    
    failedQueue = [];
};

axiosClient.interceptors.response.use(
    (response) => response, 
    async (error) => {
        const originalRequest = error.config;

        if (originalRequest.url?.includes('/auth/refresh')) {
            return Promise.reject(error); 
        }

        if (originalRequest.url?.includes('/auth/logout')) {
            return Promise.reject(error);
        }
        
        if (originalRequest.url?.includes('/auth/profile') && originalRequest._retry) {
             return Promise.reject(error);
        }

        if (error.response?.status === 401 && !originalRequest._retry) {
            if (isRefreshing) {
                return new Promise((resolve, reject) => {
                    failedQueue.push({ resolve, reject });
                }).then(() => {
                    return axiosClient(originalRequest);
                }).catch((err) => {
                    return Promise.reject(err);
                });
            }

            originalRequest._retry = true;
            isRefreshing = true;

            try {
                await axiosClient.post('/auth/refresh');
                isRefreshing = false;
                processQueue(null);
                return axiosClient(originalRequest);
            } catch (refreshError) {
                isRefreshing = false;
                processQueue(refreshError, null);
                
                console.error('Token refresh failed.');

                if (window.location.pathname !== '/login' && window.location.pathname !== '/register') {
                }
                
                return Promise.reject(refreshError);
            }
        }

        return Promise.reject(error);
    }
);

export default axiosClient;