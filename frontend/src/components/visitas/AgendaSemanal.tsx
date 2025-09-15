'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { format, startOfWeek, addDays, addWeeks, subWeeks, isSameDay, parseISO } from 'date-fns';
import { es } from 'date-fns/locale';
import { ChevronLeftIcon, ChevronRightIcon, CalendarIcon, ClockIcon } from '@heroicons/react/24/outline';
import { useVisitasStore } from '../../store/visitasStore';
import { useAuthStore } from '../../store/authStore';
import { toast } from 'react-hot-toast';

interface VisitaCalendar {
  id: number;
  title: string;
  start: string;
  end: string;
  color: string;
  estado: string;
  description?: string;
  propiedadCodigo?: string;
  clienteNombre?: string;
  agenteId?: number;
  agenteNombre?: string;
}

interface AgendaSemanalProps {
  agenteSeleccionado?: number | null;
  onEditarVisita: (visitaId: number) => void;
  onNuevaVisita: (fecha: Date, agenteId?: number) => void;
}

const HORAS_TRABAJO = Array.from({ length: 11 }, (_, i) => i + 8); // 8 AM a 6 PM
const DIAS_SEMANA = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];

const COLORES_ESTADO = {
  'Pendiente': 'bg-yellow-100 text-yellow-800 border-yellow-200',
  'Confirmada': 'bg-blue-100 text-blue-800 border-blue-200',
  'Realizada': 'bg-green-100 text-green-800 border-green-200',
  'Cancelada': 'bg-red-100 text-red-800 border-red-200'
};

const COLORES_AGENTE = [
  'bg-blue-200 text-blue-900 border-blue-300',
  'bg-green-200 text-green-900 border-green-300',
  'bg-purple-200 text-purple-900 border-purple-300',
  'bg-pink-200 text-pink-900 border-pink-300',
  'bg-indigo-200 text-indigo-900 border-indigo-300',
  'bg-red-200 text-red-900 border-red-300',
  'bg-orange-200 text-orange-900 border-orange-300',
  'bg-teal-200 text-teal-900 border-teal-300'
];

