import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { ConsumptionPoint, WeatherPoint, SyncStatus, SyncResult, RangeArgs } from '../types/types';

export const dashboardApi = createApi({
  reducerPath: 'dashboardApi',
  baseQuery: fetchBaseQuery({ baseUrl: '/api' }),
  tagTypes: ['Data'],
  endpoints: (build) => ({
    getConsumption: build.query<ConsumptionPoint[], RangeArgs>({
      query: ({ from, to, resolution }) =>
        `consumption?from=${from}&to=${to}&resolution=${resolution}`,
      providesTags: ['Data'],
    }),
    getWeather: build.query<WeatherPoint[], RangeArgs>({
      query: ({ from, to, resolution }) =>
        `weather?from=${from}&to=${to}&resolution=${resolution}`,
      providesTags: ['Data'],
    }),
    getSyncStatus: build.query<SyncStatus, void>({
      query: () => 'sync/status',
      providesTags: ['Data'],
    }),
    sync: build.mutation<SyncResult, { from: string; to: string }>({
      query: ({ from, to }) => ({ url: `sync?from=${from}&to=${to}`, method: 'POST' }),
      invalidatesTags: ['Data'],
    }),
  }),
});

export const {
  useGetConsumptionQuery,
  useGetWeatherQuery,
  useGetSyncStatusQuery,
  useSyncMutation,
} = dashboardApi;
