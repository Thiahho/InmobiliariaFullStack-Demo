'use client';

import React from 'react';
import { useAuthStore } from '../../../store/authStore';
import { LeadsAdmin } from '../../../components/leads';
import Link from 'next/link';
import { ChevronLeftIcon } from '@heroicons/react/24/outline';

export default function LeadsPage() {
  const { user, role, hasPermission } = useAuthStore();

  // Verificar permisos
  if (!hasPermission('manage_leads') && !hasPermission('view_dashboard')) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-800 mb-4">Acceso denegado</h1>
          <p className="text-gray-600 mb-6">No tienes permisos para acceder a la gestión de leads</p>
          <Link
            href="/admin"
            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
          >
            <ChevronLeftIcon className="h-4 w-4 mr-2" />
            Volver al Panel
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      {/* Header */}
      <div className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-4">
            <div className="flex items-center">
              <Link
                href="/admin"
                className="inline-flex items-center text-sm font-medium text-gray-500 hover:text-gray-700 mr-4"
              >
                <ChevronLeftIcon className="h-4 w-4 mr-1" />
                Panel de Administración
              </Link>
              <div className="text-sm text-gray-300">/ Gestión de Leads</div>
            </div>
            <div className="flex items-center space-x-4">
              <div className="text-right">
                <p className="text-sm font-medium text-gray-900">{user?.nombre}</p>
                <p className="text-xs text-gray-500">{role}</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
        <LeadsAdmin />
      </div>
    </div>
  );
}