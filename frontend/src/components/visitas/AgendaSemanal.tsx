"use client";

import React, { useState, useEffect, useRef } from "react";
import { format, startOfWeek, addDays, addWeeks, subWeeks } from "date-fns";
import { es } from "date-fns/locale";
import {
  ChevronLeftIcon,
  ChevronRightIcon,
  PlusIcon,
} from "@heroicons/react/24/outline";
import { useVisitasStore } from "../../store/visitasStore";
import { toast } from "react-hot-toast";

interface AgendaSemanalProps {
  agenteSeleccionado?: number | null;
  estadoFiltro?: string;
  onEditarVisita: (visitaId: number) => void;
  onNuevaVisita: (fecha: Date, agenteId?: number) => void;
  refreshTrigger?: number; // Prop para triggear refresh desde el padre
}

interface VisitaCalendar {
  id: number;
  title: string;
  start: string;
  end: string;
  estado: string;
  propiedadCodigo?: string;
  clienteNombre?: string;
  agenteNombre?: string;
}

const HORAS_TRABAJO = Array.from({ length: 11 }, (_, i) => i + 8); // 8 AM a 6 PM
const DIAS_SEMANA = ["Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb"];

const COLORES_ESTADO = {
  Pendiente: "bg-yellow-100 text-yellow-800 border-yellow-200",
  Confirmada: "bg-blue-100 text-blue-800 border-blue-200",
  Realizada: "bg-green-100 text-green-800 border-green-200",
  Cancelada: "bg-red-100 text-red-800 border-red-200",
};

// Función para normalizar el estado y obtener el color
const obtenerColorPorEstado = (estado: string): string => {
  // Normalizar el estado (quitar espacios, capitalizar primera letra)
  const estadoNormalizado = estado?.trim()?.toLowerCase();

  // Mapear diferentes variaciones de estado
  const mapeoEstados: { [key: string]: string } = {
    pendiente: "Pendiente",
    confirmada: "Confirmada",
    realizada: "Realizada",
    cancelada: "Cancelada",
    pending: "Pendiente",
    confirmed: "Confirmada",
    completed: "Realizada",
    cancelled: "Cancelada",
    canceled: "Cancelada",
  };

  const estadoFinal = mapeoEstados[estadoNormalizado] || estado || "Pendiente";
  return (
    COLORES_ESTADO[estadoFinal as keyof typeof COLORES_ESTADO] ||
    COLORES_ESTADO["Pendiente"]
  );
};

