import { create } from "zustand";
import { axiosClient } from "../lib/axiosClient";
import toast from "react-hot-toast";

export const usePropiedadesStore = create((set, get) => ({
  // Estado
  propiedades: [],
  propiedadActual: null,
  mediasPropiedad: [],
  filtros: {
    operacion: '',
    tipo: '',
    barrio: '',
    comuna: '',
    precioMin: '',
    precioMax: '',
    ambientes: '',
    dormitorios: '',
    cochera: null,
    estado: 'Activo',
    destacado: null,
    page: 1,
    pageSize: 20,
    orderBy: 'FechaPublicacionUtc',
    orderDesc: true
  },
  paginacion: {
    totalCount: 0,
    totalPages: 0,
    currentPage: 1,
    pageSize: 20
  },
  loading: false,
  error: null,

  // Acciones para propiedades
  fetchPropiedades: async (filtrosCustom = null) => {
    set({ loading: true, error: null });
    try {
      const filtrosFinales = filtrosCustom || get().filtros;
      
      // Verificar si hay filtros activos (incluyendo searchTerm)
      const defaultFilters = {
        operacion: '',
        tipo: '',
        barrio: '',
        comuna: '',
        precioMin: '',
        precioMax: '',
        ambientes: '',
        dormitorios: '',
        cochera: null,
        estado: 'Activo',
        destacado: null
      };

      const hasFilters = Object.entries(filtrosFinales).some(([key, value]) => {
        // Ignorar campos de paginación y ordenamiento
        if (key === 'page' || key === 'pageSize' || key === 'orderBy' || key === 'orderDesc') {
          return false;
        }
        
        // Verificar si el valor es diferente al valor por defecto
        const defaultValue = defaultFilters[key];
        return value !== defaultValue && value !== '' && value !== null && value !== undefined;
      });

      let response;
      
      if (hasFilters) {
        // Usar búsqueda avanzada si hay filtros
        const searchData = {
          page: filtrosFinales.page || 1,
          pageSize: filtrosFinales.pageSize || 20,
          operacion: filtrosFinales.operacion || null,
          tipo: filtrosFinales.tipo || null,
          barrio: filtrosFinales.barrio || null,
          comuna: filtrosFinales.comuna || null,
          precioMin: filtrosFinales.precioMin ? parseFloat(filtrosFinales.precioMin) : null,
          precioMax: filtrosFinales.precioMax ? parseFloat(filtrosFinales.precioMax) : null,
          ambientes: filtrosFinales.ambientes ? parseInt(filtrosFinales.ambientes) : null,
          dormitorios: filtrosFinales.dormitorios ? parseInt(filtrosFinales.dormitorios) : null,
          cochera: filtrosFinales.cochera,
          estado: filtrosFinales.estado || null,
          destacado: filtrosFinales.destacado,
          searchTerm: filtrosFinales.searchTerm || null // Agregar searchTerm como campo separado
        };

        response = await axiosClient.post("/propiedades/buscar-avanzada", searchData);
        const { Data, TotalCount, Pagina, TamanoPagina, TotalPaginas } = response.data;

        set({
          propiedades: Data || [],
          paginacion: {
            totalCount: TotalCount || 0,
            totalPages: TotalPaginas || 0,
            currentPage: Pagina || 1,
            pageSize: TamanoPagina || 20
          },
          loading: false
        });

        return Data;
      } else {
        // Usar endpoint simple de paginación si no hay filtros
        const params = new URLSearchParams();
        params.append('pagina', filtrosFinales.page || 1);
        params.append('tamanoPagina', filtrosFinales.pageSize || 20);

        response = await axiosClient.get(`/propiedades/paginadas?${params}`);
        const { data, totalCount, totalPaginas, pagina, tamanoPagina } = response.data;

        set({
          propiedades: data || [],
          paginacion: {
            totalCount: totalCount || 0,
            totalPages: totalPaginas || 0,
            currentPage: pagina || 1,
            pageSize: tamanoPagina || 20
          },
          loading: false
        });

        return data;
      }
    } catch (error) {
      console.error("Error al obtener propiedades:", error);
      set({ 
        propiedades: [], // Asegurar que siempre sea un array
        error: error.response?.data?.message || "Error al cargar propiedades", 
        loading: false 
      });
      toast.error("Error al cargar propiedades");
      throw error;
    }
  },

  fetchPropiedadById: async (id) => {
    set({ loading: true, error: null });
    try {
      const response = await axiosClient.get(`/propiedades/${id}`);
      set({ propiedadActual: response.data, loading: false });
      return response.data;
    } catch (error) {
      console.error("Error al obtener propiedad:", error);
      set({ error: error.response?.data?.message || "Error al cargar propiedad", loading: false });
      toast.error("Error al cargar propiedad");
      throw error;
    }
  },

  createPropiedad: async (propiedadData) => {
    set({ loading: true, error: null });
    try {
      const response = await axiosClient.post("/propiedades", propiedadData);
      
      // Actualizar lista local
      const nuevaPropiedad = response.data;
      set(state => ({
        propiedades: [nuevaPropiedad, ...(state.propiedades || [])],
        propiedadActual: nuevaPropiedad,
        loading: false
      }));

      toast.success("Propiedad creada exitosamente");
      return nuevaPropiedad;
    } catch (error) {
      console.error("Error al crear propiedad:", error);
      const message = error.response?.data?.message || "Error al crear propiedad";
      set({ error: message, loading: false });
      toast.error(message);
      throw error;
    }
  },

  updatePropiedad: async (id, propiedadData) => {
    set({ loading: true, error: null });
    try {
      await axiosClient.put(`/propiedades/${id}`, { ...propiedadData, id });
      
      // Actualizar lista local
      set(state => ({
        propiedades: (state.propiedades || []).map(p => 
          p.id === id ? { ...p, ...propiedadData } : p
        ),
        loading: false
      }));

      // Si es la propiedad actual, actualizarla también
      if (get().propiedadActual?.id === id) {
        set(state => ({
          propiedadActual: { ...state.propiedadActual, ...propiedadData }
        }));
      }

      toast.success("Propiedad actualizada exitosamente");
    } catch (error) {
      console.error("Error al actualizar propiedad:", error);
      const message = error.response?.data?.message || "Error al actualizar propiedad";
      set({ error: message, loading: false });
      toast.error(message);
      throw error;
    }
  },

  deletePropiedad: async (id) => {
    set({ loading: true, error: null });
    try {
      await axiosClient.delete(`/propiedades/${id}`);
      
      // Remover de lista local
      set(state => ({
        propiedades: state.propiedades.filter(p => p.id !== id),
        propiedadActual: state.propiedadActual?.id === id ? null : state.propiedadActual,
        loading: false
      }));

      toast.success("Propiedad eliminada exitosamente");
    } catch (error) {
      console.error("Error al eliminar propiedad:", error);
      const message = error.response?.data?.message || "Error al eliminar propiedad";
      set({ error: message, loading: false });
      toast.error(message);
      throw error;
    }
  },

  // Acciones para filtros
  setFiltros: (nuevosFiltros) => {
    set(state => ({
      filtros: { ...state.filtros, ...nuevosFiltros }
    }));
  },

  resetFiltros: () => {
    set({
      filtros: {
        operacion: '',
        tipo: '',
        barrio: '',
        comuna: '',
        precioMin: '',
        precioMax: '',
        ambientes: '',
        dormitorios: '',
        cochera: null,
        estado: 'Activo',
        destacado: null,
        page: 1,
        pageSize: 20,
        orderBy: 'FechaPublicacionUtc',
        orderDesc: true
      }
    });
  },

  // Acciones para media
  fetchMediasByPropiedad: async (propiedadId) => {
    try {
      const response = await axiosClient.get(`/media/propiedad/${propiedadId}`);
      set({ mediasPropiedad: response.data });
      return response.data;
    } catch (error) {
      console.error("Error al obtener medias:", error);
      toast.error("Error al cargar imágenes");
      throw error;
    }
  },

  uploadMedia: async (propiedadId, file, titulo = null) => {
    try {
      const formData = new FormData();
      formData.append('file', file);
      if (titulo) formData.append('titulo', titulo);

      const response = await axiosClient.post(`/media/upload/${propiedadId}`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });

      // Actualizar medias locales
      set(state => ({
        mediasPropiedad: [...(state.mediasPropiedad || []), response.data]
      }));

      toast.success("Imagen subida exitosamente");
      return response.data;
    } catch (error) {
      console.error("Error al subir media:", error);
      toast.error("Error al subir imagen");
      throw error;
    }
  },

  bulkUploadMedia: async (propiedadId, files, tituloBase = null) => {
    try {
      const formData = new FormData();
      files.forEach(file => formData.append('files', file));
      if (tituloBase) formData.append('tituloBase', tituloBase);

      const response = await axiosClient.post(`/media/bulk-upload/${propiedadId}`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });

      // Actualizar medias locales
      set(state => ({
        mediasPropiedad: [...(state.mediasPropiedad || []), ...(response.data || [])]
      }));

      toast.success(`${response.data.length} imágenes subidas exitosamente`);
      return response.data;
    } catch (error) {
      console.error("Error al subir medias:", error);
      toast.error("Error al subir imágenes");
      throw error;
    }
  },

  addExternalUrl: async (propiedadId, urlData) => {
    try {
      const response = await axiosClient.post(`/media/url/${propiedadId}`, urlData);
      
      // Actualizar medias locales
      set(state => ({
        mediasPropiedad: [...(state.mediasPropiedad || []), response.data]
      }));

      toast.success("URL externa agregada exitosamente");
      return response.data;
    } catch (error) {
      console.error("Error al agregar URL externa:", error);
      toast.error("Error al agregar URL externa");
      throw error;
    }
  },

  updateMedia: async (mediaId, updateData) => {
    try {
      await axiosClient.put(`/media/${mediaId}`, updateData);
      
      // Actualizar medias locales
      set(state => ({
        mediasPropiedad: (state.mediasPropiedad || []).map(m => 
          m.id === mediaId ? { ...m, ...updateData } : m
        )
      }));

      toast.success("Media actualizada exitosamente");
    } catch (error) {
      console.error("Error al actualizar media:", error);
      toast.error("Error al actualizar media");
      throw error;
    }
  },

  deleteMedia: async (mediaId) => {
    try {
      await axiosClient.delete(`/media/${mediaId}`);
      
      // Remover de medias locales
      set(state => ({
        mediasPropiedad: state.mediasPropiedad.filter(m => m.id !== mediaId)
      }));

      toast.success("Media eliminada exitosamente");
    } catch (error) {
      console.error("Error al eliminar media:", error);
      toast.error("Error al eliminar media");
      throw error;
    }
  },

  reorderMedia: async (propiedadId, ordenItems) => {
    try {
      await axiosClient.put(`/media/reorder/${propiedadId}`, { items: ordenItems });
      
      // Actualizar orden local
      set(state => {
        const mediasActualizadas = [...(state.mediasPropiedad || [])];
        ordenItems.forEach(item => {
          const mediaIndex = mediasActualizadas.findIndex(m => m.id === item.id);
          if (mediaIndex !== -1) {
            mediasActualizadas[mediaIndex].orden = item.orden;
          }
        });
        
        // Reordenar array
        mediasActualizadas.sort((a, b) => a.orden - b.orden);
        
        return { mediasPropiedad: mediasActualizadas };
      });

      toast.success("Orden actualizado exitosamente");
    } catch (error) {
      console.error("Error al reordenar medias:", error);
      toast.error("Error al reordenar medias");
      throw error;
    }
  },

  validateUrl: async (url) => {
    try {
      const response = await axiosClient.get(`/media/validate-url?url=${encodeURIComponent(url)}`);
      return response.data;
    } catch (error) {
      console.error("Error al validar URL:", error);
      return { valid: false, message: "Error al validar URL" };
    }
  },

  // Búsqueda avanzada
  searchPropiedades: async (searchParams) => {
    set({ loading: true, error: null });
    try {
      const response = await axiosClient.post("/propiedades/buscar-avanzada", searchParams);
      const { Data, TotalCount, TotalPaginas, Pagina, TamanoPagina } = response.data;

      set({
        propiedades: Data,
        paginacion: {
          totalCount: TotalCount,
          totalPages: TotalPaginas,
          currentPage: Pagina,
          pageSize: TamanoPagina
        },
        loading: false
      });

      return response.data;
    } catch (error) {
      console.error("Error en búsqueda avanzada:", error);
      set({ error: error.response?.data?.message || "Error en búsqueda", loading: false });
      toast.error("Error en la búsqueda");
      throw error;
    }
  },

  // Limpiar estado
  clearError: () => set({ error: null }),
  clearPropiedadActual: () => set({ propiedadActual: null }),
  clearMediasPropiedad: () => set({ mediasPropiedad: [] })
}));
