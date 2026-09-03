import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

const target =
  process.env["services__stromforbrok-api__https__0"] ??
  process.env["services__stromforbrok-api__http__0"];

export default defineConfig({
  plugins: [react()],
  server: {
    port: Number(process.env.PORT) || 5173,
    strictPort: true,
    proxy: target
      ? {
          "/api": {
            target,
            changeOrigin: true,
            secure: false,
            rewrite: (path) => path.replace(/^\/api/, ""),
          },
        }
      : undefined,
  },
});
