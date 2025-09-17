'use client';

import React, { useState } from 'react';
import { toast } from 'react-hot-toast';
import {
  XMarkIcon,
  PencilIcon,
  CheckCircleIcon,
  XCircleIcon,
  ClockIcon,
  PlayIcon,
} from '@heroicons/react/24/outline';

interface Lead {
  id: number;
  nombre: string;
  email: string;
  telefono?: string;
  propiedadCodigo: string;
  propiedadDireccion: string;
  tipoConsulta: string;
  estado: string;
  agenteAsignadoNombre?: string;
}

interface LeadStatusModalProps {
  lead: Lead | null;
  isOpen: boolean;
  onClose: () => void;
  onUpdateStatus: (leadId: number, estado: string, notasInternas?: string) => Promise<void>;
}

const ESTADOS_LEAD = [
  {
    value: 'Nuevo',
    label: 'Nuevo',
    color: 'bg-blue-100 text-blue-800 border-blue-200',
    icon: PlayIcon,
    description: 'Lead recién recibido, sin contacto previo'
  },
  {
    value: 'EnProceso',
    label: 'En Proceso',
    color: 'bg-yellow-100 text-yellow-800 border-yellow-200',
    icon: ClockIcon,
    description: 'Lead contactado, en seguimiento activo'
  },
  {
    value: 'NoContesta',
    label: 'No Contesta',
    color: 'bg-gray-100 text-gray-800 border-gray-200',
    icon: XCircleIcon,
    description: 'Cliente no responde a intentos de contacto'
  },
  {
    value: 'Cerrado',
    label: 'Cerrado',
    color: 'bg-green-100 text-green-800 border-green-200',
    icon: CheckCircleIcon,
    description: 'Lead finalizado (exitoso o descartado)'
  },
];