export default function AgendaSemanal({
  agenteSeleccionado,
  estadoFiltro,
  onEditarVisita,
  onNuevaVisita,
  refreshTrigger,
}: AgendaSemanalProps) {
  const [semanaActual, setSemanaActual] = useState(() => new Date());
  const [visitasCalendario, setVisitasCalendario] = useState<VisitaCalendar[]>(
    []
  );
  const [loading, setLoading] = useState(false);
  const lastRefreshRef = useRef(0);

  // Calcular fechas de la semana
  const inicioSemana = startOfWeek(semanaActual, { weekStartsOn: 0 });
  const diasSemana = Array.from({ length: 7 }, (_, i) =>
    addDays(inicioSemana, i)
  );

  // Carga inicial únicamente
  useEffect(() => {
    let isMounted = true;

    const cargarInicial = async () => {
      if (!isMounted) return;

      setLoading(true);
      try {
        console.log("📅 Carga inicial de agenda - mostrando agenda vacía");
        // Iniciar con agenda vacía - las visitas se cargarán solo cuando sea necesario
        setVisitasCalendario([]);
      } catch (error) {
        console.error("❌ Error en carga inicial:", error);
        if (isMounted) {
          setVisitasCalendario([]);
        }
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    };

    cargarInicial();

    return () => {
      isMounted = false;
    };
  }, []); // Solo se ejecuta una vez al montar

  // Temporal: Deshabilitamos el refresh automático para evitar loops
  // El usuario deberá hacer clic en "Cargar Visitas" después de crear una visita
  // useEffect(() => {
  //   if (refreshTrigger && refreshTrigger > lastRefreshRef.current) {
  //     console.log('🔄 Refresh automático deshabilitado temporalmente');
  //   }
  // }, [refreshTrigger]);

  // Actualizar filtros cuando cambien - solo recarga si ya hay visitas cargadas
  useEffect(() => {
    if (visitasCalendario.length > 0) {
      console.log("🔄 Filtros cambiaron, recargando agenda");
      cargarVisitasManual();
    }
  }, [agenteSeleccionado, estadoFiltro]);

  // Navegación de semanas
  const semanaAnterior = () => setSemanaActual(subWeeks(semanaActual, 1));
  const semanaSiguiente = () => setSemanaActual(addWeeks(semanaActual, 1));
  const irHoy = () => setSemanaActual(new Date());

  // Obtener visitas para un día específico
  const obtenerVisitasDia = (fecha: Date): VisitaCalendar[] => {
    return visitasCalendario.filter((visita: any) => {
      if (!visita.start) return false;
      try {
        const fechaVisita = new Date(visita.start);
        return fechaVisita.toDateString() === fecha.toDateString();
      } catch (error) {
        console.warn("Error parsing fecha visita:", visita.start);
        return false;
      }
    });
  };

  // Función para cargar visitas manualmente desde el store
  const cargarVisitasManual = async () => {
    setLoading(true);
    try {
      console.log("🔄 Cargando visitas desde el store para agenda", {
        agenteSeleccionado,
        estadoFiltro,
      });

      // Obtener visitas del store (las mismas que se muestran en Lista)
      const storeState = useVisitasStore.getState();
      let visitasStore = storeState.visitas;

      console.log("✅ Visitas del store (antes del filtro):", visitasStore);

      // Aplicar filtros
      if (agenteSeleccionado !== null) {
        // Filtrar por agente (necesitamos obtener el nombre del agente)
        const agentes = storeState.agentes;
        const agenteNombre = agentes.find(
          (a: any) => a.id === agenteSeleccionado
        )?.nombre;
        if (agenteNombre) {
          visitasStore = visitasStore.filter(
            (visita: any) => visita.agenteNombre === agenteNombre
          );
          console.log(
            "🔍 Filtrado por agente:",
            agenteNombre,
            "visitas:",
            visitasStore.length
          );
        }
      }

      if (estadoFiltro && estadoFiltro !== "") {
        visitasStore = visitasStore.filter(
          (visita: any) => visita.estado === estadoFiltro
        );
        console.log(
          "🔍 Filtrado por estado:",
          estadoFiltro,
          "visitas:",
          visitasStore.length
        );
      }

      if (visitasStore && visitasStore.length > 0) {
        // Convertir formato de store a formato de calendario
        const visitasCalendario = visitasStore.map((visita: any) => ({
          id: visita.id,
          title: `${visita.propiedadCodigo} - ${visita.clienteNombre}`,
          start: visita.fechaHora,
          end: visita.fechaHora, // Usamos la misma fecha como fin
          estado: visita.estado,
          propiedadCodigo: visita.propiedadCodigo,
          clienteNombre: visita.clienteNombre,
          agenteNombre: visita.agenteNombre,
        }));

        console.log(
          "🔄 Visitas convertidas para calendario (después del filtro):",
          visitasCalendario
        );
        setVisitasCalendario(visitasCalendario);
      } else {
        console.log("📭 No hay visitas después del filtro");
        setVisitasCalendario([]);
      }
    } catch (error) {
      console.error("❌ Error cargando visitas desde store:", error);
      setVisitasCalendario([]);
      toast.error("Error al cargar visitas del calendario");
    } finally {
      setLoading(false);
    }
  };

  // Crear nueva visita
  const handleNuevaVisita = (fecha: Date, hora: number) => {
    const nuevaFecha = new Date(fecha);
    nuevaFecha.setHours(hora, 0, 0, 0);
    onNuevaVisita(nuevaFecha, agenteSeleccionado || undefined);
  };

  return (
    <div className="bg-white rounded-lg shadow-lg p-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center space-x-4">
          <h2 className="text-2xl font-bold text-gray-900">Agenda Semanal</h2>

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
            {format(inicioSemana, "d MMMM", { locale: es })} -{" "}
            {format(addDays(inicioSemana, 6), "d MMMM yyyy", { locale: es })}
          </span>

          {/* Botón para cargar visitas */}
          <button
            onClick={cargarVisitasManual}
            disabled={loading}
            className="px-3 py-1 text-sm bg-blue-100 text-blue-700 rounded-md hover:bg-blue-200 disabled:opacity-50"
          >
            {loading ? "Cargando..." : "Cargar Visitas"}
          </button>

          {/* Leyenda de colores */}
          <div className="flex items-center space-x-3 text-sm">
            {Object.entries(COLORES_ESTADO).map(([estado, color]) => (
              <div key={estado} className="flex items-center space-x-1">
                <div className={`w-3 h-3 rounded ${color.split(" ")[0]}`}></div>
                <span>{estado}</span>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Grid del calendario */}
      <div className="grid grid-cols-8 gap-0 border border-gray-200 rounded-lg overflow-hidden">
        {/* Header de la grilla */}
        <div className="bg-gray-50 p-3 border-r border-gray-200 font-medium text-center">
          Hora
        </div>
        {diasSemana.map((dia, index) => (
          <div
            key={index}
            className="bg-gray-50 p-3 border-r border-gray-200 text-center"
          >
            <div className="font-medium">{DIAS_SEMANA[index]}</div>
            <div className="text-lg font-bold text-gray-900">
              {format(dia, "d", { locale: es })}
            </div>
            <div className="text-sm text-gray-500">
              {format(dia, "MMM", { locale: es })}
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
              const visitasHora = visitasDia.filter((visita) => {
                if (!visita.start) return false;
                try {
                  const horaVisita = new Date(visita.start).getHours();
                  return horaVisita === hora;
                } catch (error) {
                  console.warn("Error parsing hora visita:", visita.start);
                  return false;
                }
              });

              return (
                <div
                  key={`${hora}-${diaIndex}`}
                  className="relative border-r border-t border-gray-200 h-16 hover:bg-gray-50 cursor-pointer"
                  onClick={() => handleNuevaVisita(dia, hora)}
                >
                  {/* Visitas en esta hora */}
                  {visitasHora.map((visita: any) => {
                    console.log(
                      "🎨 Aplicando color para visita:",
                      visita.id,
                      "estado:",
                      visita.estado,
                      "color:",
                      obtenerColorPorEstado(visita.estado)
                    );
                    return (
                      <div
                        key={visita.id}
                        onClick={(e) => {
                          e.stopPropagation();
                          onEditarVisita(visita.id);
                        }}
                        className={`absolute left-1 right-1 top-1 bottom-1 rounded border cursor-pointer hover:shadow-md transition-shadow p-1 text-xs ${obtenerColorPorEstado(
                          visita.estado
                        )}`}
                      >
                        <div className="font-medium truncate">
                          {visita.propiedadCodigo || visita.title || "Visita"}
                        </div>
                        <div className="truncate">
                          {visita.clienteNombre || "Cliente"}
                        </div>
                        {visita.agenteNombre && agenteSeleccionado === null && (
                          <div className="truncate text-xs opacity-75">
                            {visita.agenteNombre}
                          </div>
                        )}
                      </div>
                    );
                  })}

                  {/* Indicador para agregar nueva visita */}
                  {visitasHora.length === 0 && (
                    <div className="absolute inset-0 flex items-center justify-center opacity-0 hover:opacity-100 transition-opacity">
                      <PlusIcon className="w-4 h-4 text-gray-400" />
                    </div>
                  )}
                </div>
              );
            })}
          </React.Fragment>
        ))}
      </div>

      {/* Instrucciones */}
      <div className="mt-4 text-sm text-gray-600">
        {visitasCalendario.length === 0 && !loading && (
          <div className="text-center py-6 bg-blue-50 rounded-lg mb-4 border border-blue-200">
            <p className="text-blue-600 mb-2 font-medium">📅 Agenda vacía</p>
            <p className="text-blue-500 mb-2">
              Haz clic en "Cargar Visitas" para ver las visitas de la base de
              datos
            </p>
            <p className="text-blue-400 text-sm">
              Se aplicarán los filtros actuales (agente y estado) al cargar
            </p>
            <p className="text-blue-400 text-sm">
              Después de crear o modificar visitas, vuelve a hacer clic en
              "Cargar Visitas"
            </p>
          </div>
        )}
        <p>• Haz clic en una celda vacía para crear una nueva visita</p>
        <p>• Haz clic en una visita para editarla</p>
      </div>

      {/* Loading overlay */}
      {loading && (
        <div className="absolute inset-0 bg-white bg-opacity-75 flex items-center justify-center">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          <span className="ml-3 text-gray-600">Cargando agenda...</span>
        </div>
      )}
    </div>
  );
}
