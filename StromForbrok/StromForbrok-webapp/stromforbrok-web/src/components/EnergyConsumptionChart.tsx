import { Alert, Paper, Skeleton, Typography, useTheme } from "@mui/material";
import { BarChart } from "@mui/x-charts/BarChart";
import { useGetConsumptionQuery } from "../api/dashboardApi";
import { useAppSelector } from "../app/hooks";
import { chartResolution, periodRange } from "../features/period/periodMath";

export function EnergyConsumptionChart() {
  const { anchorDate, resolution } = useAppSelector((s) => s.period);
  const { from, to } = periodRange(anchorDate, resolution);
  const { data, isFetching, error } = useGetConsumptionQuery({
    from,
    to,
    resolution: chartResolution(resolution),
  });

  const theme = useTheme();

  return (
    <Paper elevation={3} sx={{ p: 2 }}>
      <Typography variant="subtitle1" gutterBottom>
        Strømforbruk (kWh)
      </Typography>

      {error && <Alert severity="error">Henting av strømforbruk feilet</Alert>}
      {isFetching && <Skeleton variant="rectangular" height={300} />}

      {!isFetching && data && data.length === 0 && (
        <Typography color="text.secondary" sx={{ py: 6, textAlign: "center" }}>
          Klikk Hent data for å hente data for denne perioden
        </Typography>
      )}

      {!isFetching && data && data.length > 0 && (
        <BarChart
          height={300}
          xAxis={[{ scaleType: "band", data: data.map((d) => d.periodStart) }]}
          series={[
            {
              data: data.map((d) => d.kwh),
              label: "kWh",
              color: theme.palette.success.main,
            },
          ]}
        />
      )}
    </Paper>
  );
}
