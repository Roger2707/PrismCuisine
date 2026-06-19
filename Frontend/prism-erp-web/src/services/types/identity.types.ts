// Identity Module Types

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  refreshTokenExpiresAt: string;
}

export interface LogoutRequest {
  refreshToken: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface CurrentUserResponse {
  userId: number;
  email: string;
  displayName: string;
  roles: string[];
  permissions: any;
}

export interface RefreshPageResponse {
  accessToken: string;
  accessTokenExpiresAt: string;
}

export interface UserDto {
  id: number;
  email: string;
  displayName: string;
  isActive: boolean;
  roles: string[];
}
