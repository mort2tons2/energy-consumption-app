export type ChartResolution = "Day" | "Month";

export type ConsumptionPoint = {
  periodStart: string;
  kwh: number;
};

export type WeatherPoint = {
  periodStart: string;
  meanTemperature: number;
  degreeDays: number;
};

export type SourceStatus = {
  first: string | null;
  last: string | null;
  count: number;
};

export type SyncStatus = {
  consumption: SourceStatus;
  weather: SourceStatus;
};

export type UpsertCount = {
  fetched: number;
  added: number;
  updated: number;
};

export type SyncResult = {
  from: string;
  to: string;
  consumption: UpsertCount;
  weather: UpsertCount;
};

export type RangeArgs = {
  from: string;
  to: string;
  resolution: ChartResolution;
};

export type PeriodResolution = "Week" | "Month" | "Year";
