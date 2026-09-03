import {
  Alert,
  Button,
  CircularProgress,
  Stack,
  Typography,
} from "@mui/material";
import { useGetSyncStatusQuery, useSyncMutation } from "../api/dashboardApi";
import { useAppSelector } from "../app/hooks";
import { periodRange } from "../features/period/periodMath";

const shortDateFormat = (value: string | null) =>
  value ? value.slice(0, 10) : "—";

export function SyncButton() {
  const { anchorDate, resolution } = useAppSelector((s) => s.period);
  const { data: status } = useGetSyncStatusQuery();
  const [sync, { isLoading, error, data }] = useSyncMutation();

  const fetchData = () => {
    const { from, to } = periodRange(anchorDate, resolution);
    sync({ from, to });
  };

  return (
    <Stack spacing={1}>
      <Stack
        direction="row"
        spacing={2}
        useFlexGap
        sx={{ alignItems: "center", flexWrap: "wrap" }}
      >
        <Button
          variant="contained"
          onClick={fetchData}
          disabled={isLoading}
          sx={{ minWidth: 160 }}
        >
          {isLoading ? (
            <CircularProgress size={20} color="inherit" />
          ) : (
            "Hent data"
          )}
        </Button>
        {status && (
          <Typography variant="body2" color="text.secondary">
            I databasen: Lagret: {status.consumption.count} rader med
            strømforbruk (til: {shortDateFormat(status.consumption.last)}),{" "}
            {status.weather.count} vær rader (til:{" "}
            {shortDateFormat(status.weather.last)})
          </Typography>
        )}
      </Stack>

      {error && (
        <Alert severity="error">
          Henting av data feilet {JSON.stringify(error)}
        </Alert>
      )}
      {data && (
        <Alert severity="success">
          Strømforbruk: +{data.consumption.added} nye,{" "}
          {data.consumption.updated} oppdatert. Vær: +{data.weather.added} nye,{" "}
          {data.weather.updated} opdatert.
        </Alert>
      )}
    </Stack>
  );
}
