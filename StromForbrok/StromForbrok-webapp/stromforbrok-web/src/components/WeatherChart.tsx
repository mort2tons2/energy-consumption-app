import { Alert, Paper, Skeleton, Typography, useTheme } from "@mui/material";
import { LineChart } from "@mui/x-charts/LineChart";
import { useGetWeatherQuery } from "../api/dashboardApi";
import { useAppSelector } from "../app/hooks";
import { chartResolution, periodRange } from "../features/period/periodMath";

export function WeatherChart() {
  const { anchorDate, resolution } = useAppSelector((s) => s.period);
  const { from, to } = periodRange(anchorDate, resolution);
  const { data, isFetching, error } = useGetWeatherQuery({
    from,
    to,
    resolution: chartResolution(resolution),
  });

  const theme = useTheme();

  return (
    <Paper elevation={3} sx={{ p: 2 }}>
      <Typography variant="subtitle1" gutterBottom>
        Temperatur &amp; graddager
      </Typography>

      {error && <Alert severity="error">Henting av vær feilet.</Alert>}
      {isFetching && <Skeleton variant="rectangular" height={300} />}

      {!isFetching && data && data.length === 0 && (
        <Typography color="text.secondary" sx={{ py: 6, textAlign: "center" }}>
          Klikk Hent data for å hente data for denne perioden
        </Typography>
      )}

      {!isFetching && data && data.length > 0 && (
        <LineChart
          height={300}
          xAxis={[{ scaleType: "point", data: data.map((d) => d.periodStart) }]}
          series={[
            {
              data: data.map((d) => d.meanTemperature),
              label: "Temperatur °C (gjennomsnitt)",
              color: theme.palette.warning.main,
            },
            {
              data: data.map((d) => d.degreeDays),
              label: "Graddager",
              color: theme.palette.primary.main,
            },
          ]}
        />
      )}
    </Paper>
  );
}
