'use client';

import React, { useState, useEffect } from 'react';
import { useAuthStore } from '../../store/authStore';
import { axiosClient } from '../../lib/axiosClient';
import { toast } from 'react-hot-toast';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import {
  MagnifyingGlassIcon,
  FunnelIcon,
  EyeIcon,
  UserPlusIcon,
  PencilIcon,
  TrashIcon,
  CheckCircleIcon,
  XCircleIcon,
  ClockIcon,
  PhoneIcon,
  EnvelopeIcon,
  MapPinIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
} from '@heroicons/react/24/outline';
import LeadDetailModal from './LeadDetailModal';
import LeadAssignModal from './LeadAssignModal';
import LeadStatusModal from './LeadStatusModal';

interface Lead {
  id: number;
  nombre: string;
  email: string;
  telefono?: string;
  mensaje?: string;
  propiedadId: number;
  propiedadCodigo: string;
  propiedadDireccion: string;
  tipoConsulta: string;
  estado: string;
  agenteAsignadoId?: number;
  agenteAsignadoNombre?: string;
  fechaCreacion: string;
  fechaActualizacion?: string;
  ipAddress?: string;
  userAgent?: string;
}

interface LeadFilters {
  estado?: string;
  tipoConsulta?: string;
  agenteAsignadoId?: number;
  fechaDesde?: string;
  fechaHasta?: string;
  search?: string;
}

interface Agente {
  id: number;
  nombre: string;
  email: string;
  activo: boolean;
}

const ESTADOS_LEAD = [
  { value: 'Nuevo', label: 'Nuevo', color: 'bg-blue-100 text-blue-800' },
  { value: 'EnProceso', label: 'En Proceso', color: 'bg-yellow-100 text-yellow-800' },
  { value: 'NoContesta', label: 'No Contesta', color: 'bg-gray-100 text-gray-800' },
  { value: 'Cerrado', label: 'Cerrado', color: 'bg-green-100 text-green-800' },
];

const TIPOS_CONSULTA = [
  { value: 'Consulta', label: 'Consulta General' },
  { value: 'Visita', label: 'Solicitud de Visita' },
  { value: 'Informacion', label: 'Información' },
  { value: 'Otro', label: 'Otro' },
];

