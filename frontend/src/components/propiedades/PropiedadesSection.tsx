'use client';

import React, { useState } from 'react';
import { useAuthStore } from '../../store/authStore';
import PropiedadesModule from './PropiedadesModule';

const PropiedadesSection = () => {
  const { hasPermission } = useAuthStore();
  const [showModule, setShowModule] = useState(false);

  if (!hasPermission('manage_propiedades') && !hasPermission('view_dashboard')) {
    return (
      <div className="bg-white overflow-hidden shadow rounded-lg opacity-50">
        <div className="p-5">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <div className="w-8 h-8 bg-gray-400 rounded-lg flex items-center justify-center">
                🏠
              </div>
            </div>
            <div className="ml-5 w-0 flex-1">
              <dl>
                <dt className="text-sm font-medium text-gray-500 truncate">
                  Propiedades
                </dt>
                <dd className="text-lg font-medium text-gray-400">
                  Sin permisos de acceso
                </dd>
              </dl>
            </div>
          </div>
        </div>
        <div className="bg-gray-50 px-5 py-3">
          <div className="text-sm">
            <span className="text-gray-400">Contacta al administrador</span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <>
      {/* Card de Propiedades */}
      <div className="bg-white overflow-hidden shadow rounded-lg">
        <div className="p-5">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <div className="w-8 h-8 bg-blue-500 rounded-lg flex items-center justify-center">
                🏠
              </div>
            </div>
            <div className="ml-5 w-0 flex-1">
              <dl>
                <dt className="text-sm font-medium text-gray-500 truncate">
                  Propiedades
                </dt>
                <dd className="text-lg font-medium text-gray-900">
                  Gestionar propiedades
                </dd>
              </dl>
            </div>
          </div>
        </div>
        <div className="bg-gray-50 px-5 py-3">
          <div className="text-sm">
            <button 
              onClick={() => setShowModule(true)}
              className="font-medium text-blue-600 hover:text-blue-500 transition-colors"
            >
              Ver todas las propiedades →
            </button>
          </div>
        </div>
      </div>

      {/* Modal del módulo de propiedades */}
      {showModule && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 overflow-y-auto">
          <div className="min-h-screen px-4 py-8">
            <div className="max-w-7xl mx-auto bg-white rounded-lg shadow-xl overflow-hidden">
              {/* Header del modal */}
              <div className="flex justify-between items-center p-6 border-b border-gray-200 bg-white">
                <div>
                  <h1 className="text-2xl font-bold text-gray-900">
                    Gestión de Propiedades
                  </h1>
                  <p className="text-gray-600">
                    Administra el catálogo completo de propiedades
                  </p>
                </div>
                
                <button
                  onClick={() => setShowModule(false)}
                  className="inline-flex items-center p-2 border border-transparent rounded-full text-gray-400 hover:text-gray-600 hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                >
                  <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>

              {/* Contenido del módulo */}
              <div className="max-h-[calc(100vh-200px)] overflow-y-auto">
                <PropiedadesModule />
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
};

export default PropiedadesSection;
