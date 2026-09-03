import { configureStore } from '@reduxjs/toolkit';
import { dashboardApi } from '../api/dashboardApi';
import periodReducer from '../features/period/periodSlice';

export const store = configureStore({
  reducer: {
    period: periodReducer,
    [dashboardApi.reducerPath]: dashboardApi.reducer,
  },
  middleware: (getDefaultMiddleware) => getDefaultMiddleware().concat(dashboardApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
