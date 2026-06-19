import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { CurrentUserResponse } from '../services/types/identity.types';

interface UserState {
  accessToken: string | null;
  user: CurrentUserResponse | null;
  isAuthenticated: boolean;
  isHydrating: boolean;
}

const initialState: UserState = {
  accessToken: null,
  user: null,
  isAuthenticated: false,
  isHydrating: true,
};

export const userSlice = createSlice({
  name: 'user',
  initialState,
  reducers: {
    setAccessToken(state, action: PayloadAction<string>) {
      state.accessToken = action.payload;
    },
    setUser(state, action: PayloadAction<CurrentUserResponse>) {
      state.user = action.payload;
      state.isAuthenticated = true;
    },
    setAuth(
      state,
      action: PayloadAction<{ accessToken: string; user: CurrentUserResponse }>,
    ) {
      state.accessToken = action.payload.accessToken;
      state.user = action.payload.user;
      state.isAuthenticated = true;
      state.isHydrating = false;
    },
    clearAuth(state) {
      state.accessToken = null;
      state.user = null;
      state.isAuthenticated = false;
      state.isHydrating = false;
    },
    finishHydration(state) {
      state.isHydrating = false;
    },
  },
});

export const { setAccessToken, setUser, setAuth, clearAuth, finishHydration } = userSlice.actions;
export const userReducer = userSlice.reducer;
