import apiClient from './apiClient';
import type {
  LoginResponse,
  LogoutRequest,
  ChangePasswordRequest,
  CurrentUserResponse,
  RefreshPageResponse,
  UserDto,
} from './types/identity.types';

// Identity/Auth Module
export const authApi = {
  login: async (email: string, password: string): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>('/api/identity/auth/login', { email, password });
    return response.data;
  },
  logout: async (request: LogoutRequest): Promise<void> => {
    await apiClient.post('/api/identity/auth/logout', request);
  },
  getCurrentUser: async (): Promise<CurrentUserResponse> => {
    const response = await apiClient.get<CurrentUserResponse>('/api/identity/auth/current-user');
    return response.data;
  },
  changePassword: async (request: ChangePasswordRequest): Promise<void> => {
    await apiClient.post('/api/identity/auth/change-password', request);
  },
  refreshPage: async (): Promise<RefreshPageResponse> => {
    const response = await apiClient.post<RefreshPageResponse>('/api/identity/auth/refresh-page');
    return response.data;
  },
};

// Identity/Users Module
export const usersApi = {
  getById: async (id: number): Promise<UserDto> => {
    const response = await apiClient.get<UserDto>(`/api/identity/users/${id}`);
    return response.data;
  },
};
