"use client";

import React, { useState, useEffect, useCallback, useRef } from "react";
import {
  CalendarDaysIcon,
  ListBulletIcon,
  FunnelIcon,
  PlusIcon,
  CheckIcon,
  XMarkIcon,
  ClockIcon,
  EnvelopeIcon,
  ArrowPathIcon,
} from "@heroicons/react/24/outline";
import AgendaSemanal from "./AgendaSemanal";
import VisitaForm from "./VisitaForm";
// Eliminamos modales específicos: usaremos el mismo formulario de edición
import { useVisitasStore } from "../../store/visitasStore";
import { useAuthStore } from "../../store/authStore";
import { toast } from "react-hot-toast";

type VistaActiva = "agenda" | "lista";

interface Visita {
  id: number;
  propiedadCodigo: string;
  propiedadDireccion: string;
  clienteNombre: string;
  clienteTelefono?: string;
  clienteEmail?: string;
  agenteNombre: string;
  fechaHora: string;
  duracionMinutos: number;
  estado: string;
  observaciones?: string;
  fechaCreacion: string;
}

export default function VisitasAdmin() {
  const [vistaActiva, setVistaActiva] = useState<VistaActiva>("lista");
  const [mostrarFormulario, setMostrarFormulario] = useState(false);
  const [visitaEditando, setVisitaEditando] = useState<number | null>(null);
  const [fechaInicialForm, setFechaInicialForm] = useState<Date | undefined>();
  const [agenteInicialForm, setAgenteInicialForm] = useState<
    number | undefined
  >();
  const [agenteSeleccionado, setAgenteSeleccionado] = useState<number | null>(
    null
  );
  const [estadoFiltro, setEstadoFiltro] = useState<string>("");
  const [visitasSeleccionadas, setVisitasSeleccionadas] = useState<number[]>(
    []
  );
  const [agendaRefreshTrigger, setAgendaRefreshTrigger] = useState(0);
  // Eliminamos estados de modales de cancelar/reagendar

  // Seleccionar solo el estado necesario para evitar re-renders innecesarios
  const { visitas, agentes, loading, error } = useVisitasStore((state) => ({
    visitas: state.visitas,
    agentes: state.agentes,
    loading: state.loading,
    error: state.error
  }));

  // Auth state
  const { isAuthenticated, isInitialized } = useAuthStore();

  // console.log('🔄 VisitasAdmin RENDER:', { vistaActiva, agenteSeleccionado, isAuthenticated });

  // Cargar datos cuando el usuario esté autenticado
  const initializedRef = useRef(false);
  useEffect(() => {
    // console.log('🔍 VisitasAdmin useEffect:', { isInitialized, isAuthenticated, hasInitialized: initializedRef.current });
    if (!isInitialized || !isAuthenticated || initializedRef.current) return;
    // console.log('✅ Ejecutando cargarDatos en VisitasAdmin');
    initializedRef.current = true;

    const { cargarAgentes } = useVisitasStore.getState();
    cargarAgentes();
  }, [isInitialized, isAuthenticated]);

  // Cargar visitas cuando cambie la vista a lista o cambien filtros en vista lista
  useEffect(() => {
    if (vistaActiva === "lista" && isAuthenticated) {
      const { setFiltros, cargarVisitas } = useVisitasStore.getState();
      setFiltros({
        agenteId: agenteSeleccionado,
        estado: estadoFiltro || undefined,
      });
      cargarVisitas();
    }
  }, [vistaActiva, agenteSeleccionado, estadoFiltro, isAuthenticated]);

  // Manejar apertura de formulario para nueva visita
  const handleNuevaVisita = (fecha?: Date, agenteId?: number) => {
    setFechaInicialForm(fecha);
    setAgenteInicialForm(agenteId);
    setVisitaEditando(null);
    setMostrarFormulario(true);
  };

  // Manejar edición de visita
  const handleEditarVisita = (visitaId: number) => {
    setVisitaEditando(visitaId);
    setFechaInicialForm(undefined);
    setAgenteInicialForm(undefined);
    setMostrarFormulario(true);
  };

  // Función para triggear refresh de la agenda
  const triggerAgendaRefresh = () => {
    setAgendaRefreshTrigger(Date.now());
  };

  // Manejar éxito del formulario
  const handleFormularioExito = () => {
    // No necesitamos recargar las visitas porque el store ya actualiza la lista localmente
    console.log("🎉 Formulario completado exitosamente");
    setMostrarFormulario(false);
    triggerAgendaRefresh(); // Refrescar agenda
  };

  // Acciones de estado de visita
  const handleConfirmarVisita = async (visitaId: number) => {
    try {
      const { confirmarVisita } = useVisitasStore.getState();
      await confirmarVisita(visitaId);
      toast.success("Visita confirmada correctamente");
      triggerAgendaRefresh();
    } catch (error) {
      toast.error("Error al confirmar la visita");
    }
  };

  const handleCancelarVisita = (visitaId: number) => {
    // Abrimos el mismo formulario de edición para gestionar la cancelación
    setVisitaEditando(visitaId);
    setFechaInicialForm(undefined);
    setAgenteInicialForm(undefined);
    setMostrarFormulario(true);
  };

  // Cancelación directa desde el formulario de edición (sin modal separado)
  const confirmarCancelacion = async (_motivo: string) => {
    if (!visitaEditando) return;
    try {
      const { cancelarVisita } = useVisitasStore.getState();
      await cancelarVisita(visitaEditando, _motivo);
      toast.success("Visita cancelada correctamente");
      setMostrarFormulario(false);
      setVisitaEditando(null);
      triggerAgendaRefresh();
    } catch (error) {
      toast.error("Error al cancelar la visita");
      throw error;
    }
  };

  const handleReagendarVisita = (visitaId: number) => {
    // Usamos el formulario de edición para que el usuario cambie fecha/hora
    setVisitaEditando(visitaId);
    setFechaInicialForm(undefined);
    setAgenteInicialForm(undefined);
    setMostrarFormulario(true);
  };

  const confirmarReagendamiento = async (nuevaFecha: Date) => {
    if (!visitaEditando) return;
    try {
      const { reagendarVisita } = useVisitasStore.getState();
      await reagendarVisita(visitaEditando, nuevaFecha);
      toast.success("Visita reagendada correctamente");
      setMostrarFormulario(false);
      setVisitaEditando(null);
      triggerAgendaRefresh();
    } catch (error) {
      toast.error("Error al reagendar la visita");
      throw error;
    }
  };

  const handleMarcarRealizada = async (visitaId: number) => {
    const notas = ""; // Sin prompt; se podría incorporar en el formulario si se requiere

    try {
      const { marcarRealizada } = useVisitasStore.getState();
      await marcarRealizada(visitaId, notas);
      toast.success("Visita marcada como realizada");
      triggerAgendaRefresh();
    } catch (error) {
      toast.error("Error al marcar la visita como realizada");
    }
  };

  // Acciones masivas
  const handleAccionMasiva = async (accion: string) => {
    if (visitasSeleccionadas.length === 0) {
      toast.error("Selecciona al menos una visita");
      return;
    }

    // En lugar de prompts/modales, abrimos el formulario de la primera visita seleccionada
    const primera = visitasSeleccionadas[0];
    if (accion === "Cancelar" || accion === "Reagendar") {
      setVisitaEditando(primera);
      setMostrarFormulario(true);
      return;
    }

    // Para otras acciones masivas (ej. Confirmar) mantenemos flujo automático
    try {
      const { accionMasiva } = useVisitasStore.getState();
      await accionMasiva(visitasSeleccionadas, accion, {});
      setVisitasSeleccionadas([]);
      toast.success(`Acción "${accion}" aplicada correctamente`);
      triggerAgendaRefresh();
    } catch (error) {
      toast.error(`Error al ejecutar la acción "${accion}"`);
    }
  };

  // Manejar selección de visitas
  const handleSeleccionarVisita = (visitaId: number, seleccionada: boolean) => {
    if (seleccionada) {
      setVisitasSeleccionadas((prev) => [...prev, visitaId]);
    } else {
      setVisitasSeleccionadas((prev) => prev.filter((id) => id !== visitaId));
    }
  };

  // Limpiar errores cuando cambian
  useEffect(() => {
    if (error) {
      toast.error(error);
      const { clearError } = useVisitasStore.getState();
      clearError();
    }
  }, [error]);

  return (
    <div className="p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <div className="flex justify-between items-center">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">
                Gestión de Visitas
              </h1>
              <p className="text-gray-600 mt-1">
                Administra las visitas a propiedades
              </p>
            </div>

            <button
              onClick={() => handleNuevaVisita()}
              className="flex items-center px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors"
            >
              <PlusIcon className="w-5 h-5 mr-2" />
              Nueva Visita
            </button>
          </div>

          {/* Toggle de vista */}
          <div className="flex space-x-2 mt-6">
            <button
              onClick={() => setVistaActiva("agenda")}
              className={`flex items-center px-4 py-2 rounded-lg border transition-colors ${vistaActiva === "agenda"
                ? "bg-blue-500 text-white border-blue-500"
                : "bg-white text-gray-700 border-gray-300 hover:bg-gray-50"
                }`}
            >
              <CalendarDaysIcon className="w-5 h-5 mr-2" />
              Agenda
            </button>

            <button
              onClick={() => setVistaActiva("lista")}
              className={`flex items-center px-4 py-2 rounded-lg border transition-colors ${vistaActiva === "lista"
                ? "bg-blue-500 text-white border-blue-500"
                : "bg-white text-gray-700 border-gray-300 hover:bg-gray-50"
                }`}
            >
              <ListBulletIcon className="w-5 h-5 mr-2" />
              Lista
            </button>
          </div>
        </div>

        {/* Filtros */}
        <div className="mb-6 bg-white p-4 rounded-lg shadow-sm border">
          <div className="flex items-center space-x-4">
            <FunnelIcon className="w-5 h-5 text-gray-500" />

            {/* Filtro por agente */}
            <select
              value={agenteSeleccionado === null ? "todos" : agenteSeleccionado.toString()}
              onChange={(e) =>
                setAgenteSeleccionado(
                  e.target.value === "todos" ? null : Number(e.target.value)
                )
              }
              className="border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="todos">Todos los agentes</option>
              {agentes.map((agente: any) => (
                <option key={agente.id} value={agente.id}>
                  {agente.nombre}
                </option>
              ))}
            </select>

            {/* Filtro por estado */}
            <select
              value={estadoFiltro}
              onChange={(e) => setEstadoFiltro(e.target.value)}
              className="border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Todos los estados</option>
              <option value="Pendiente">Pendiente</option>
              <option value="Confirmada">Confirmada</option>
              <option value="Realizada">Realizada</option>
              <option value="Cancelada">Cancelada</option>
            </select>
          </div>
        </div>

        {/* Contenido principal */}
        <div className="bg-white rounded-lg shadow-sm border">
          {loading ? (
            <div className="flex items-center justify-center py-12">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
              <span className="ml-3 text-gray-600">Cargando visitas...</span>
            </div>
          ) : vistaActiva === "agenda" ? (
            <AgendaSemanal
              onNuevaVisita={handleNuevaVisita}
              onEditarVisita={handleEditarVisita}
              agenteSeleccionado={agenteSeleccionado}
              estadoFiltro={estadoFiltro}
              refreshTrigger={agendaRefreshTrigger}
            />
          ) : (
            <div className="p-6">
              {/* Acciones masivas */}
              {visitasSeleccionadas.length > 0 && (
                <div className="mb-4 p-4 bg-blue-50 border border-blue-200 rounded-lg">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-blue-700">
                      {visitasSeleccionadas.length} visita(s) seleccionada(s)
                    </span>
                    <div className="flex space-x-2">
                      <button
                        onClick={() => handleAccionMasiva("Confirmar")}
                        className="px-3 py-1 text-sm bg-green-600 text-white rounded hover:bg-green-700"
                      >
                        Confirmar
                      </button>
                      <button
                        onClick={() => handleAccionMasiva("Cancelar")}
                        className="px-3 py-1 text-sm bg-red-600 text-white rounded hover:bg-red-700"
                      >
                        Cancelar
                      </button>
                      <button
                        onClick={() => handleAccionMasiva("Reagendar")}
                        className="px-3 py-1 text-sm bg-blue-600 text-white rounded hover:bg-blue-700"
                      >
                        Reagendar
                      </button>
                    </div>
                  </div>
                </div>
              )}

              {/* Lista de visitas */}
              {visitas.length === 0 ? (
                <div className="text-center py-8">
                  <CalendarDaysIcon className="w-16 h-16 text-gray-400 mx-auto mb-4" />
                  <h3 className="text-lg font-semibold text-gray-900 mb-2">
                    No hay visitas
                  </h3>
                  <p className="text-gray-600">
                    Comienza creando tu primera visita
                  </p>
                </div>
              ) : (
                <div className="space-y-4">
                  {visitas.map((visita: any) => {
                    console.log("🔍 Renderizando visita:", visita);
                    return (
                      <div
                        key={visita.id}
                        className="border border-gray-200 rounded-lg p-4"
                      >
                        <div className="flex justify-between items-start">
                          <div className="flex items-start space-x-3 flex-1">
                            <input
                              type="checkbox"
                              checked={visitasSeleccionadas.includes(visita.id)}
                              onChange={(e) =>
                                handleSeleccionarVisita(
                                  visita.id,
                                  e.target.checked
                                )
                              }
                              className="mt-1 rounded border-gray-300"
                            />
                            <div className="flex-1">
                              <h4 className="text-lg font-semibold text-gray-900">
                                {visita.propiedadCodigo} -{" "}
                                {visita.propiedadDireccion}
                              </h4>
                              <p className="text-gray-600">
                                Cliente: {visita.clienteNombre}
                              </p>
                              <p className="text-gray-600">
                                Agente: {visita.agenteNombre}
                              </p>
                              <p className="text-gray-600">
                                Fecha:{" "}
                                {new Date(visita.fechaHora).toLocaleString()}
                              </p>
                            </div>
                          </div>

                          <div className="flex items-center space-x-2">
                            <span
                              className={`px-2 py-1 rounded-full text-xs font-medium ${visita.estado === "Confirmada"
                                ? "bg-green-100 text-green-800"
                                : visita.estado === "Pendiente"
                                  ? "bg-yellow-100 text-yellow-800"
                                  : visita.estado === "Realizada"
                                    ? "bg-blue-100 text-blue-800"
                                    : "bg-red-100 text-red-800"
                                }`}
                            >
                              {visita.estado}
                            </span>

                            <button
                              onClick={() => handleEditarVisita(visita.id)}
                              className="px-3 py-1 text-sm text-blue-600 hover:bg-blue-50 rounded border border-blue-200"
                              title="Editar"
                            >
                              Editar
                            </button>

                            {visita.estado === "Pendiente" && (
                              <button
                                onClick={() => handleConfirmarVisita(visita.id)}
                                className="p-2 text-green-600 hover:bg-green-50 rounded-lg"
                                title="Confirmar"
                              >
                                <CheckIcon className="w-4 h-4" />
                              </button>
                            )}

                            {visita.estado === "Confirmada" && (
                              <button
                                onClick={() => handleMarcarRealizada(visita.id)}
                                className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg"
                                title="Marcar como realizada"
                              >
                                <ClockIcon className="w-4 h-4" />
                              </button>
                            )}

                            {visita.estado !== "Realizada" &&
                              visita.estado !== "Cancelada" && (
                                <button
                                  onClick={() =>
                                    handleReagendarVisita(visita.id)
                                  }
                                  className="p-2 text-orange-600 hover:bg-orange-50 rounded-lg"
                                  title="Reagendar"
                                >
                                  <ArrowPathIcon className="w-4 h-4" />
                                </button>
                              )}

                            {visita.estado !== "Realizada" && (
                              <button
                                onClick={() => handleCancelarVisita(visita.id)}
                                className="p-2 text-red-600 hover:bg-red-50 rounded-lg"
                                title="Cancelar"
                              >
                                <XMarkIcon className="w-4 h-4" />
                              </button>
                            )}
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>
          )}
        </div>

        {/* Modal de formulario */}
        <VisitaForm
          isOpen={mostrarFormulario}
          onClose={() => setMostrarFormulario(false)}
          onSuccess={handleFormularioExito}
          fechaInicial={fechaInicialForm}
          agenteInicial={agenteInicialForm}
          visitaId={visitaEditando || undefined}
        />

        {/* Sin modales adicionales: reagendar/cancelar usan el mismo formulario de edición */}
      </div>
    </div>
  );
}
