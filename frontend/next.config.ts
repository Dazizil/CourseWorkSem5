import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Минимальный самодостаточный сервер для Docker-образа (.next/standalone)
  output: "standalone",
  cacheComponents: true,
  partialPrefetching: true,
  turbopack: {
    rules: {
      "*.css": {
        loaders: ["@tailwindcss/turbopack"],
        as: "*.css",
      },
    },
  },
};

export default nextConfig;
