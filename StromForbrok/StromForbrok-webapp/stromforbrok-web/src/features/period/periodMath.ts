import {
  endOfMonth,
  endOfWeek,
  endOfYear,
  format,
  startOfMonth,
  startOfWeek,
  startOfYear,
} from "date-fns";
import type { ChartResolution, PeriodResolution } from "../../types/types";

const fmt = (d: Date) => format(d, "yyyy-MM-dd");
const anchorDate = (anchorIso: string) => new Date(`${anchorIso}T12:00:00`);
const monday = { weekStartsOn: 1 as const };

export function periodRange(
  anchorIso: string,
  resolution: PeriodResolution,
): { from: string; to: string } {
  const d = anchorDate(anchorIso);
  if (resolution === "Week") {
    return { from: fmt(startOfWeek(d, monday)), to: fmt(endOfWeek(d, monday)) };
  }
  if (resolution === "Month") {
    return { from: fmt(startOfMonth(d)), to: fmt(endOfMonth(d)) };
  }
  return { from: fmt(startOfYear(d)), to: fmt(endOfYear(d)) };
}

export function periodLabel(
  anchorIso: string,
  resolution: PeriodResolution,
): string {
  const d = anchorDate(anchorIso);
  if (resolution === "Week") {
    return `${format(startOfWeek(d, monday), "d MMM")} – ${format(endOfWeek(d, monday), "d MMM yyyy")}`;
  }
  if (resolution === "Month") {
    return format(d, "MMMM yyyy");
  }
  return format(d, "yyyy");
}

export function chartResolution(resolution: PeriodResolution): ChartResolution {
  return resolution === "Year" ? "Month" : "Day";
}
