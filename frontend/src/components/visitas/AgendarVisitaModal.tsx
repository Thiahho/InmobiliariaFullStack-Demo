'use client';

import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { format, addDays } from 'date-fns';
import { es } from 'date-fns/locale';
import { 
  XMarkIcon, 
  CalendarDaysIcon,
  ClockIcon,
  UserIcon,
  PhoneIcon,
  EnvelopeIcon,
  HomeIcon,
  CheckCircleIcon
} from '@heroicons/react/24/outline';
import { agendarVisitaPublicSchema } from '../../schemas/visitaSchemas';
import { axiosClient } from '../../lib/axiosClient';
import { toast } from 'react-hot-toast';

interface AgendarVisitaFormData {
  nombre: string;
  telefono: string;
  email: string;
  fechaPreferida: Date;
  horaPreferida: string;
  mensaje?: string;
}

interface Propiedad {
  id: number;
  codigo: string;
  tipo: string;
  direccion: string;
  barrio: string;
  precio: number;
  moneda: string;
  ambientes: number;
  dormitorios?: number;
  banos?: number;
  metrosCubiertos?: number;
}

interface AgendarVisitaModalProps {
  isOpen: boolean;
  onClose: () => void;
  propiedad: Propiedad;
}

const HORARIOS_DISPONIBLES = [
  '09:00', '09:30', '10:00', '10:30', '11:00', '11:30',
  '12:00', '12:30', '14:00', '14:30', '15:00', '15:30',
  '16:00', '16:30', '17:00', '17:30', '18:00'
];

