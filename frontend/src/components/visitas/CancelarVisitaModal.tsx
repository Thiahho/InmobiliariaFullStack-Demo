'use client';

import React, { useState } from 'react';
import { XMarkIcon } from '@heroicons/react/24/outline';

interface CancelarVisitaModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: (motivo: string) => void;
  visitaInfo?: {
    clienteNombre: string;
    propiedadCodigo: string;
    fechaHora: string;
  };
}

export default function CancelarVisitaModal({ 
  isOpen, 
  onClose, 
  onConfirm, 
  visitaInfo 
}: CancelarVisitaModalProps) {
  const [motivo, setMotivo] = useState('');
  const [loading, setLoading] = useState(false);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!motivo.trim()) return;
    
    setLoading(true);
    try {
      await onConfirm(motivo.trim());
      setMotivo('');
      onClose();
    } catch (error) {
      // El error será manejado por el componente padre
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    if (!loading) {
      setMotivo('');
      onClose();
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl max-w-md w-full">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-gray-900">
            Cancelar Visita
          </h3>
          <button
            onClick={handleClose}
            disabled={loading}
            className="text-gray-400 hover:text-gray-600 disabled:opacity-50"
          >
            <XMarkIcon className="w-6 h-6" />
          </button>
        </div>

        {/* Content */}
        <form onSubmit={handleSubmit} className="p-6">
          {visitaInfo && (
            <div className="mb-4 p-4 bg-gray-50 rounded-lg">
              <div className="text-sm text-gray-600">
                <p><span className="font-medium">Cliente:</span> {visitaInfo.clienteNombre}</p>
                <p><span className="font-medium">Propiedad:</span> {visitaInfo.propiedadCodigo}</p>
                <p><span className="font-medium">Fecha:</span> {new Date(visitaInfo.fechaHora).toLocaleString()}</p>
              </div>
            </div>
          )}

          <div className="mb-6">
            <label htmlFor="motivo" className="block text-sm font-medium text-gray-700 mb-2">
              Motivo de la cancelación *
            </label>
            <textarea
              id="motivo"
              value={motivo}
              onChange={(e) => setMotivo(e.target.value)}
              rows={4}
              required
              disabled={loading}
              placeholder="Describe el motivo de la cancelación..."
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 disabled:bg-gray-100 disabled:cursor-not-allowed"
            />
          </div>

          {/* Actions */}
          <div className="flex justify-end space-x-3">
            <button
              type="button"
              onClick={handleClose}
              disabled={loading}
              className="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={loading || !motivo.trim()}
              className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center"
            >
              {loading && (
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
              )}
              {loading ? 'Cancelando...' : 'Cancelar Visita'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}