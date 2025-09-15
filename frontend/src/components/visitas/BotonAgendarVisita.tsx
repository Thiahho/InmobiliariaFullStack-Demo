'use client';

import React, { useState } from 'react';
import { CalendarDaysIcon } from '@heroicons/react/24/outline';
import AgendarVisitaModal from './AgendarVisitaModal';

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

interface BotonAgendarVisitaProps {
  propiedad: Propiedad;
  variant?: 'primary' | 'secondary' | 'outline';
  size?: 'sm' | 'md' | 'lg';
  fullWidth?: boolean;
  className?: string;
}

export default function BotonAgendarVisita({ 
  propiedad, 
  variant = 'primary',
  size = 'md',
  fullWidth = false,
  className = ''
}: BotonAgendarVisitaProps) {
  const [mostrarModal, setMostrarModal] = useState(false);

  const getButtonClasses = () => {
    const baseClasses = 'inline-flex items-center justify-center font-medium rounded-md transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2';
    
    // Variantes de color
    const variantClasses = {
      primary: 'bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500',
      secondary: 'bg-green-600 text-white hover:bg-green-700 focus:ring-green-500',
      outline: 'border border-blue-600 text-blue-600 bg-white hover:bg-blue-50 focus:ring-blue-500'
    };

    // Tamaños
    const sizeClasses = {
      sm: 'px-3 py-2 text-sm',
      md: 'px-4 py-2 text-sm',
      lg: 'px-6 py-3 text-base'
    };

    // Ancho completo
    const widthClass = fullWidth ? 'w-full' : '';

    return `${baseClasses} ${variantClasses[variant]} ${sizeClasses[size]} ${widthClass} ${className}`;
  };

  const getIconSize = () => {
    switch (size) {
      case 'sm': return 'w-4 h-4';
      case 'lg': return 'w-6 h-6';
      default: return 'w-5 h-5';
    }
  };

  return (
    <>
      <button
        onClick={() => setMostrarModal(true)}
        className={getButtonClasses()}
        title="Agendar una visita a esta propiedad"
      >
        <CalendarDaysIcon className={`${getIconSize()} mr-2`} />
        Agendar Visita
      </button>

      <AgendarVisitaModal
        isOpen={mostrarModal}
        onClose={() => setMostrarModal(false)}
        propiedad={propiedad}
      />
    </>
  );
}
