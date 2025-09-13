import axios from "axios";

const baseURL =
  (typeof import.meta !== "undefined" &&
    import.meta.env &&
    import.meta.env.VITE_API_URL) ||
  process.env.NEXT_PUBLIC_API_URL ||
  "https://localhost:5174/api";

export const axiosClient = axios.create({
  baseURL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Interceptor para agregar JWT token
axiosClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("access_token");
  if (token && token !== "null" && token !== "undefined" && token.length > 0) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor para manejar refresh token
axiosClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = localStorage.getItem("refresh_token");
        const accessToken = localStorage.getItem("access_token");

        if (
          refreshToken &&
          refreshToken !== "null" &&
          refreshToken !== "undefined" &&
          accessToken &&
          accessToken !== "null" &&
          accessToken !== "undefined"
        ) {
          const response = await axios.post(`${baseURL}/auth/refresh-token`, {
            refreshToken,
            accessToken,
          });

          const { AccessToken, RefreshToken } = response.data;
          localStorage.setItem("access_token", AccessToken);
          localStorage.setItem("refresh_token", RefreshToken);

          originalRequest.headers.Authorization = `Bearer ${AccessToken}`;
          return axiosClient(originalRequest);
        }
      } catch (refreshError) {
        console.error("Error refreshing token:", refreshError);
        localStorage.removeItem("access_token");
        localStorage.removeItem("refresh_token");
        window.location.href = "/login";
      }
    }

    return Promise.reject(error);
  }
);
