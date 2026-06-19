import axios from 'axios';
import { store } from '../app/store';
import { clearAuth } from '../app/userSlice';
import { parseApiError, type ApiError } from '../utils/errorHandler';

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ??
  (import.meta.env.PROD ? '' : 'http://localhost:5085');

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use(
  (config) => {
    const token = store.getState().user.accessToken;
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error),
);

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const apiError = parseApiError(error);
    (error as { parsedError?: ApiError }).parsedError = apiError;

    if (apiError.type === 'unauthorized') {
      const url = error.config?.url as string | undefined;
      const isAuthEndpoint =
        url?.includes('/api/identity/auth/login') ||
        url?.includes('/api/identity/auth/refresh-page');

      if (!isAuthEndpoint) {
        store.dispatch(clearAuth());
      }
    }

    return Promise.reject(error);
  },
);

export default apiClient;
export type { ApiError };
