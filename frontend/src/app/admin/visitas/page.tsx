'use client';

import React, { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '../../../store/authStore';
import { VisitasAdmin } from '../../../components/visitas';
import { ArrowLeftIcon } from '@heroicons/react/24/outline';


export default function VisitasPage() {
  const { isAuthenticated, role } = useAuthStore();
  const router = useRouter();

  useEffect(() => {
    console.log('VisitasPage - isAuthenticated:', isAuthenticated, 'role:', role);

    // Verificar autenticación y permisos
    if (!isAuthenticated) {
      console.log('Usuario no autenticado, redirigiendo a home');
      router.push('/');
      return;
    }

    // Solo Admin y Agente pueden acceder a la gestión de visitas
    if (role !== 'Admin' && role !== 'Agente') {
      console.log('Rol no permitido:', role, 'redirigiendo a admin');
      router.push('/admin');
      return;
    }

    console.log('Acceso permitido a visitas');
  }, [isAuthenticated, role, router]);

  if (!isAuthenticated || (role !== 'Admin' && role !== 'Agente')) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
  <div className="min-h-screen bg-gray-50">
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-3xl font-bold">Gestión de Visitas</h1>
        <button
          onClick={() => router.push('/admin')}
          className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 hover:text-gray-700 transition-colors"
        >
          <ArrowLeftIcon className="w-4 h-4" />
          Volver al Panel
        </button>
      </div>

      <div className="bg-white p-4 rounded-lg shadow">
        <VisitasAdmin />
      </div>
    </div>
  </div>
);
}
