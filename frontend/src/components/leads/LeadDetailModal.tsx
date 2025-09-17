'use client';

import React from 'react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import {
  XMarkIcon,
  EnvelopeIcon,
  PhoneIcon,
  MapPinIcon,
  CalendarIcon,
  UserIcon,
  DocumentTextIcon,
  ComputerDesktopIcon,
  GlobeAltIcon,
} from '@heroicons/react/24/outline';

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
  notasInternas?: string;
}

interface LeadDetailModalProps {
  lead: Lead;
  isOpen: boolean;
  onClose: () => void;
}

const ESTADOS_LEAD = [
  { value: 'Nuevo', label: 'Nuevo', color: 'bg-blue-100 text-blue-800' },
  { value: 'EnProceso', label: 'En Proceso', color: 'bg-yellow-100 text-yellow-800' },
  { value: 'NoContesta', label: 'No Contesta', color: 'bg-gray-100 text-gray-800' },
  { value: 'Cerrado', label: 'Cerrado', color: 'bg-green-100 text-green-800' },
];

export default function LeadDetailModal({ lead, isOpen, onClose }: LeadDetailModalProps) {
  if (!isOpen) return null;

  const getEstadoBadge = (estado: string) => {
    const estadoConfig = ESTADOS_LEAD.find(e => e.value === estado);
    return estadoConfig || { value: estado, label: estado, color: 'bg-gray-100 text-gray-800' };
  };

  const estadoBadge = getEstadoBadge(lead.estado);

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 z-50 overflow-y-auto">
      <div className="flex min-h-screen items-center justify-center p-4">
        <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b border-gray-200">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
                  <UserIcon className="h-6 w-6 text-blue-600" />
                </div>
              </div>
              <div className="ml-4">
                <h3 className="text-lg font-medium text-gray-900">
                  Detalle del Lead #{lead.id}
                </h3>
                <p className="text-sm text-gray-500">{lead.nombre}</p>
              </div>
            </div>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600"
            >
              <XMarkIcon className="h-6 w-6" />
            </button>
          </div>

          {/* Content */}
          <div className="p-6 space-y-6">
            {/* Estado y Tipo */}
            <div className="flex items-center justify-between">
              <div>
                <h4 className="text-sm font-medium text-gray-500 mb-1">Estado</h4>
                <span className={`inline-flex px-3 py-1 text-sm font-semibold rounded-full ${estadoBadge.color}`}>
                  {estadoBadge.label}
                </span>
              </div>
              <div className="text-right">
                <h4 className="text-sm font-medium text-gray-500 mb-1">Tipo de Consulta</h4>
                <p className="text-sm text-gray-900">{lead.tipoConsulta}</p>
              </div>
            </div>

            {/* Información del Cliente */}
            <div>
              <h4 className="text-lg font-medium text-gray-900 mb-3">Información del Cliente</h4>
              <div className="bg-gray-50 rounded-lg p-4 space-y-3">
                <div className="flex items-center">
                  <UserIcon className="h-5 w-5 text-gray-400 mr-3" />
                  <div>
                    <p className="text-sm font-medium text-gray-900">{lead.nombre}</p>
                  </div>
                </div>

                <div className="flex items-center">
                  <EnvelopeIcon className="h-5 w-5 text-gray-400 mr-3" />
                  <div>
                    <p className="text-sm text-gray-900">{lead.email}</p>
                  </div>
                </div>

                {lead.telefono && (
                  <div className="flex items-center">
                    <PhoneIcon className="h-5 w-5 text-gray-400 mr-3" />
                    <div>
                      <p className="text-sm text-gray-900">{lead.telefono}</p>
                    </div>
                  </div>
                )}
              </div>
            </div>

            {/* Información de la Propiedad */}
            <div>
              <h4 className="text-lg font-medium text-gray-900 mb-3">Propiedad de Interés</h4>
              <div className="bg-gray-50 rounded-lg p-4">
                <div className="flex items-center mb-2">
                  <MapPinIcon className="h-5 w-5 text-gray-400 mr-3" />
                  <div>
                    <p className="text-sm font-medium text-gray-900">{lead.propiedadCodigo}</p>
                    <p className="text-sm text-gray-500">{lead.propiedadDireccion}</p>
                  </div>
                </div>
              </div>
            </div>

            {/* Mensaje del Cliente */}
            {lead.mensaje && (
              <div>
                <h4 className="text-lg font-medium text-gray-900 mb-3">Mensaje del Cliente</h4>
                <div className="bg-gray-50 rounded-lg p-4">
                  <div className="flex items-start">
                    <DocumentTextIcon className="h-5 w-5 text-gray-400 mr-3 mt-0.5" />
                    <div className="flex-1">
                      <p className="text-sm text-gray-900 whitespace-pre-wrap">{lead.mensaje}</p>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {/* Agente Asignado */}
            {lead.agenteAsignadoNombre && (
              <div>
                <h4 className="text-lg font-medium text-gray-900 mb-3">Agente Asignado</h4>
                <div className="bg-green-50 rounded-lg p-4">
                  <div className="flex items-center">
                    <div className="w-8 h-8 bg-green-100 rounded-full flex items-center justify-center mr-3">
                      <UserIcon className="h-4 w-4 text-green-600" />
                    </div>
                    <div>
                      <p className="text-sm font-medium text-green-900">{lead.agenteAsignadoNombre}</p>
                      <p className="text-sm text-green-600">Agente responsable</p>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {/* Notas Internas */}
            {lead.notasInternas && (
              <div>
                <h4 className="text-lg font-medium text-gray-900 mb-3">Notas Internas</h4>
                <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                  <p className="text-sm text-yellow-800 whitespace-pre-wrap">{lead.notasInternas}</p>
                </div>
              </div>
            )}

            {/* Información Técnica */}
            <div>
              <h4 className="text-lg font-medium text-gray-900 mb-3">Información Técnica</h4>
              <div className="bg-gray-50 rounded-lg p-4 space-y-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center">
                    <CalendarIcon className="h-5 w-5 text-gray-400 mr-3" />
                    <div>
                      <p className="text-sm font-medium text-gray-900">Fecha de Creación</p>
                      <p className="text-sm text-gray-500">
                        {format(new Date(lead.fechaCreacion), 'dd/MM/yyyy HH:mm', { locale: es })}
                      </p>
                    </div>
                  </div>
                </div>

                {lead.fechaActualizacion && (
                  <div className="flex items-center justify-between">
                    <div className="flex items-center">
                      <CalendarIcon className="h-5 w-5 text-gray-400 mr-3" />
                      <div>
                        <p className="text-sm font-medium text-gray-900">Última Actualización</p>
                        <p className="text-sm text-gray-500">
                          {format(new Date(lead.fechaActualizacion), 'dd/MM/yyyy HH:mm', { locale: es })}
                        </p>
                      </div>
                    </div>
                  </div>
                )}

                {lead.ipAddress && (
                  <div className="flex items-center">
                    <GlobeAltIcon className="h-5 w-5 text-gray-400 mr-3" />
                    <div>
                      <p className="text-sm font-medium text-gray-900">IP Address</p>
                      <p className="text-sm text-gray-500">{lead.ipAddress}</p>
                    </div>
                  </div>
                )}

                {lead.userAgent && (
                  <div className="flex items-start">
                    <ComputerDesktopIcon className="h-5 w-5 text-gray-400 mr-3 mt-0.5" />
                    <div className="flex-1">
                      <p className="text-sm font-medium text-gray-900">User Agent</p>
                      <p className="text-sm text-gray-500 break-all">{lead.userAgent}</p>
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Footer */}
          <div className="flex justify-end p-6 border-t border-gray-200">
            <button
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            >
              Cerrar
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}