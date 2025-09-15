'use client';

import React, { useState, useEffect, useMemo } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import { 
  UserIcon, 
  ClockIcon, 
  ExclamationTriangleIcon,
  XMarkIcon,
  CalendarIcon 
} from '@heroicons/react/24/outline';
import { useVisitasStore } from '../../store/visitasStore';
import { usePropiedadesStore } from '../../store/propiedadesStore';
import { visitaCreateSchema } from '../../schemas/visitaSchemas';
import { axiosClient } from '../../lib/axiosClient';
import { toast } from 'react-hot-toast';

interface VisitaFormData {
  propiedadId: number;
  agenteId: number;
  clienteNombre: string;
  clienteTelefono?: string;
  clienteEmail?: string;
  fechaHora: Date;
  duracionMinutos: number;
  observaciones?: string;
}

interface VisitaFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  fechaInicial?: Date;
  agenteInicial?: number;
  visitaId?: number; // Para edición
}

interface Propiedad {
  id: number;
  codigo: string;
  tipo: string;
  direccion: string;
  barrio: string;
  precio: number;
  moneda: string;
}

interface Lead {
  id: number;
  nombre: string;
  email: string;
  telefono?: string;
  propiedadId: number;
}

export default function VisitaForm({ 
  isOpen, 
  onClose, 
  onSuccess, 
  fechaInicial, 
  agenteInicial,
  visitaId 
}: VisitaFormProps) {
  const [propiedadSeleccionada, setPropiedadSeleccionada] = useState<Propiedad | null>(null);
  const [leads, setLeads] = useState<Lead[]>([]);
  const [leadSeleccionado, setLeadSeleccionado] = useState<Lead | null>(null);
  const [mostrarFormularioCliente, setMostrarFormularioCliente] = useState(true);
  const [conflictoDetectado, setConflictoDetectado] = useState(false);
  const [validandoConflicto, setValidandoConflicto] = useState(false);
  const [ultimaValidacion, setUltimaValidacion] = useState<string>('');
  const [submitting, setSubmitting] = useState(false);

  // CORRECCIÓN: Usar selectores individuales para evitar loops
  const crearVisita = useVisitasStore(state => state.crearVisita);
  const actualizarVisita = useVisitasStore(state => state.actualizarVisita);
  const obtenerVisita = useVisitasStore(state => state.obtenerVisita);
  const validarConflicto = useVisitasStore(state => state.validarConflicto);
  const agentes = useVisitasStore(state => state.agentes);
  const loading = useVisitasStore(state => state.loading);
  const setLoading = useVisitasStore(state => state.setLoading);

  // Store de propiedades
  const { 
    propiedades, 
    fetchPropiedades 
  } = usePropiedadesStore();

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
    reset
  } = useForm<VisitaFormData>({
    resolver: zodResolver(visitaCreateSchema),
    defaultValues: {
      duracionMinutos: 60,
      fechaHora: fechaInicial || new Date(),
      agenteId: agenteInicial || 0
    }
  });

  const watchedValues = watch(['agenteId', 'fechaHora', 'duracionMinutos']);

  // CORRECCIÓN: Resetear loading al montar el componente
  useEffect(() => {
    console.log('🔧 Componente montado, reseteando estado loading...');
    setLoading(false);
  }, [setLoading]);

  // Memoizar las opciones de agentes para evitar re-renders innecesarios
  const agentesOptions = useMemo(() => {
    return agentes.map((agente: any) => (
      <option key={agente.id} value={agente.id}>
        {agente.nombre}
      </option>
    ));
  }, [agentes]);

  // Cargar datos iniciales
  useEffect(() => {
    if (isOpen) {
      // CORRECCIÓN: Resetear loading INMEDIATAMENTE al abrir el modal
      const state = useVisitasStore.getState();
      console.log('🔍 Estado loading antes del reset:', state.loading);
      state.setLoading(false);
      state.clearError();
      console.log('🔍 Estado loading después del reset:', useVisitasStore.getState().loading);
      
      console.log('VisitaForm: Cargando datos iniciales...');
      console.log('🔍 Estado inicial del formulario:', {
        errors,
        loading: false, // Forzamos que se vea como false
        conflictoDetectado,
        validandoConflicto,
        agentesLength: agentes.length,
        propiedadesLength: propiedades.length
      });
      
      console.log('Agentes actuales en store:', state.agentes);

      state.cargarAgentes().then(() => {
        console.log('Agentes después de cargar:', useVisitasStore.getState().agentes);
      });

      // Cargar propiedades sin filtros para obtener todas
      fetchPropiedades({ estado: 'Activo', page: 1, pageSize: 1000 });

      if (visitaId) {
        cargarVisitaParaEdicion();
      } else {
        reset({
          duracionMinutos: 60,
          fechaHora: fechaInicial || new Date(),
          agenteId: agenteInicial || 0
        });
      }
    }
  }, [isOpen, visitaId, fechaInicial, agenteInicial]);

  // Cargar visita para edición
  const cargarVisitaParaEdicion = async () => {
    if (!visitaId) return;
    
    try {
      const visita = await obtenerVisita(visitaId);
      reset({
        propiedadId: visita.propiedadId,
        agenteId: visita.agenteId,
        clienteNombre: visita.clienteNombre,
        clienteTelefono: visita.clienteTelefono,
        clienteEmail: visita.clienteEmail,
        fechaHora: new Date(visita.fechaHora),
        duracionMinutos: visita.duracionMinutos,
        observaciones: visita.observaciones
      });

      // Seleccionar propiedad actual
      await seleccionarPropiedad(visita.propiedadId);
    } catch (error) {
      toast.error('Error al cargar la visita');
    }
  };

  // Seleccionar propiedad
  const seleccionarPropiedad = async (propiedadId: number) => {
    const propiedad = propiedades.find((p: any) => p.id === propiedadId);
    if (!propiedad) return;

    setPropiedadSeleccionada(propiedad);
    setValue('propiedadId', propiedad.id);

    // Cargar leads de esta propiedad
    try {
      const response = await axiosClient.get(`/lead/propiedad/${propiedad.id}`);
      setLeads(response.data || []);
    } catch (error) {
      console.error('Error cargando leads:', error);
      setLeads([]);
    }
  };

  // Seleccionar lead existente
  const seleccionarLead = (lead: Lead) => {
    setLeadSeleccionado(lead);
    setValue('clienteNombre', lead.nombre);
    setValue('clienteEmail', lead.email);
    setValue('clienteTelefono', lead.telefono || '');
    setMostrarFormularioCliente(false);
  };

  // Validar conflictos de horario
  useEffect(() => {
    const validarHorario = async () => {
      const [agenteId, fechaHora, duracionMinutos] = watchedValues;

      // Solo validar si todos los campos necesarios están presentes y son válidos
      if (!agenteId || agenteId === 0 || !fechaHora || !duracionMinutos) {
        setConflictoDetectado(false);
        setValidandoConflicto(false);
        return;
      }

      // Crear una clave única para esta validación
      const claveValidacion = `${agenteId}-${fechaHora?.getTime()}-${duracionMinutos}-${visitaId || 'new'}`;

      // Evitar validaciones duplicadas
      if (claveValidacion === ultimaValidacion) {
        return;
      }

      setUltimaValidacion(claveValidacion);
      setValidandoConflicto(true);
      setConflictoDetectado(false);

      try {
        const tieneConflicto = await validarConflicto(
          agenteId,
          fechaHora,
          duracionMinutos,
          visitaId
        );
        setConflictoDetectado(tieneConflicto);
      } catch (error) {
        console.error('Error validando conflicto:', error);
        // En caso de error en la validación, no bloquear la creación
        setConflictoDetectado(false);
      } finally {
        setValidandoConflicto(false);
      }
    };

    // Aumentar el timeout para reducir llamadas frecuentes
    const timeoutId = setTimeout(validarHorario, 1000);
    return () => {
      clearTimeout(timeoutId);
    };
  }, [watchedValues, visitaId]);

  // Enviar formulario
  const onSubmit = async (data: VisitaFormData) => {
    console.log('📝 Enviando formulario con datos:', data);
    
    // Solo mostrar advertencia si hay conflicto, pero permitir continuar
    if (conflictoDetectado) {
      const confirmar = window.confirm(
        'Se detectó un posible conflicto de horario. ¿Deseas continuar de todas formas?'
      );
      if (!confirmar) {
        return;
      }
    }

    // Marcar como enviando
    setSubmitting(true);

    // Timeout de seguridad para evitar que se quede colgado
    const timeoutId = setTimeout(() => {
      console.error('⏰ Timeout: La operación está tardando demasiado');
      toast.error('La operación está tardando demasiado. Por favor, intenta de nuevo.');
      
      // Resetear estados
      setSubmitting(false);
      const state = useVisitasStore.getState();
      state.setLoading(false);
    }, 30000); // 30 segundos

    try {
      console.log('🚀 Iniciando creación/actualización de visita...');
      
      if (visitaId) {
        console.log('📝 Actualizando visita existente:', visitaId);
        await actualizarVisita(visitaId, data);
        console.log('✅ Visita actualizada correctamente');
        toast.success('Visita actualizada correctamente');
      } else {
        console.log('🆕 Creando nueva visita...');
        const resultado = await crearVisita(data);
        console.log('✅ Visita creada correctamente:', resultado);
        toast.success('Visita creada correctamente');
      }

      console.log('🎉 Proceso completado, cerrando modal...');
      onSuccess();
      onClose();
    } catch (error: any) {
      console.error('❌ Error al guardar visita:', error);
      console.error('❌ Error completo:', {
        message: error?.message,
        response: error?.response,
        status: error?.response?.status,
        data: error?.response?.data
      });
      
      // Log específico para errores de validación
      if (error?.response?.status === 400 && error?.response?.data?.errors) {
        console.error('❌ Errores de validación específicos:', error.response.data.errors);
        
        // Mostrar errores de validación específicos
        const validationErrors = Object.entries(error.response.data.errors)
          .map(([field, errors]) => `${field}: ${Array.isArray(errors) ? errors.join(', ') : errors}`)
          .join('\n');
        
        toast.error(`Errores de validación:\n${validationErrors}`);
      } else {
        const errorMsg = error?.response?.data?.message || error?.message || 'Error al guardar la visita';
        toast.error(errorMsg);
      }
    } finally {
      // Limpiar estados
      setSubmitting(false);
      clearTimeout(timeoutId);
    }
  };

  // Resetear loading cuando se cierre el modal
  useEffect(() => {
    if (!isOpen) {
      console.log('🔧 Modal cerrado, reseteando estados...');
      setLoading(false);
      setSubmitting(false);
    }
  }, [isOpen, setLoading]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b">
          <h2 className="text-xl font-semibold text-gray-900">
            {visitaId ? 'Editar Visita' : 'Nueva Visita'}
          </h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600"
          >
            <XMarkIcon className="w-6 h-6" />
          </button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-6">
          {/* Selector de Propiedad */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Propiedad *
            </label>
            <select
              {...register('propiedadId', { valueAsNumber: true })}
              onChange={(e) => {
                const propiedadId = parseInt(e.target.value);
                if (propiedadId) {
                  seleccionarPropiedad(propiedadId);
                }
              }}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            >
              <option value={0}>Seleccionar propiedad</option>
              {Array.isArray(propiedades) && propiedades.map((propiedad) => (
                <option key={propiedad.id} value={propiedad.id}>
                  {propiedad.codigo} - {propiedad.tipo} en {propiedad.barrio} (${propiedad.precio?.toLocaleString() || 0})
                </option>
              ))}
            </select>

            {errors.propiedadId && (
              <p className="mt-1 text-sm text-red-600">{errors.propiedadId.message}</p>
            )}

            {/* Información de propiedad seleccionada */}
            {propiedadSeleccionada && (
              <div className="mt-3 p-3 bg-blue-50 border border-blue-200 rounded-md">
                <div className="font-medium text-blue-900">{propiedadSeleccionada.codigo}</div>
                <div className="text-sm text-blue-700">{propiedadSeleccionada.tipo} en {propiedadSeleccionada.barrio}</div>
                <div className="text-sm text-blue-600">{propiedadSeleccionada.direccion}</div>
                <div className="text-sm font-medium text-blue-800 mt-1">
                  {propiedadSeleccionada.moneda} {propiedadSeleccionada.precio.toLocaleString()}
                </div>
              </div>
            )}
          </div>

          {/* Selección de Lead o Cliente Nuevo */}
          {leads.length > 0 && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Cliente
              </label>
              <div className="flex space-x-2 mb-3">
                <button
                  type="button"
                  onClick={() => {
                    setMostrarFormularioCliente(false);
                    if (leadSeleccionado) seleccionarLead(leadSeleccionado);
                  }}
                  className={`px-3 py-1 text-sm rounded-md ${
                    !mostrarFormularioCliente 
                      ? 'bg-blue-100 text-blue-700' 
                      : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                  }`}
                >
                  Lead Existente
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setMostrarFormularioCliente(true);
                    setLeadSeleccionado(null);
                    setValue('clienteNombre', '');
                    setValue('clienteEmail', '');
                    setValue('clienteTelefono', '');
                  }}
                  className={`px-3 py-1 text-sm rounded-md ${
                    mostrarFormularioCliente 
                      ? 'bg-blue-100 text-blue-700' 
                      : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                  }`}
                >
                  Cliente Nuevo
                </button>
              </div>

              {/* Lista de leads */}
              {!mostrarFormularioCliente && (
                <div className="space-y-2 max-h-32 overflow-y-auto">
                  {leads.map((lead) => (
                    <button
                      key={lead.id}
                      type="button"
                      onClick={() => seleccionarLead(lead)}
                      className={`w-full p-3 text-left border rounded-md hover:bg-gray-50 ${
                        leadSeleccionado?.id === lead.id 
                          ? 'border-blue-300 bg-blue-50' 
                          : 'border-gray-200'
                      }`}
                    >
                      <div className="font-medium">{lead.nombre}</div>
                      <div className="text-sm text-gray-600">{lead.email}</div>
                      {lead.telefono && (
                        <div className="text-sm text-gray-500">{lead.telefono}</div>
                      )}
                    </button>
                  ))}
                </div>
              )}
            </div>
          )}

          {/* Formulario de Cliente */}
          {(mostrarFormularioCliente || leads.length === 0) && (
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Nombre del Cliente *
                </label>
                <div className="relative">
                  <UserIcon className="absolute left-3 top-3 w-5 h-5 text-gray-400" />
                  <input
                    {...register('clienteNombre')}
                    type="text"
                    className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                    placeholder="Nombre completo"
                  />
                </div>
                {errors.clienteNombre && (
                  <p className="mt-1 text-sm text-red-600">{errors.clienteNombre.message}</p>
                )}
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Teléfono
                  </label>
                  <input
                    {...register('clienteTelefono')}
                    type="tel"
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                    placeholder="+54 11 1234-5678"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Email
                  </label>
                  <input
                    {...register('clienteEmail')}
                    type="email"
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                    placeholder="email@ejemplo.com"
                  />
                  {errors.clienteEmail && (
                    <p className="mt-1 text-sm text-red-600">{errors.clienteEmail.message}</p>
                  )}
                </div>
              </div>
            </div>
          )}

          {/* Fecha, Hora y Agente */}
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Fecha y Hora *
              </label>
              <div className="relative">
                <CalendarIcon className="absolute left-3 top-3 w-5 h-5 text-gray-400" />
                <input
                  {...register('fechaHora', { valueAsDate: true })}
                  type="datetime-local"
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                />
              </div>
              {errors.fechaHora && (
                <p className="mt-1 text-sm text-red-600">{errors.fechaHora.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Duración (min) *
              </label>
              <div className="relative">
                <ClockIcon className="absolute left-3 top-3 w-5 h-5 text-gray-400" />
                <select
                  {...register('duracionMinutos', { valueAsNumber: true })}
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                >
                  <option value={30}>30 min</option>
                  <option value={45}>45 min</option>
                  <option value={60}>1 hora</option>
                  <option value={90}>1.5 horas</option>
                  <option value={120}>2 horas</option>
                </select>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Agente *
              </label>
              <select
                {...register('agenteId', { valueAsNumber: true })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
              >
                <option value={0}>Seleccionar agente</option>
                {agentesOptions}
              </select>
              {errors.agenteId && (
                <p className="mt-1 text-sm text-red-600">{errors.agenteId.message}</p>
              )}
            </div>
          </div>

          {/* Alerta de conflicto */}
          {(conflictoDetectado || validandoConflicto) && (
            <div className={`p-4 rounded-md flex items-start space-x-3 ${
              conflictoDetectado ? 'bg-red-50 border border-red-200' : 'bg-yellow-50 border border-yellow-200'
            }`}>
              <ExclamationTriangleIcon className={`w-5 h-5 mt-0.5 ${
                conflictoDetectado ? 'text-red-400' : 'text-yellow-400'
              }`} />
              <div className="text-sm">
                {validandoConflicto ? (
                  <span className="text-yellow-700">Validando disponibilidad...</span>
                ) : (
                  <span className="text-red-700">
                    ⚠️ Conflicto detectado: El agente ya tiene una visita programada en este horario.
                  </span>
                )}
              </div>
            </div>
          )}

          {/* Observaciones */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Observaciones
            </label>
            <textarea
              {...register('observaciones')}
              rows={3}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
              placeholder="Notas adicionales sobre la visita..."
            />
          </div>

          {/* Botones */}
          <div className="flex justify-end space-x-3 pt-4 border-t">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={submitting}
              onClick={() => console.log('🔘 Botón clickeado - Estado submitting:', submitting, 'loading:', loading)}
              className={`px-4 py-2 text-sm font-medium text-white rounded-md ${
                submitting
                  ? 'bg-gray-400 cursor-not-allowed'
                  : conflictoDetectado
                  ? 'bg-orange-600 hover:bg-orange-700'
                  : 'bg-blue-600 hover:bg-blue-700'
              }`}
            >
              {submitting ? 'Guardando...' : (conflictoDetectado ? '⚠️ Crear con Conflicto' : (visitaId ? 'Actualizar' : 'Crear Visita'))}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
