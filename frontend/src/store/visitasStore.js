import { create } from 'zustand';
import { axiosClient } from '../lib/axiosClient';

export const useVisitasStore = create((set, get) => ({
  // Estado
  visitas: [],
  visitaActual: null,
  agentes: [],
  loading: false,
  error: null,
  filtros: {
    agenteId: null,
    estado: '',
    fechaDesde: null,
    fechaHasta: null,
    page: 1,
    pageSize: 20
  },
  totalCount: 0,
  totalPages: 0,

  // Acciones básicas
  setLoading: (loading) => set({ loading }),
  setError: (error) => set({ error }),
  clearError: () => set({ error: null }),

  // Gestión de filtros
  setFiltros: (filtros) => set((state) => ({
    filtros: { ...state.filtros, ...filtros, page: 1 }
  })),
  
  resetFiltros: () => set({
    filtros: {
      agenteId: null,
      estado: '',
      fechaDesde: null,
      fechaHasta: null,
      page: 1,
      pageSize: 20
    }
  }),

  // Cargar visitas
  cargarVisitas: async () => {
    set({ loading: true, error: null });
    try {
      const { filtros } = get();
      const params = new URLSearchParams();
      
      // Para cargar todas las visitas, usamos un rango de fechas amplio si no está especificado
      if (!filtros.fechaDesde) {
        const haceUnMes = new Date();
        haceUnMes.setMonth(haceUnMes.getMonth() - 1);
        params.append('fechaDesde', haceUnMes.toISOString());
      }
      
      if (!filtros.fechaHasta) {
        const enTresMeses = new Date();
        enTresMeses.setMonth(enTresMeses.getMonth() + 3);
        params.append('fechaHasta', enTresMeses.toISOString());
      }
      
      Object.entries(filtros).forEach(([key, value]) => {
        if (value !== null && value !== '' && value !== undefined) {
          if (value instanceof Date) {
            params.append(key, value.toISOString());
          } else if (key === 'agenteId' && value) {
            params.append(key, value.toString());
          }
        }
      });

      // Usar el endpoint de calendar que funciona
      const response = await axiosClient.get(`/visita/calendar?${params}`);
      
      // Transformar datos del formato calendar al formato esperado por el componente
      const visitasTransformadas = (response.data || []).map(visita => {
        // Extraer información del description
        const lines = visita.description?.split('\n') || [];
        const clienteLine = lines.find(line => line.startsWith('Cliente:'));
        const propiedadLine = lines.find(line => line.startsWith('Propiedad:'));
        const agenteLine = lines.find(line => line.startsWith('Agente:'));
        
        // Extraer código de propiedad del title
        const codigoMatch = visita.title?.match(/- (\d+)$/);
        
        return {
          id: visita.id,
          propiedadCodigo: codigoMatch ? codigoMatch[1] : 'N/A',
          propiedadDireccion: propiedadLine ? propiedadLine.replace('Propiedad: ', '') : 'N/A',
          clienteNombre: clienteLine ? clienteLine.replace('Cliente: ', '') : 'N/A',
          clienteTelefono: '',
          clienteEmail: '',
          agenteNombre: agenteLine ? agenteLine.replace('Agente: ', '') : 'N/A',
          fechaHora: visita.start,
          duracionMinutos: visita.end && visita.start ? 
            Math.round((new Date(visita.end) - new Date(visita.start)) / (1000 * 60)) : 60,
          estado: visita.estado || 'Pendiente',
          observaciones: '',
          fechaCreacion: visita.start
        };
      });
      
      set({
        visitas: visitasTransformadas,
        totalCount: visitasTransformadas.length,
        totalPages: 1,
        loading: false
      });
    } catch (error) {
      console.error('Error cargando visitas:', error);
      set({ 
        error: error.response?.data?.message || 'Error al cargar visitas',
        loading: false
      });
    }
  },

  // Cargar visitas para calendario
  cargarVisitasCalendario: async (agenteId = null, fechaDesde = null, fechaHasta = null) => {
    console.log('🔄 Iniciando cargarVisitasCalendario...', { agenteId, fechaDesde, fechaHasta });
    set({ loading: true, error: null });
    try {
      const params = new URLSearchParams();
      if (agenteId) params.append('agenteId', agenteId);
      if (fechaDesde) params.append('fechaDesde', fechaDesde.toISOString());
      if (fechaHasta) params.append('fechaHasta', fechaHasta.toISOString());

      const url = `/visita/calendar?${params}`;
      console.log('📡 Haciendo llamada a:', url);
      const response = await axiosClient.get(url);
      console.log('✅ Respuesta recibida:', response.data);
      const raw = response.data || [];

      // Normalizar claves (Start/End/Estado -> start/end/estado) y asegurar strings ISO
      const normalized = raw.map((v) => {
        const start = v.start || v.Start || v.fechaInicio || v.FechaInicio;
        const end = v.end || v.End || v.fechaFin || v.FechaFin;
        return {
          id: v.id || v.Id,
          title: v.title || v.Title || '',
          start: typeof start === 'string' ? start : (start instanceof Date ? start.toISOString() : ''),
          end: typeof end === 'string' ? end : (end instanceof Date ? end.toISOString() : ''),
          color: v.color || v.Color || '#007bff',
          estado: v.estado || v.Estado || 'Pendiente',
          description: v.description || v.Description,
          propiedadCodigo: v.propiedadCodigo || v.PropiedadCodigo,
          clienteNombre: v.clienteNombre || v.ClienteNombre,
          agenteId: v.agenteId || v.AgenteId,
          agenteNombre: v.agenteNombre || v.AgenteNombre,
        };
      });

      set({ loading: false });
      return normalized;
    } catch (error) {
      set({ 
        error: error.response?.data?.message || 'Error al cargar calendario',
        loading: false
      });
      return [];
    }
  },

  // Obtener visita por ID
  obtenerVisita: async (id) => {
    set({ loading: true, error: null });
    try {
      const response = await axiosClient.get(`/visita/${id}`);
      set({ visitaActual: response.data, loading: false });
      return response.data;
    } catch (error) {
      set({ 
        error: error.response?.data?.message || 'Error al obtener visita',
        loading: false
      });
      throw error;
    }
  },

  // Crear visita
  crearVisita: async (visitaData) => {
    console.log('🔧 Store: Iniciando crearVisita con datos:', visitaData);
    set({ loading: true, error: null });
    try {
      // Transformar datos a PascalCase para el backend
      const dataForBackend = {
        PropiedadId: visitaData.propiedadId,
        AgenteId: visitaData.agenteId,
        ClienteNombre: visitaData.clienteNombre,
        ClienteTelefono: visitaData.clienteTelefono || null,
        ClienteEmail: visitaData.clienteEmail || null,
        FechaHora: visitaData.fechaHora instanceof Date 
          ? visitaData.fechaHora.toISOString() 
          : visitaData.fechaHora,
        DuracionMinutos: visitaData.duracionMinutos,
        Observaciones: visitaData.observaciones || null
      };

      console.log('🌐 Store: Enviando datos al backend:', dataForBackend);
      const response = await axiosClient.post('/visita', dataForBackend);
      console.log('📨 Store: Respuesta del backend:', response.data);
      
      // Actualizar lista local
      set((state) => ({
        visitas: [response.data, ...state.visitas],
        loading: false
      }));

      console.log('✅ Store: Visita creada exitosamente');
      return response.data;
    } catch (error) {
      console.error('❌ Store: Error en crearVisita:', error);
      console.error('❌ Store: Error completo:', {
        message: error?.message,
        response: error?.response,
        status: error?.response?.status,
        data: error?.response?.data
      });
      
      // Log específico para errores de validación
      if (error?.response?.status === 400 && error?.response?.data?.errors) {
        console.error('❌ Errores de validación específicos:', error.response.data.errors);
      }
      
      set({ 
        error: error.response?.data?.message || 'Error al crear visita',
        loading: false
      });
      throw error;
    }
  },

  // Actualizar visita
  actualizarVisita: async (id, visitaData) => {
    set({ loading: true, error: null });
    try {
      // Transformar datos a PascalCase para el backend
      const dataForBackend = {
        Id: id,
        PropiedadId: visitaData.propiedadId,
        AgenteId: visitaData.agenteId,
        ClienteNombre: visitaData.clienteNombre,
        ClienteTelefono: visitaData.clienteTelefono,
        ClienteEmail: visitaData.clienteEmail,
        FechaHora: visitaData.fechaHora,
        DuracionMinutos: visitaData.duracionMinutos,
        Observaciones: visitaData.observaciones,
        Estado: visitaData.estado,
        NotasVisita: visitaData.notasVisita
      };

      const response = await axiosClient.put(`/visita/${id}`, dataForBackend);
      
      // Actualizar lista local
      set((state) => ({
        visitas: state.visitas.map(v => v.id === id ? response.data : v),
        visitaActual: state.visitaActual?.id === id ? response.data : state.visitaActual,
        loading: false
      }));

      return response.data;
    } catch (error) {
      set({ 
        error: error.response?.data?.message || 'Error al actualizar visita',
        loading: false
      });
      throw error;
    }
  },

  // Eliminar visita
  eliminarVisita: async (id) => {
    set({ loading: true, error: null });
    try {
      await axiosClient.delete(`/visita/${id}`);
      
      // Actualizar lista local
      set((state) => ({
        visitas: state.visitas.filter(v => v.id !== id),
        loading: false
      }));
    } catch (error) {
      set({ 
        error: error.response?.data?.message || 'Error al eliminar visita',
        loading: false
      });
      throw error;
    }
  },

  // Acciones de estado
  confirmarVisita: async (id) => {
    set({ loading: true, error: null });
    try {
      const response = await axiosClient.patch(`/visita/${id}/confirmar`);
      
      // Actualizar lista local
      set((state) => ({
        visitas: state.visitas.map(v => v.id === id ? response.data : v),
        loading: false
      }));

      return response.data;
    } catch (error) {
      set({ 
        error: error.response?.data?.message || 'Error al confirmar visita',
        loading: false
      });
      throw error;
    }
  },

  cancelarVisita: async (id, motivo) => {
    set({ loading: true, error: null });
    try {
      const response = await axiosClient.patch(`/visita/${id}/cancelar`, motivo);
      
      // Actualizar lista local
      set((state) => ({
        visitas: state.visitas.map(v => v.id === id ? response.data : v),
        loading: false
      }));

      return response.data;
    } catch (error) {
      set({ 
        error: error.response?.data?.message || 'Error al cancelar visita',
        loading: false
      });
      throw error;
    }
  },

  reagendarVisita: async (id, nuevaFecha) => {
    set({ loading: true, error: null });
    try {
      const response = await axiosClient.patch(`/visita/${id}/reagendar`, (nuevaFecha instanceof Date ? nuevaFecha.toISOString() : nuevaFecha));
      
      // Actualizar lista local
      set((state) => ({
        visitas: state.visitas.map(v => v.id === id ? response.data : v),
        loading: false
      }));

      return response.data;
    } catch (error) {
      set({ 
        error: error.response?.data?.message || 'Error al reagendar visita',
        loading: false
      });
      throw error;
    }
  },

  marcarRealizada: async (id, notas = '') => {
    set({ loading: true, error: null });
    try {
      const response = await axiosClient.patch(`/visita/${id}/realizada`, notas);
      
      // Actualizar lista local
      set((state) => ({
        visitas: state.visitas.map(v => v.id === id ? response.data : v),
        loading: false
      }));

      return response.data;
    } catch (error) {
      set({ 
        error: error.response?.data?.message || 'Error al marcar como realizada',
        loading: false
      });
      throw error;
    }
  },

  // Acciones masivas
  accionMasiva: async (visitaIds, accion, options = {}) => {
    set({ loading: true, error: null });
    try {
      const payload = {
        visitaIds,
        accion,
        ...options
      };

      await axiosClient.post('/visita/bulk-action', payload);
      
      // Recargar visitas
      get().cargarVisitas();
    } catch (error) {
      set({ 
        error: error.response?.data?.message || 'Error en acción masiva',
        loading: false
      });
      throw error;
    }
  },

  // Validar conflictos
  validarConflicto: async (agenteId, fechaHora, duracionMinutos, visitaIdExcluir = null) => {
    try {
      const response = await axiosClient.post('/visita/check-conflict', {
        AgenteId: agenteId,
        FechaHora: fechaHora,
        DuracionMinutos: duracionMinutos,
        VisitaIdExcluir: visitaIdExcluir
      });
      return response.data.hasConflict;
    } catch (error) {
      console.error('Error validando conflicto:', error);
      return false;
    }
  },

  // Obtener slots disponibles
  obtenerSlotsDisponibles: async (agenteId, fecha, duracionMinutos = 60) => {
    try {
      const response = await axiosClient.get(`/visita/available-slots`, {
        params: { agenteId, fecha: fecha.toISOString(), duracionMinutos }
      });
      return response.data;
    } catch (error) {
      console.error('Error obteniendo slots:', error);
      return [];
    }
  },

  // Cargar agentes
  cargarAgentes: async () => {
    console.log('🔄 Iniciando cargarAgentes...');
    try {
      console.log('📡 Haciendo llamada a /usuarios/agentes');
      const response = await axiosClient.get('/usuarios/agentes');
      console.log('✅ Respuesta recibida:', response.data);
      set({ agentes: response.data });
      console.log('✅ Agentes guardados en estado');
    } catch (error) {
      console.error('❌ Error cargando agentes:', error);
      set({ agentes: [] });
    }
  },

  // Descargar ICS
  descargarICS: async (visitaId) => {
    try {
      const response = await axiosClient.get(`/visita/${visitaId}/ics`, {
        responseType: 'blob'
      });
      
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `visita-${visitaId}.ics`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      set({ error: 'Error al descargar archivo ICS' });
    }
  },

  // Enviar notificación
  enviarNotificacion: async (visitaId, tipo = 'Email') => {
    try {
      await axiosClient.post(`/visita/${visitaId}/notification`, null, {
        params: { tipo }
      });
    } catch (error) {
      set({ error: 'Error al enviar notificación' });
      throw error;
    }
  }
}));