export default function AgendarVisitaModal({ isOpen, onClose, propiedad }: AgendarVisitaModalProps) {
  const [paso, setPaso] = useState<'formulario' | 'confirmacion'>('formulario');
  const [enviando, setEnviando] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    watch
  } = useForm<AgendarVisitaFormData>({
    resolver: zodResolver(agendarVisitaPublicSchema),
    defaultValues: {
      fechaPreferida: addDays(new Date(), 1), // Mañana por defecto
      horaPreferida: '10:00'
    }
  });

  const watchedValues = watch();

  // Generar fechas disponibles (próximos 14 días, excluyendo domingos)
  const getFechasDisponibles = () => {
    const fechas = [];
    let fecha = new Date();
    fecha.setDate(fecha.getDate() + 1); // Empezar desde mañana

    while (fechas.length < 14) {
      // Excluir domingos (0 = domingo)
      if (fecha.getDay() !== 0) {
        fechas.push(new Date(fecha));
      }
      fecha.setDate(fecha.getDate() + 1);
    }

    return fechas;
  };

  const fechasDisponibles = getFechasDisponibles();

  const onSubmit = async (data: AgendarVisitaFormData) => {
    setEnviando(true);
    
    try {
      // Crear el lead con solicitud de visita
      const leadData = {
        nombre: data.nombre,
        email: data.email,
        telefono: data.telefono,
        mensaje: `Solicitud de visita para el ${format(data.fechaPreferida, 'dd/MM/yyyy')} a las ${data.horaPreferida}. ${data.mensaje || ''}`.trim(),
        propiedadId: propiedad.id,
        tipoConsulta: 'Visita',
        canal: 'Web',
        origen: 'ficha-propiedad'
      };

      await axiosClient.post('/leads/solicitar-visita', leadData);

      // Mostrar confirmación
      setPaso('confirmacion');
      
      toast.success('¡Solicitud enviada correctamente!');
    } catch (error) {
      console.error('Error enviando solicitud:', error);
      toast.error('Error al enviar la solicitud. Por favor intenta nuevamente.');
    } finally {
      setEnviando(false);
    }
  };

  const handleClose = () => {
    setPaso('formulario');
    reset();
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-md w-full max-h-[90vh] overflow-y-auto">
        {paso === 'formulario' ? (
          <>
            {/* Header */}
            <div className="flex items-center justify-between p-6 border-b">
              <div>
                <h2 className="text-xl font-semibold text-gray-900">Agendar Visita</h2>
                <p className="text-sm text-gray-600 mt-1">Te contactaremos para coordinar el horario</p>
              </div>
              <button
                onClick={handleClose}
                className="text-gray-400 hover:text-gray-600"
              >
                <XMarkIcon className="w-6 h-6" />
              </button>
            </div>

            {/* Información de la propiedad */}
            <div className="p-6 bg-gray-50 border-b">
              <div className="flex items-start space-x-3">
                <div className="flex-shrink-0">
                  <HomeIcon className="w-6 h-6 text-blue-600" />
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="font-medium text-gray-900">{propiedad.codigo}</h3>
                  <p className="text-sm text-gray-600">{propiedad.tipo} en {propiedad.barrio}</p>
                  <p className="text-sm text-gray-500">{propiedad.direccion}</p>
                  <div className="flex items-center space-x-4 mt-2 text-sm text-gray-600">
                    <span>{propiedad.ambientes} amb</span>
                    {propiedad.dormitorios && <span>{propiedad.dormitorios} dorm</span>}
                    {propiedad.banos && <span>{propiedad.banos} baños</span>}
                    {propiedad.metrosCubiertos && <span>{propiedad.metrosCubiertos}m²</span>}
                  </div>
                  <p className="text-lg font-semibold text-blue-600 mt-2">
                    {propiedad.moneda} {propiedad.precio.toLocaleString()}
                  </p>
                </div>
              </div>
            </div>

            {/* Formulario */}
            <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-4">
              {/* Datos personales */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Nombre completo *
                </label>
                <div className="relative">
                  <UserIcon className="absolute left-3 top-3 w-5 h-5 text-gray-400" />
                  <input
                    {...register('nombre')}
                    type="text"
                    className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                    placeholder="Ej: Juan Pérez"
                  />
                </div>
                {errors.nombre && (
                  <p className="mt-1 text-sm text-red-600">{errors.nombre.message}</p>
                )}
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Teléfono *
                  </label>
                  <div className="relative">
                    <PhoneIcon className="absolute left-3 top-3 w-5 h-5 text-gray-400" />
                    <input
                      {...register('telefono')}
                      type="tel"
                      className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                      placeholder="11 1234-5678"
                    />
                  </div>
                  {errors.telefono && (
                    <p className="mt-1 text-sm text-red-600">{errors.telefono.message}</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Email *
                  </label>
                  <div className="relative">
                    <EnvelopeIcon className="absolute left-3 top-3 w-5 h-5 text-gray-400" />
                    <input
                      {...register('email')}
                      type="email"
                      className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                      placeholder="juan@email.com"
                    />
                  </div>
                  {errors.email && (
                    <p className="mt-1 text-sm text-red-600">{errors.email.message}</p>
                  )}
                </div>
              </div>

              {/* Fecha y hora preferida */}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Fecha preferida *
                  </label>
                  <div className="relative">
                    <CalendarDaysIcon className="absolute left-3 top-3 w-5 h-5 text-gray-400" />
                    <select
                      {...register('fechaPreferida', { valueAsDate: true })}
                      className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                    >
                      {fechasDisponibles.map((fecha) => (
                        <option key={fecha.toISOString()} value={fecha.toISOString().split('T')[0]}>
                          {format(fecha, 'EEEE d \'de\' MMMM', { locale: es })}
                        </option>
                      ))}
                    </select>
                  </div>
                  {errors.fechaPreferida && (
                    <p className="mt-1 text-sm text-red-600">{errors.fechaPreferida.message}</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Hora preferida *
                  </label>
                  <div className="relative">
                    <ClockIcon className="absolute left-3 top-3 w-5 h-5 text-gray-400" />
                    <select
                      {...register('horaPreferida')}
                      className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                    >
                      {HORARIOS_DISPONIBLES.map((hora) => (
                        <option key={hora} value={hora}>
                          {hora}
                        </option>
                      ))}
                    </select>
                  </div>
                  {errors.horaPreferida && (
                    <p className="mt-1 text-sm text-red-600">{errors.horaPreferida.message}</p>
                  )}
                </div>
              </div>

              {/* Mensaje adicional */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Mensaje adicional (opcional)
                </label>
                <textarea
                  {...register('mensaje')}
                  rows={3}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                  placeholder="Algún comentario o preferencia adicional..."
                />
              </div>

              {/* Resumen */}
              <div className="bg-blue-50 p-4 rounded-md border border-blue-200">
                <h4 className="font-medium text-blue-900 mb-2">Resumen de tu solicitud:</h4>
                <div className="text-sm text-blue-800 space-y-1">
                  <p>📅 <strong>Fecha:</strong> {watchedValues.fechaPreferida && format(new Date(watchedValues.fechaPreferida), 'EEEE d \'de\' MMMM', { locale: es })}</p>
                  <p>🕒 <strong>Hora:</strong> {watchedValues.horaPreferida}</p>
                  <p>🏠 <strong>Propiedad:</strong> {propiedad.codigo} - {propiedad.barrio}</p>
                </div>
              </div>

              {/* Disclaimer */}
              <div className="bg-yellow-50 p-4 rounded-md border border-yellow-200">
                <p className="text-sm text-yellow-800">
                  <strong>📞 Importante:</strong> Esta es una solicitud de visita. Un agente se contactará contigo 
                  dentro de las próximas 2 horas para confirmar la disponibilidad y coordinar el horario definitivo.
                </p>
              </div>

              {/* Botones */}
              <div className="flex space-x-3 pt-4">
                <button
                  type="button"
                  onClick={handleClose}
                  className="flex-1 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={enviando}
                  className={`flex-1 px-4 py-2 text-sm font-medium text-white rounded-md ${
                    enviando
                      ? 'bg-gray-400 cursor-not-allowed'
                      : 'bg-blue-600 hover:bg-blue-700'
                  }`}
                >
                  {enviando ? 'Enviando...' : 'Solicitar Visita'}
                </button>
              </div>
            </form>
          </>
        ) : (
          /* Confirmación */
          <div className="p-6 text-center">
            <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-green-100 mb-4">
              <CheckCircleIcon className="h-6 w-6 text-green-600" />
            </div>
            
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              ¡Solicitud enviada correctamente!
            </h3>
            
            <div className="text-sm text-gray-600 space-y-2 mb-6">
              <p>
                Hemos recibido tu solicitud de visita para la propiedad <strong>{propiedad.codigo}</strong>.
              </p>
              <p>
                Un agente se contactará contigo <strong>dentro de las próximas 2 horas</strong> al teléfono{' '}
                <strong>{watchedValues.telefono}</strong> para confirmar la disponibilidad y coordinar el horario definitivo.
              </p>
            </div>

            <div className="bg-blue-50 p-4 rounded-md border border-blue-200 mb-6">
              <h4 className="font-medium text-blue-900 mb-2">Detalles de tu solicitud:</h4>
              <div className="text-sm text-blue-800 space-y-1 text-left">
                <p>👤 <strong>Contacto:</strong> {watchedValues.nombre}</p>
                <p>📞 <strong>Teléfono:</strong> {watchedValues.telefono}</p>
                <p>📧 <strong>Email:</strong> {watchedValues.email}</p>
                <p>📅 <strong>Fecha preferida:</strong> {watchedValues.fechaPreferida && format(new Date(watchedValues.fechaPreferida), 'EEEE d \'de\' MMMM', { locale: es })}</p>
                <p>🕒 <strong>Hora preferida:</strong> {watchedValues.horaPreferida}</p>
                <p>🏠 <strong>Propiedad:</strong> {propiedad.codigo} - {propiedad.direccion}, {propiedad.barrio}</p>
              </div>
            </div>

            <div className="text-xs text-gray-500 mb-6">
              <p>
                💡 <strong>Tip:</strong> También recibirás un email de confirmación con todos los detalles.
              </p>
            </div>
            
            <button
              onClick={handleClose}
              className="w-full px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700"
            >
              Entendido
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
