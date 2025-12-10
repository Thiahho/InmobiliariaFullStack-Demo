/** @type {import('next').NextConfig} */
const nextConfig = {
  images: {
    domains: [
      "localhost",
      "drive.google.com",
      "inmobiliariafullstack-demo.onrender.com",
      "inmobiliaria-full-stack-demo.vercel.app",
      "docs.google.com",
      "img.youtube.com",
      "i.ytimg.com",
      "vimeo.com",
      "player.vimeo.com",
      "imgur.com",
      "dropbox.com",
      "tile.openstreetmap.org",
      "a.tile.openstreetmap.org",
      "b.tile.openstreetmap.org",
      "c.tile.openstreetmap.org",
      "cdnjs.cloudflare.com",
    ],
    formats: ["image/webp", "image/avif"],
  },
  env: {
    NEXT_PUBLIC_API_URL:
      process.env.NEXT_PUBLIC_API_URL ||
      "https://inmobiliariafullstack-demo.onrender.com/api",
    NEXT_PUBLIC_BASE_URL:
      process.env.NEXT_PUBLIC_BASE_URL || "http://localhost:3000",
  },
};

module.exports = nextConfig;
