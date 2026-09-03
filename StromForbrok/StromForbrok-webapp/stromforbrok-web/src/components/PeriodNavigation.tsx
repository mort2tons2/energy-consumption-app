import {
  Button,
  IconButton,
  Stack,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from "@mui/material";
import { useAppDispatch, useAppSelector } from "../app/hooks";
import { periodLabel } from "../features/period/periodMath";
import { reset, setResolution, shift } from "../features/period/periodSlice";
import type { PeriodResolution } from "../types/types";

export function PeriodNavigation() {
  const dispatch = useAppDispatch();
  const { anchorDate, resolution } = useAppSelector((s) => s.period);

  return (
    <Stack
      direction="row"
      spacing={2}
      useFlexGap
      sx={{ alignItems: "center", flexWrap: "wrap" }}
    >
      <ToggleButtonGroup
        exclusive
        size="small"
        value={resolution}
        onChange={(_, value: PeriodResolution | null) => {
          if (value) {
            dispatch(setResolution(value));
          }
        }}
      >
        <ToggleButton value="Week">Uke</ToggleButton>
        <ToggleButton value="Month">Måned</ToggleButton>
        <ToggleButton value="Year">År</ToggleButton>
      </ToggleButtonGroup>

      <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
        <IconButton onClick={() => dispatch(shift(-1))}>{"‹"}</IconButton>
        <Typography variant="h6" sx={{ minWidth: 150, textAlign: "center" }}>
          {periodLabel(anchorDate, resolution)}
        </Typography>
        <IconButton onClick={() => dispatch(shift(1))}>{"›"}</IconButton>
      </Stack>

      <Button size="small" onClick={() => dispatch(reset())}>
        I dag
      </Button>
    </Stack>
  );
}
