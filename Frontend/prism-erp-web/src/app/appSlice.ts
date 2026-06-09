import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

interface AppState {
  apiBaseUrl: string;
  isLoading: boolean;
}

const initialState: AppState = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? '/api',
  isLoading: false,
};

export const appSlice = createSlice({
  name: 'app',
  initialState,
  reducers: {
    setLoading(state, action: PayloadAction<boolean>) {
      state.isLoading = action.payload;
    },
  },
});

export const { setLoading } = appSlice.actions;
export const appReducer = appSlice.reducer;