export default function LeadsAdmin() {
  const { user, role } = useAuthStore();
  const [leads, setLeads] = useState<Lead[]>([]);
  const [agentes, setAgentes] = useState<Agente[]>([]);
  const [loading, setLoading] = useState(true);
  const [filters, setFilters] = useState<LeadFilters>({});
  const [showFilters, setShowFilters] = useState(false);
  const [selectedLead, setSelectedLead] = useState<Lead | null>(null);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [showAssignModal, setShowAssignModal] = useState(false);
  const [showStatusModal, setShowStatusModal] = useState(false);

  // Paginación
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 20;

  useEffect(() => {
    fetchLeads();
    fetchAgentes();
  }, [currentPage, filters]);

  const fetchLeads = async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams();

      if (filters.estado) params.append('estado', filters.estado);
      if (filters.tipoConsulta) params.append('tipoConsulta', filters.tipoConsulta);
      if (filters.agenteAsignadoId) params.append('agenteAsignadoId', filters.agenteAsignadoId.toString());
      if (filters.fechaDesde) params.append('fechaDesde', filters.fechaDesde);
      if (filters.fechaHasta) params.append('fechaHasta', filters.fechaHasta);

      params.append('page', currentPage.toString());
      params.append('pageSize', pageSize.toString());
      params.append('orderDesc', 'true');

      const response = await axiosClient.get(`/lead?${params.toString()}`);

      setLeads(response.data.data || []);
      setTotalCount(response.data.totalCount || 0);
      setTotalPages(response.data.totalPaginas || 1);
    } catch (error) {
      console.error('Error fetching leads:', error);
      toast.error('Error al cargar los leads');
    } finally {
      setLoading(false);
    }
  };

  const fetchAgentes = async () => {
    try {
      const response = await axiosClient.get('/usuarios/agentes');
      setAgentes(response.data || []);
    } catch (error) {
      console.error('Error fetching agentes:', error);
    }
  };

  const handleAssignLead = async (leadId: number, agenteId: number, notas?: string) => {
    try {
      await axiosClient.post('/lead/assign', {
        leadId,
        agenteId,
        notas
      });
      toast.success('Lead asignado correctamente');
      setShowAssignModal(false);
      fetchLeads();
    } catch (error) {
      console.error('Error assigning lead:', error);
      toast.error('Error al asignar el lead');
    }
  };

  const handleUpdateStatus = async (leadId: number, estado: string, notasInternas?: string) => {
    try {
      await axiosClient.put('/lead/status', {
        leadId,
        estado,
        notasInternas
      });
      toast.success('Estado actualizado correctamente');
      setShowStatusModal(false);
      fetchLeads();
    } catch (error) {
      console.error('Error updating status:', error);
      toast.error('Error al actualizar el estado');
    }
  };

  const getEstadoBadge = (estado: string) => {
    const estadoConfig = ESTADOS_LEAD.find(e => e.value === estado);
    return estadoConfig || { value: estado, label: estado, color: 'bg-gray-100 text-gray-800' };
  };

  const handleFilterChange = (key: keyof LeadFilters, value: any) => {
    setFilters(prev => ({ ...prev, [key]: value }));
    setCurrentPage(1);
  };

  const clearFilters = () => {
    setFilters({});
    setCurrentPage(1);
  };

  const filteredLeads = leads.filter(lead => {
    if (!filters.search) return true;
    const search = filters.search.toLowerCase();
    return (
      lead.nombre.toLowerCase().includes(search) ||
      lead.email.toLowerCase().includes(search) ||
      lead.propiedadCodigo.toLowerCase().includes(search) ||
      (lead.telefono && lead.telefono.includes(search))
    );
  });

  if (loading && leads.length === 0) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Gestión de Leads</h1>
          <p className="text-gray-600">Administra y da seguimiento a los leads generados</p>
        </div>
        <div className="flex items-center space-x-3">
          <span className="text-sm text-gray-500">
            {totalCount} lead{totalCount !== 1 ? 's' : ''} total{totalCount !== 1 ? 'es' : ''}
          </span>
        </div>
      </div>

      {/* Filtros y búsqueda */}
      <div className="bg-white rounded-lg shadow p-4">
        <div className="flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
          <div className="flex-1 max-w-md">
            <div className="relative">
              <MagnifyingGlassIcon className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
              <input
                type="text"
                placeholder="Buscar por nombre, email, teléfono o código de propiedad..."
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                value={filters.search || ''}
                onChange={(e) => handleFilterChange('search', e.target.value)}
              />
            </div>
          </div>
          <div className="flex items-center space-x-2">
            <button
              onClick={() => setShowFilters(!showFilters)}
              className="inline-flex items-center px-3 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
            >
              <FunnelIcon className="h-4 w-4 mr-2" />
              Filtros
            </button>
          </div>
        </div>

        {/* Panel de filtros expandible */}
        {showFilters && (
          <div className="mt-4 pt-4 border-t border-gray-200">
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Estado</label>
                <select
                  value={filters.estado || ''}
                  onChange={(e) => handleFilterChange('estado', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                >
                  <option value="">Todos los estados</option>
                  {ESTADOS_LEAD.map(estado => (
                    <option key={estado.value} value={estado.value}>{estado.label}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Tipo de Consulta</label>
                <select
                  value={filters.tipoConsulta || ''}
                  onChange={(e) => handleFilterChange('tipoConsulta', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                >
                  <option value="">Todos los tipos</option>
                  {TIPOS_CONSULTA.map(tipo => (
                    <option key={tipo.value} value={tipo.value}>{tipo.label}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Agente Asignado</label>
                <select
                  value={filters.agenteAsignadoId || ''}
                  onChange={(e) => handleFilterChange('agenteAsignadoId', e.target.value ? parseInt(e.target.value) : undefined)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                >
                  <option value="">Todos los agentes</option>
                  <option value="0">Sin asignar</option>
                  {agentes.map(agente => (
                    <option key={agente.id} value={agente.id}>{agente.nombre}</option>
                  ))}
                </select>
              </div>

              <div className="flex items-end">
                <button
                  onClick={clearFilters}
                  className="w-full px-3 py-2 text-sm text-gray-600 bg-gray-100 rounded-md hover:bg-gray-200"
                >
                  Limpiar filtros
                </button>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Tabla de leads */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Cliente
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Propiedad
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Tipo
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Estado
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Agente
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Fecha
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Acciones
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredLeads.length === 0 ? (
                <tr>
                  <td colSpan={7} className="px-6 py-12 text-center text-gray-500">
                    No se encontraron leads con los criterios seleccionados
                  </td>
                </tr>
              ) : (
                filteredLeads.map((lead) => {
                  const estadoBadge = getEstadoBadge(lead.estado);
                  return (
                    <tr key={lead.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div>
                          <div className="text-sm font-medium text-gray-900">{lead.nombre}</div>
                          <div className="text-sm text-gray-500 flex items-center">
                            <EnvelopeIcon className="h-3 w-3 mr-1" />
                            {lead.email}
                          </div>
                          {lead.telefono && (
                            <div className="text-sm text-gray-500 flex items-center">
                              <PhoneIcon className="h-3 w-3 mr-1" />
                              {lead.telefono}
                            </div>
                          )}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div>
                          <div className="text-sm font-medium text-gray-900">{lead.propiedadCodigo}</div>
                          <div className="text-sm text-gray-500 flex items-center">
                            <MapPinIcon className="h-3 w-3 mr-1" />
                            {lead.propiedadDireccion}
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className="text-sm text-gray-900">{lead.tipoConsulta}</span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${estadoBadge.color}`}>
                          {estadoBadge.label}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        {lead.agenteAsignadoNombre ? (
                          <span className="text-sm text-gray-900">{lead.agenteAsignadoNombre}</span>
                        ) : (
                          <span className="text-sm text-gray-400">Sin asignar</span>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm text-gray-900">
                          {format(new Date(lead.fechaCreacion), 'dd/MM/yyyy', { locale: es })}
                        </div>
                        <div className="text-sm text-gray-500">
                          {format(new Date(lead.fechaCreacion), 'HH:mm')}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <div className="flex items-center justify-end space-x-2">
                          <button
                            onClick={() => {
                              setSelectedLead(lead);
                              setShowDetailModal(true);
                            }}
                            className="text-blue-600 hover:text-blue-900"
                            title="Ver detalles"
                          >
                            <EyeIcon className="h-4 w-4" />
                          </button>

                          {!lead.agenteAsignadoId && (
                            <button
                              onClick={() => {
                                setSelectedLead(lead);
                                setShowAssignModal(true);
                              }}
                              className="text-green-600 hover:text-green-900"
                              title="Asignar agente"
                            >
                              <UserPlusIcon className="h-4 w-4" />
                            </button>
                          )}

                          <button
                            onClick={() => {
                              setSelectedLead(lead);
                              setShowStatusModal(true);
                            }}
                            className="text-yellow-600 hover:text-yellow-900"
                            title="Cambiar estado"
                          >
                            <PencilIcon className="h-4 w-4" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>

        {/* Paginación */}
        {totalPages > 1 && (
          <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
            <div className="flex-1 flex justify-between sm:hidden">
              <button
                onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
                disabled={currentPage === 1}
                className="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
              >
                Anterior
              </button>
              <button
                onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
                disabled={currentPage === totalPages}
                className="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
              >
                Siguiente
              </button>
            </div>
            <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
              <div>
                <p className="text-sm text-gray-700">
                  Mostrando{' '}
                  <span className="font-medium">{(currentPage - 1) * pageSize + 1}</span>
                  {' '}a{' '}
                  <span className="font-medium">
                    {Math.min(currentPage * pageSize, totalCount)}
                  </span>
                  {' '}de{' '}
                  <span className="font-medium">{totalCount}</span>
                  {' '}resultados
                </p>
              </div>
              <div>
                <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px">
                  <button
                    onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
                    disabled={currentPage === 1}
                    className="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                  >
                    <ChevronLeftIcon className="h-5 w-5" />
                  </button>

                  {/* Números de página */}
                  {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                    const pageNum = Math.max(1, Math.min(currentPage - 2 + i, totalPages - 4 + i));
                    return pageNum <= totalPages ? (
                      <button
                        key={pageNum}
                        onClick={() => setCurrentPage(pageNum)}
                        className={`relative inline-flex items-center px-4 py-2 border text-sm font-medium ${
                          currentPage === pageNum
                            ? 'z-10 bg-blue-50 border-blue-500 text-blue-600'
                            : 'bg-white border-gray-300 text-gray-500 hover:bg-gray-50'
                        }`}
                      >
                        {pageNum}
                      </button>
                    ) : null;
                  })}

                  <button
                    onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
                    disabled={currentPage === totalPages}
                    className="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                  >
                    <ChevronRightIcon className="h-5 w-5" />
                  </button>
                </nav>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Modales */}
      <LeadDetailModal
        lead={selectedLead}
        isOpen={showDetailModal}
        onClose={() => setShowDetailModal(false)}
      />

      <LeadAssignModal
        lead={selectedLead}
        agentes={agentes}
        isOpen={showAssignModal}
        onClose={() => setShowAssignModal(false)}
        onAssign={handleAssignLead}
      />

      <LeadStatusModal
        lead={selectedLead}
        isOpen={showStatusModal}
        onClose={() => setShowStatusModal(false)}
        onUpdateStatus={handleUpdateStatus}
      />
    </div>
  );
}