export default function LeadStatusModal({
  lead,
  isOpen,
  onClose,
  onUpdateStatus,
}: LeadStatusModalProps) {
  const [selectedEstado, setSelectedEstado] = useState<string>('');
  const [notasInternas, setNotasInternas] = useState('');
  const [loading, setLoading] = useState(false);

  React.useEffect(() => {
    if (lead) {
      setSelectedEstado(lead.estado);
      setNotasInternas('');
    }
  }, [lead]);

  if (!isOpen || !lead) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedEstado) {
      toast.error('Por favor selecciona un estado');
      return;
    }

    if (selectedEstado === lead.estado && !notasInternas.trim()) {
      toast.error('No hay cambios para guardar');
      return;
    }

    try {
      setLoading(true);
      await onUpdateStatus(lead.id, selectedEstado, notasInternas.trim() || undefined);

      // Limpiar formulario
      setNotasInternas('');
    } catch (error) {
      // El error ya se maneja en el componente padre
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    if (loading) return;
    setSelectedEstado(lead?.estado || '');
    setNotasInternas('');
    onClose();
  };

  const selectedEstadoConfig = ESTADOS_LEAD.find(e => e.value === selectedEstado);
  const currentEstadoConfig = ESTADOS_LEAD.find(e => e.value === lead.estado);

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 z-50 overflow-y-auto">
      <div className="flex min-h-screen items-center justify-center p-4">
        <div className="bg-white rounded-lg shadow-xl max-w-md w-full">
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b border-gray-200">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <div className="w-10 h-10 bg-yellow-100 rounded-full flex items-center justify-center">
                  <PencilIcon className="h-6 w-6 text-yellow-600" />
                </div>
              </div>
              <div className="ml-4">
                <h3 className="text-lg font-medium text-gray-900">
                  Cambiar Estado
                </h3>
                <p className="text-sm text-gray-500">Lead #{lead.id} - {lead.nombre}</p>
              </div>
            </div>
            <button
              onClick={handleClose}
              disabled={loading}
              className="text-gray-400 hover:text-gray-600 disabled:opacity-50"
            >
              <XMarkIcon className="h-6 w-6" />
            </button>
          </div>

          {/* Content */}
          <form onSubmit={handleSubmit}>
            <div className="p-6 space-y-4">
              {/* Información del Lead */}
              <div className="bg-gray-50 rounded-lg p-4">
                <h4 className="text-sm font-medium text-gray-900 mb-2">Información del Lead</h4>
                <div className="space-y-1 text-sm text-gray-600">
                  <p><span className="font-medium">Cliente:</span> {lead.nombre}</p>
                  <p><span className="font-medium">Email:</span> {lead.email}</p>
                  <p><span className="font-medium">Propiedad:</span> {lead.propiedadCodigo}</p>
                  <p><span className="font-medium">Tipo:</span> {lead.tipoConsulta}</p>
                  {lead.agenteAsignadoNombre && (
                    <p><span className="font-medium">Agente:</span> {lead.agenteAsignadoNombre}</p>
                  )}
                </div>
              </div>

              {/* Estado Actual */}
              {currentEstadoConfig && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Estado Actual
                  </label>
                  <div className="flex items-center p-3 bg-gray-50 rounded-md border">
                    <currentEstadoConfig.icon className="h-5 w-5 text-gray-500 mr-3" />
                    <div className="flex-1">
                      <span className={`inline-flex px-2 py-1 text-sm font-semibold rounded-full ${currentEstadoConfig.color}`}>
                        {currentEstadoConfig.label}
                      </span>
                      <p className="text-sm text-gray-500 mt-1">{currentEstadoConfig.description}</p>
                    </div>
                  </div>
                </div>
              )}

              {/* Selección de Nuevo Estado */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Nuevo Estado *
                </label>
                <div className="space-y-2">
                  {ESTADOS_LEAD.map((estado) => {
                    const IconComponent = estado.icon;
                    const isSelected = selectedEstado === estado.value;
                    const isCurrent = lead.estado === estado.value;

                    return (
                      <label
                        key={estado.value}
                        className={`flex items-center p-3 cursor-pointer rounded-md border transition-colors ${
                          isSelected
                            ? `${estado.color.replace('bg-', 'bg-').replace('text-', 'text-')} border-current`
                            : 'hover:bg-gray-50 border-gray-200'
                        } ${isCurrent ? 'ring-2 ring-blue-200' : ''}`}
                      >
                        <input
                          type="radio"
                          name="estado"
                          value={estado.value}
                          checked={selectedEstado === estado.value}
                          onChange={(e) => setSelectedEstado(e.target.value)}
                          className="sr-only"
                          disabled={loading}
                        />
                        <IconComponent className={`h-5 w-5 mr-3 ${isSelected ? 'text-current' : 'text-gray-400'}`} />
                        <div className="flex-1">
                          <div className="flex items-center">
                            <span className="text-sm font-medium">{estado.label}</span>
                            {isCurrent && (
                              <span className="ml-2 text-xs text-blue-600 bg-blue-100 px-2 py-0.5 rounded">
                                Actual
                              </span>
                            )}
                          </div>
                          <p className={`text-sm mt-1 ${isSelected ? 'text-current opacity-80' : 'text-gray-500'}`}>
                            {estado.description}
                          </p>
                        </div>
                      </label>
                    );
                  })}
                </div>
              </div>

              {/* Notas Internas */}
              <div>
                <label htmlFor="notasInternas" className="block text-sm font-medium text-gray-700 mb-2">
                  Notas Internas {selectedEstado !== lead.estado ? '(Opcional)' : ''}
                </label>
                <textarea
                  id="notasInternas"
                  rows={3}
                  value={notasInternas}
                  onChange={(e) => setNotasInternas(e.target.value)}
                  placeholder="Agregar notas sobre el cambio de estado, razones, próximos pasos, etc..."
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                  disabled={loading}
                />
                <p className="mt-1 text-xs text-gray-500">
                  Estas notas serán visibles para otros agentes y administradores
                </p>
              </div>

              {/* Advertencia si no hay cambios */}
              {selectedEstado === lead.estado && !notasInternas.trim() && (
                <div className="bg-yellow-50 border border-yellow-200 rounded-md p-3">
                  <p className="text-sm text-yellow-800">
                    💡 El estado no ha cambiado. Puedes agregar notas internas para registrar información adicional.
                  </p>
                </div>
              )}
            </div>

            {/* Footer */}
            <div className="flex justify-end space-x-3 p-6 border-t border-gray-200">
              <button
                type="button"
                onClick={handleClose}
                disabled={loading}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 disabled:opacity-50"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={loading || (!selectedEstado || (selectedEstado === lead.estado && !notasInternas.trim()))}
                className="px-4 py-2 text-sm font-medium text-white bg-yellow-600 border border-transparent rounded-md hover:bg-yellow-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-yellow-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {loading ? (
                  <div className="flex items-center">
                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                    Guardando...
                  </div>
                ) : (
                  selectedEstado === lead.estado ? 'Agregar Notas' : 'Cambiar Estado'
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}