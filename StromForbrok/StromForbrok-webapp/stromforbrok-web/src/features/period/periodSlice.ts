import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import { addMonths, addWeeks, addYears, format } from "date-fns";
import type { PeriodResolution } from "../../types/types";

type PeriodState = {
  resolution: PeriodResolution;
  anchorDate: string;
};

const todayIso = () => format(new Date(), "yyyy-MM-dd");

const initialState: PeriodState = {
  resolution: "Month",
  anchorDate: todayIso(),
};

const periodSlice = createSlice({
  name: "period",
  initialState,
  reducers: {
    setResolution(state, action: PayloadAction<PeriodResolution>) {
      state.resolution = action.payload;
    },
    shift(state, action: PayloadAction<-1 | 1>) {
      const d = new Date(`${state.anchorDate}T12:00:00`);
      const n = action.payload;
      const next =
        state.resolution === "Week"
          ? addWeeks(d, n)
          : state.resolution === "Month"
            ? addMonths(d, n)
            : addYears(d, n);
      state.anchorDate = format(next, "yyyy-MM-dd");
    },
    reset(state) {
      state.anchorDate = todayIso();
    },
  },
});

export const { setResolution, shift, reset } = periodSlice.actions;
export default periodSlice.reducer;
