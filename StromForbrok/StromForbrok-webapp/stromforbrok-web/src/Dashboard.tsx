import { Box, Container, Stack, Typography } from "@mui/material";
import { EnergyConsumptionChart } from "./components/EnergyConsumptionChart";
import { PeriodNavigation } from "./components/PeriodNavigation";
import { SyncButton } from "./components/SyncButton";
import { WeatherChart } from "./components/WeatherChart";

export function Dashboard() {
  return (
    <Container maxWidth="lg" sx={{ py: 3 }}>
      <Stack spacing={3}>
        <Typography variant="h4">Strømforbruk</Typography>
        <PeriodNavigation />
        <SyncButton />
        <Box
          sx={{
            display: "grid",
            gap: 3,
            gridTemplateColumns: { xs: "1fr", md: "1fr 1fr" },
          }}
        >
          <EnergyConsumptionChart />
          <WeatherChart />
        </Box>
      </Stack>
    </Container>
  );
}
