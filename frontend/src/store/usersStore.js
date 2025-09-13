import { create } from "zustand";
import { axiosClient } from "../lib/axiosClient";

export const useUsersStore = create((set, get) => ({
  users: [],
  loading: false,
  error: null,

  fetchUsers: async () => {
    set({ loading: true, error: null });
    try {
      const res = await axiosClient.get('/usuarios');
      set({ users: res.data || [], loading: false });
    } catch (e) {
      set({ loading: false, error: 'Error al cargar usuarios' });
    }
  },

  createUser: async (user) => {
    const res = await axiosClient.post('/usuarios', user);
    const nuevo = res.data;
    set(state => ({ users: [nuevo, ...state.users] }));
    return nuevo;
  },

  updateUser: async (id, updates) => {
    const res = await axiosClient.put('/usuarios/profile', updates);
    const updated = res.data;
    set(state => ({
      users: state.users.map(u => u.id === updated.id ? updated : u)
    }));
  },

  toggleActivo: async (id) => {
    await axiosClient.post(`/usuarios/${id}/toggle-activo`);
    set(state => ({
      users: state.users.map(u => u.id === id ? { ...u, activo: !u.activo } : u)
    }));
  }
}));


