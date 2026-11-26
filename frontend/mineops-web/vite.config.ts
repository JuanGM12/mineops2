import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    proxy: {
      "/identity": {
        target: "http://localhost:5001",
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/identity/, ""),
      },
      "/planning": {
        target: "http://localhost:5002",
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/planning/, ""),
      },
      "/maintenance": {
        target: "http://localhost:5003",
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/maintenance/, ""),
      },
    },
  },
});
