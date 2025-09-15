'use client';

import React, { useState, useEffect } from 'react';
import { XMarkIcon, CalendarIcon, ClockIcon } from '@heroicons/react/24/outline';
import { format, addDays } from 'date-fns';
import { es } from 'date-fns/locale';

interface ReagendarVisitaModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: (nuevaFecha: Date) => void;
  visitaInfo?: {
    clienteNombre: string;
    propiedadCodigo: string;
    fechaHora: string;
  };
}

export default function ReagendarVisitaModal({ 
  isOpen, 
  onClose, 
  onConfirm, 
  visitaInfo 
}: ReagendarVisitaModalProps) {
  const [fecha, setFecha] = useState('');
  const [hora, setHora] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // Inicializar con valores por defecto cuando se abre el modal
  useEffect(() => {
    if (isOpen) {
      const mañana = addDays(new Date(), 1);
      setFecha(format(mañana, 'yyyy-MM-dd'));
      setHora('09:00');
      setError('');
    }
  }, [isOpen]);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    
    if (!fecha || !hora) {
      setError('Fecha y hora son requeridos');
      return;
    }

    // Crear nueva fecha
    const nuevaFecha = new Date(`${fecha}T${hora}`);
    
    // Validar que no sea en el pasado
    if (nuevaFecha <= new Date()) {
      setError('La fecha debe ser futura');
      return;
    }
    
    setLoading(true);
    try {
      await onConfirm(nuevaFecha);
      handleClose();
    } catch (error) {
      setError('Error al reagendar la visita');
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    if (!loading) {
      setFecha('');
      setHora('');
      setError('');
      onClose();
    }
  };

  // Generar opciones de hora (8 AM a 6 PM)
  const horasDisponibles = [];
  for (let h = 8; h <= 18; h++) {
    horasDisponibles.push({
      value: `${h.toString().padStart(2, '0')}:00`,
      label: `${h}:00 ${h < 12 ? 'AM' : 'PM'}`
    });
    if (h < 18) {
      horasDisponibles.push({
        value: `${h.toString().padStart(2, '0')}:30`,
        label: `${h}:30 ${h < 12 ? 'AM' : 'PM'}`
      });
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl max-w-md w-full">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-gray-900">
            Reagendar Visita
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
                <p><span className="font-medium">Fecha actual:</span> {new Date(visitaInfo.fechaHora).toLocaleString()}</p>
              </div>
            </div>
          )}

          {error && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-sm text-red-700">{error}</p>
            </div>
          )}

          {/* Nueva fecha */}
          <div className="mb-4">
            <label htmlFor="fecha" className="block text-sm font-medium text-gray-700 mb-2">
              Nueva fecha *
            </label>
            <div className="relative">
              <input
                type="date"
                id="fecha"
                value={fecha}
                onChange={(e) => setFecha(e.target.value)}
                min={format(addDays(new Date(), 1), 'yyyy-MM-dd')}
                required
                disabled={loading}
                className="w-full px-3 py-2 pl-10 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
              />
              <CalendarIcon className="w-5 h-5 text-gray-400 absolute left-3 top-2.5" />
            </div>
          </div>

          {/* Nueva hora */}
          <div className="mb-6">
            <label htmlFor="hora" className="block text-sm font-medium text-gray-700 mb-2">
              Nueva hora *
            </label>
            <div className="relative">
              <select
                id="hora"
                value={hora}
                onChange={(e) => setHora(e.target.value)}
                required
                disabled={loading}
                className="w-full px-3 py-2 pl-10 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
              >
                <option value="">Seleccionar hora</option>
                {horasDisponibles.map((opcion) => (
                  <option key={opcion.value} value={opcion.value}>
                    {opcion.label}
                  </option>
                ))}
              </select>
              <ClockIcon className="w-5 h-5 text-gray-400 absolute left-3 top-2.5" />
            </div>
          </div>

          {/* Preview de nueva fecha */}
          {fecha && hora && (
            <div className="mb-6 p-3 bg-blue-50 border border-blue-200 rounded-lg">
              <p className="text-sm text-blue-700">
                <span className="font-medium">Nueva fecha y hora:</span> {' '}
                {format(new Date(`${fecha}T${hora}`), "EEEE, d 'de' MMMM 'de' yyyy 'a las' HH:mm", { locale: es })}
              </p>
            </div>
          )}

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
              disabled={loading || !fecha || !hora}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center"
            >
              {loading && (
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
              )}
              {loading ? 'Reagendando...' : 'Reagendar'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}