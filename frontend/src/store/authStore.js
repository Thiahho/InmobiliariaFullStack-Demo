import { create } from "zustand";
import { axiosClient } from "../lib/axiosClient";

export const Roles = {
  Admin: "Admin",
  Agente: "Agente",
  Cargador: "Cargador",
};

const rolePermissions = {
  [Roles.Admin]: [
    "view_dashboard",
    "manage_propiedades",
    "manage_agentes",
    "manage_leads",
    "manage_visitas",
    "upload_media",
  ],
  [Roles.Agente]: [
    "view_dashboard",
    "manage_propiedades",
    "manage_leads",
    "manage_visitas",
  ],
  [Roles.Cargador]: ["upload_media", "manage_propiedades"],
};

export const useAuthStore = create((set, get) => ({
  isAuthenticated: false,
  user: null,
  role: null,
  isInitialized: false,

  initializeAuth: async () => {
    const token = localStorage.getItem('access_token');
    if (!token || token === 'null' || token === 'undefined') {
      set({ isInitialized: true });
      return;
    }

    try {
      const response = await axiosClient.get('/auth/me');
      const userData = response.data;

      set({
        isAuthenticated: true,
        user: userData,
        role: userData.rol || userData.role,
        isInitialized: true
      });
    } catch (error) {
      console.error('Error initializing auth:', error);
      localStorage.removeItem('access_token');
      localStorage.removeItem('refresh_token');
      set({
        isAuthenticated: false,
        user: null,
        role: null,
        isInitialized: true
      });
    }
  },

  login: (user, role = Roles.Admin) => set({ isAuthenticated: true, user, role }),
  logout: () => {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    set({ isAuthenticated: false, user: null, role: null });
  },
  setRole: (role) => set({ role }),
  updateProfile: async (updates) => {
    try {
      const res = await axiosClient.put('/usuarios/profile', updates);
      const updated = res.data;
      set((state) => ({
        user: { ...(state.user || {}), ...updated },
      }));
      return updated;
    } catch (err) {
      const message = err?.response?.data?.message || 'Error al actualizar perfil';
      throw new Error(message);
    }
  },
  hasPermission: (perm) => {
    const role = get().role;
    if (!role) return false;
    return rolePermissions[role]?.includes(perm) ?? false;
  },
}));

export const getRolePermissions = (role) => rolePermissions[role] || [];


