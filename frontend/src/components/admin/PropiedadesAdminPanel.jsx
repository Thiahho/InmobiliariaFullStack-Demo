import React from 'react';
import { useAuthStore } from '../../store/authStore';
import PropiedadesModule from '../propiedades/PropiedadesModule';
import { 
  HomeIcon, 
  ExclamationTriangleIcon 
} from '@heroicons/react/24/outline';

const PropiedadesAdminPanel = () => {
  const { hasPermission, user, role } = useAuthStore();

  // Verificar permisos básicos
  const canViewPropiedades = hasPermission('view_dashboard') || hasPermission('manage_propiedades');

  if (!canViewPropiedades) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="max-w-md w-full bg-white rounded-lg shadow-lg p-8 text-center">
          <ExclamationTriangleIcon className="h-16 w-16 text-red-500 mx-auto mb-4" />
          <h2 className="text-2xl font-bold text-gray-900 mb-2">Acceso Denegado</h2>
          <p className="text-gray-600 mb-6">
            No tienes permisos para acceder al módulo de propiedades.
          </p>
          <div className="bg-gray-50 rounded-lg p-4">
            <p className="text-sm text-gray-700">
              <strong>Usuario:</strong> {user?.name || 'No identificado'}
            </p>
            <p className="text-sm text-gray-700">
              <strong>Rol:</strong> {role || 'Sin rol asignado'}
            </p>
          </div>
          <p className="text-sm text-gray-500 mt-4">
            Contacta al administrador para solicitar acceso.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header de contexto */}
      <div className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center py-4">
            <HomeIcon className="h-8 w-8 text-blue-600 mr-3" />
            <div>
              <h1 className="text-xl font-semibold text-gray-900">
                Gestión de Propiedades
              </h1>
              <p className="text-sm text-gray-600">
                Administra el catálogo completo de propiedades
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Contenido principal */}
      <PropiedadesModule />

      {/* Footer informativo */}
      <footer className="bg-white border-t border-gray-200 mt-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex justify-between items-center text-sm text-gray-500">
            <div className="flex items-center space-x-4">
              <span>Rol: {role}</span>
              <span>•</span>
              <span>Usuario: {user?.name}</span>
            </div>
            <div className="flex items-center space-x-4">
              {hasPermission('manage_propiedades') && (
                <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                  ✓ Gestión completa
                </span>
              )}
              {hasPermission('upload_media') && (
                <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                  ✓ Subida de medios
                </span>
              )}
              {!hasPermission('manage_propiedades') && (
                <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                  👁️ Solo lectura
                </span>
              )}
            </div>
          </div>
        </div>
      </footer>
    </div>
  );
};

export default PropiedadesAdminPanel;
