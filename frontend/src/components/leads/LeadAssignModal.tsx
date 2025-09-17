'use client';

import React, { useState } from 'react';
import { toast } from 'react-hot-toast';
import {
  XMarkIcon,
  UserPlusIcon,
  UserIcon,
} from '@heroicons/react/24/outline';

interface Lead {
  id: number;
  nombre: string;
  email: string;
  telefono?: string;
  propiedadCodigo: string;
  propiedadDireccion: string;
  tipoConsulta: string;
}

interface Agente {
  id: number;
  nombre: string;
  email: string;
  activo: boolean;
}

interface LeadAssignModalProps {
  lead: Lead | null;
  agentes: Agente[];
  isOpen: boolean;
  onClose: () => void;
  onAssign: (leadId: number, agenteId: number, notas?: string) => Promise<void>;
}

export default function LeadAssignModal({
  lead,
  agentes,
  isOpen,
  onClose,
  onAssign,
}: LeadAssignModalProps) {
  const [selectedAgente, setSelectedAgente] = useState<number | null>(null);
  const [notas, setNotas] = useState('');
  const [loading, setLoading] = useState(false);

  if (!isOpen || !lead) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedAgente) {
      toast.error('Por favor selecciona un agente');
      return;
    }

    try {
      setLoading(true);
      await onAssign(lead.id, selectedAgente, notas.trim() || undefined);

      // Limpiar formulario
      setSelectedAgente(null);
      setNotas('');
    } catch (error) {
      // El error ya se maneja en el componente padre
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    if (loading) return;
    setSelectedAgente(null);
    setNotas('');
    onClose();
  };

  const agentesActivos = agentes.filter(agente => agente.activo);

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 z-50 overflow-y-auto">
      <div className="flex min-h-screen items-center justify-center p-4">
        <div className="bg-white rounded-lg shadow-xl max-w-md w-full">
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b border-gray-200">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <div className="w-10 h-10 bg-green-100 rounded-full flex items-center justify-center">
                  <UserPlusIcon className="h-6 w-6 text-green-600" />
                </div>
              </div>
              <div className="ml-4">
                <h3 className="text-lg font-medium text-gray-900">
                  Asignar Lead
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
                  {lead.telefono && (
                    <p><span className="font-medium">Teléfono:</span> {lead.telefono}</p>
                  )}
                  <p><span className="font-medium">Propiedad:</span> {lead.propiedadCodigo}</p>
                  <p><span className="font-medium">Tipo:</span> {lead.tipoConsulta}</p>
                </div>
              </div>

              {/* Selección de Agente */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Seleccionar Agente *
                </label>
                {agentesActivos.length === 0 ? (
                  <div className="text-center py-4">
                    <UserIcon className="mx-auto h-12 w-12 text-gray-400" />
                    <p className="mt-2 text-sm text-gray-500">No hay agentes activos disponibles</p>
                  </div>
                ) : (
                  <div className="space-y-2 max-h-48 overflow-y-auto border border-gray-200 rounded-md">
                    {agentesActivos.map((agente) => (
                      <label
                        key={agente.id}
                        className={`flex items-center p-3 cursor-pointer hover:bg-gray-50 transition-colors ${
                          selectedAgente === agente.id ? 'bg-blue-50 border-blue-200' : ''
                        }`}
                      >
                        <input
                          type="radio"
                          name="agente"
                          value={agente.id}
                          checked={selectedAgente === agente.id}
                          onChange={(e) => setSelectedAgente(parseInt(e.target.value))}
                          className="h-4 w-4 text-blue-600 border-gray-300 focus:ring-blue-500"
                          disabled={loading}
                        />
                        <div className="ml-3 flex items-center">
                          <div className="w-8 h-8 bg-gray-200 rounded-full flex items-center justify-center mr-3">
                            <UserIcon className="h-4 w-4 text-gray-600" />
                          </div>
                          <div>
                            <p className="text-sm font-medium text-gray-900">{agente.nombre}</p>
                            <p className="text-sm text-gray-500">{agente.email}</p>
                          </div>
                        </div>
                      </label>
                    ))}
                  </div>
                )}
              </div>

              {/* Notas */}
              <div>
                <label htmlFor="notas" className="block text-sm font-medium text-gray-700 mb-2">
                  Notas para el Agente (Opcional)
                </label>
                <textarea
                  id="notas"
                  rows={3}
                  value={notas}
                  onChange={(e) => setNotas(e.target.value)}
                  placeholder="Agregar notas adicionales para el agente asignado..."
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                  disabled={loading}
                />
                <p className="mt-1 text-xs text-gray-500">
                  Estas notas serán visibles para el agente asignado
                </p>
              </div>
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
                disabled={loading || !selectedAgente || agentesActivos.length === 0}
                className="px-4 py-2 text-sm font-medium text-white bg-green-600 border border-transparent rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {loading ? (
                  <div className="flex items-center">
                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                    Asignando...
                  </div>
                ) : (
                  'Asignar Lead'
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}