export default function AgendaSemanal({ agenteSeleccionado, onEditarVisita, onNuevaVisita }: AgendaSemanalProps) {
  const [semanaActual, setSemanaActual] = useState(new Date());
  const [visitasCalendario, setVisitasCalendario] = useState<VisitaCalendar[]>([]);
  const [visitaArrastrada, setVisitaArrastrada] = useState<VisitaCalendar | null>(null);
  const [agentes, setAgentes] = useState<any[]>([]);
  const [vistaCalendario, setVistaCalendario] = useState<'dia' | 'semana'>('semana');

  // Seleccionar funciones/estado de forma aislada para evitar re-renders en bucle
  const cargarVisitasCalendario = useVisitasStore((s) => s.cargarVisitasCalendario);
  const reagendarVisita = useVisitasStore((s) => s.reagendarVisita);
  const cargarAgentesStore = useVisitasStore((s) => s.cargarAgentes);
  const loading = useVisitasStore((s) => s.loading);

  // Auth state
  const { isAuthenticated, isInitialized } = useAuthStore();

  // Calcular fechas según la vista
  const inicioSemana = startOfWeek(semanaActual, { weekStartsOn: 0 });
  const diasSemana = vistaCalendario === 'dia'
    ? [semanaActual]
    : Array.from({ length: 7 }, (_, i) => addDays(inicioSemana, i));

  // Cargar agentes al montar el componente
  useEffect(() => {
    console.log('🔍 AgendaSemanal useEffect (agentes):', { isInitialized, isAuthenticated });
    if (!isInitialized || !isAuthenticated) return;

    let mounted = true;
    const loadAgentes = async () => {
      try {
        console.log('✅ Cargando agentes en AgendaSemanal...');
        await cargarAgentesStore();
        if (mounted) setAgentes(useVisitasStore.getState().agentes);
      } catch (error) {
        console.error('❌ Error cargando agentes:', error);
      }
    };
    loadAgentes();
    return () => { mounted = false; };
  }, [cargarAgentesStore, isInitialized, isAuthenticated]);

  // Cargar visitas del calendario
  useEffect(() => {
    console.log('🔍 AgendaSemanal useEffect (visitas):', { isInitialized, isAuthenticated, semanaActual, agenteSeleccionado });
    if (!isInitialized || !isAuthenticated) return;

    let mounted = true;

    const loadVisitas = async () => {
      try {
        const fechaDesde = startOfWeek(semanaActual, { weekStartsOn: 0 });
        const fechaHasta = addDays(fechaDesde, 6);
        console.log('✅ Cargando visitas del calendario:', { fechaDesde, fechaHasta, agenteSeleccionado });

        const visitas = await cargarVisitasCalendario(agenteSeleccionado, fechaDesde, fechaHasta);
        console.log('✅ Visitas cargadas:', visitas);
        if (mounted) {
          setVisitasCalendario(visitas);
        }
      } catch (error) {
        console.error('❌ Error al cargar visitas del calendario:', error);
        if (mounted) {
          toast.error('Error al cargar visitas del calendario');
        }
      }
    };

    loadVisitas();
    return () => { mounted = false; };
  }, [semanaActual, agenteSeleccionado, cargarVisitasCalendario, isInitialized, isAuthenticated]);

  // Navegación de semanas
  const semanaAnterior = () => setSemanaActual(subWeeks(semanaActual, 1));
  const semanaSiguiente = () => setSemanaActual(addWeeks(semanaActual, 1));
  const irHoy = () => setSemanaActual(new Date());

  // Obtener visitas para un día específico
  const obtenerVisitasDia = (fecha: Date): VisitaCalendar[] => {
    return visitasCalendario.filter(visita => {
      if (!visita.start) return false;
      const fechaVisita = parseISO(visita.start);
      return isSameDay(fechaVisita, fecha);
    });
  };

  // Calcular posición y altura de la visita en la grilla
  const calcularPosicionVisita = (visita: VisitaCalendar) => {
    const inicio = visita.start ? parseISO(visita.start) : new Date();
    const fin = visita.end ? parseISO(visita.end) : new Date(inicio.getTime() + 60 * 60000);

    const horaInicio = inicio.getHours() + inicio.getMinutes() / 60;
    const horaFin = fin.getHours() + fin.getMinutes() / 60;

    const top = ((horaInicio - 8) * 60); // 60px por hora, empezando en 8 AM
    const height = ((horaFin - horaInicio) * 60);

    return { top, height };
  };

  // Drag & Drop handlers
  const handleDragStart = (e: React.DragEvent, visita: VisitaCalendar) => {
    setVisitaArrastrada(visita);
    e.dataTransfer.effectAllowed = 'move';
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
  };

  const handleDrop = async (e: React.DragEvent, fecha: Date, hora: number) => {
    e.preventDefault();

    if (!visitaArrastrada) return;

    try {
      const nuevaFecha = new Date(fecha);
      nuevaFecha.setHours(hora, 0, 0, 0);

      // Validar antisolapeo
      const hayConflicto = await validarConflicto(visitaArrastrada.id, nuevaFecha);
      if (hayConflicto) {
        toast.error('Conflicto de horario: Ya existe una visita en ese horario');
        setVisitaArrastrada(null);
        return;
      }

      await reagendarVisita(visitaArrastrada.id, nuevaFecha);
      
      // Recargar calendario
      const fechaDesde = startOfWeek(semanaActual, { weekStartsOn: 0 });
      const fechaHasta = addDays(fechaDesde, 6);
      const visitas = await cargarVisitasCalendario(agenteSeleccionado, fechaDesde, fechaHasta);
      setVisitasCalendario(visitas);

      toast.success('Visita reagendada correctamente');
    } catch (error) {
      toast.error('Error al reagendar la visita');
    } finally {
      setVisitaArrastrada(null);
    }
  };

  // Validar conflictos de horario
  const validarConflicto = async (visitaId: number, nuevaFecha: Date): Promise<boolean> => {
    try {
      const { validarConflicto } = useVisitasStore.getState();
      
      // Obtener duración de la visita actual
      const visitaActual = visitasCalendario.find(v => v.id === visitaId);
      const duracion = visitaActual ? 
        Math.round((new Date(visitaActual.end) - new Date(visitaActual.start)) / (1000 * 60)) : 60;

      return await validarConflicto(
        visitaActual?.agenteId || agenteSeleccionado || 0,
        nuevaFecha,
        duracion,
        visitaId
      );
    } catch (error) {
      console.error('Error validando conflicto:', error);
      return false; // En caso de error, permitir el movimiento
    }
  };

  // Crear nueva visita
  const handleNuevaVisita = (fecha: Date, hora: number) => {
    const nuevaFecha = new Date(fecha);
    nuevaFecha.setHours(hora, 0, 0, 0);
    onNuevaVisita(nuevaFecha, agenteSeleccionado || undefined);
  };

  // Obtener color de la visita (por agente si es vista "todos", por estado si es agente específico)
  const obtenerColorVisita = (visita: VisitaCalendar) => {
    if (agenteSeleccionado === null) {
      // Vista "Todos los agentes" - colorear por agente
      const colorIndex = (visita.agenteId || 0) % COLORES_AGENTE.length;
      return COLORES_AGENTE[colorIndex];
    } else {
      // Vista de agente específico - colorear por estado
      return COLORES_ESTADO[visita.estado as keyof typeof COLORES_ESTADO] || COLORES_ESTADO['Pendiente'];
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-lg p-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center space-x-4">
          <h2 className="text-2xl font-bold text-gray-900">
            Agenda {vistaCalendario === 'dia' ? 'Diaria' : 'Semanal'}
          </h2>
          
          {/* Toggle Día/Semana */}
          <div className="flex items-center bg-gray-100 rounded-lg p-1">
            <button
              onClick={() => setVistaCalendario('dia')}
              className={`px-3 py-1 text-sm rounded-md transition-colors ${
                vistaCalendario === 'dia'
                  ? 'bg-white text-gray-900 shadow-sm'
                  : 'text-gray-600 hover:text-gray-900'
              }`}
            >
              Día
            </button>
            <button
              onClick={() => setVistaCalendario('semana')}
              className={`px-3 py-1 text-sm rounded-md transition-colors ${
                vistaCalendario === 'semana'
                  ? 'bg-white text-gray-900 shadow-sm'
                  : 'text-gray-600 hover:text-gray-900'
              }`}
            >
              Semana
            </button>
          </div>
          
          <div className="flex items-center space-x-2">
            <button
              onClick={semanaAnterior}
              className="p-2 rounded-md hover:bg-gray-100"
            >
              <ChevronLeftIcon className="w-5 h-5" />
            </button>
            <button
              onClick={irHoy}
              className="px-3 py-1 text-sm bg-blue-100 text-blue-700 rounded-md hover:bg-blue-200"
            >
              Hoy
            </button>
            <button
              onClick={semanaSiguiente}
              className="p-2 rounded-md hover:bg-gray-100"
            >
              <ChevronRightIcon className="w-5 h-5" />
            </button>
          </div>
        </div>

        <div className="flex items-center space-x-4">
          <span className="text-lg font-medium text-gray-700">
            {format(inicioSemana, 'd MMMM', { locale: es })} - {format(addDays(inicioSemana, 6), 'd MMMM yyyy', { locale: es })}
          </span>

          {/* Leyenda de colores */}
          <div className="flex items-center space-x-3 text-sm">
            {agenteSeleccionado === null ? (
              // Leyenda por agentes
              <span className="text-gray-600">
                Colores por agente {agentes.length > 0 && `(${agentes.length} agentes)`}
              </span>
            ) : (
              // Leyenda por estados
              Object.entries(COLORES_ESTADO).map(([estado, color]) => (
                <div key={estado} className="flex items-center space-x-1">
                  <div className={`w-3 h-3 rounded ${color.split(' ')[0]}`}></div>
                  <span>{estado}</span>
                </div>
              ))
            )}
          </div>
        </div>
      </div>

      {/* Grid del calendario */}
      <div className={`grid ${vistaCalendario === 'dia' ? 'grid-cols-2' : 'grid-cols-8'} gap-0 border border-gray-200 rounded-lg overflow-hidden`}>
        {/* Header de la grilla */}
        <div className="bg-gray-50 p-3 border-r border-gray-200 font-medium text-center">
          Hora
        </div>
        {diasSemana.map((dia, index) => (
          <div key={index} className="bg-gray-50 p-3 border-r border-gray-200 text-center">
            <div className="font-medium">{DIAS_SEMANA[index]}</div>
            <div className="text-lg font-bold text-gray-900">
              {format(dia, 'd', { locale: es })}
            </div>
            <div className="text-sm text-gray-500">
              {format(dia, 'MMM', { locale: es })}
            </div>
          </div>
        ))}

        {/* Filas de horas */}
        {HORAS_TRABAJO.map((hora) => (
          <React.Fragment key={hora}>
            {/* Columna de hora */}
            <div className="bg-gray-50 p-3 border-r border-t border-gray-200 text-center text-sm font-medium">
              {hora}:00
            </div>

            {/* Columnas de días */}
            {diasSemana.map((dia, diaIndex) => {
              const visitasDia = obtenerVisitasDia(dia);
              const visitasHora = visitasDia.filter(visita => {
                if (!visita.start) return false;
                const horaVisita = parseISO(visita.start).getHours();
                return horaVisita === hora;
              });

              return (
                <div
                  key={`${hora}-${diaIndex}`}
                  className="relative border-r border-t border-gray-200 h-16 hover:bg-gray-50 cursor-pointer"
                  onDragOver={handleDragOver}
                  onDrop={(e) => handleDrop(e, dia, hora)}
                  onClick={() => handleNuevaVisita(dia, hora)}
                >
                  {/* Visitas en esta hora */}
                  {visitasHora.map((visita, visitaIndex) => {
                    const { top, height } = calcularPosicionVisita(visita);

                    return (
                      <div
                        key={visita.id}
                        draggable
                        onDragStart={(e) => handleDragStart(e, visita)}
                        onClick={(e) => {
                          e.stopPropagation();
                          onEditarVisita(visita.id);
                        }}
                        className={`absolute left-1 right-1 rounded border cursor-move hover:shadow-md transition-shadow z-10 ${obtenerColorVisita(visita)
                          }`}
                        style={{
                          top: `${(top % 60)}px`,
                          height: `${Math.min(height, 60 - (top % 60))}px`,
                          left: `${visitaIndex * 20 + 4}px`,
                          right: `${4}px`,
                          minHeight: '20px'
                        }}
                      >
                        <div className="p-1 text-xs">
                          <div className="font-medium truncate">{visita.propiedadCodigo}</div>
                          <div className="truncate">{visita.clienteNombre}</div>
                          {agenteSeleccionado === null && (
                            <div className="truncate text-xs opacity-75">{visita.agenteNombre}</div>
                          )}
                          <div className="flex items-center space-x-1">
                            <ClockIcon className="w-3 h-3" />
                            <span>{format(parseISO(visita.start), 'HH:mm')}</span>
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              );
            })}
          </React.Fragment>
        ))}
      </div>

      {/* Instrucciones */}
      <div className="mt-4 text-sm text-gray-600">
        <p>• Haz clic en una celda vacía para crear una nueva visita</p>
        <p>• Arrastra las visitas para reagendarlas</p>
        <p>• Haz clic en una visita para editarla</p>
      </div>

      {loading && (
        <div className="absolute inset-0 bg-white bg-opacity-75 flex items-center justify-center">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
      )}
    </div>
  );
}
