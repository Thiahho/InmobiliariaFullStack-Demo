'use client';

import React from 'react';
import { usePropiedadesStore } from '../../store/propiedadesStore';

const TestPropiedades = () => {
  const { propiedades, loading, error, fetchPropiedades } = usePropiedadesStore();

  const handleTest = () => {
    console.log('Testing propiedades store...');
    fetchPropiedades().then(() => {
      console.log('Propiedades cargadas:', propiedades.length);
    }).catch((err: any) => {
      console.error('Error:', err);
    });
  };

  return (
    <div className="bg-white p-6 rounded-lg shadow-lg">
      <h3 className="text-lg font-bold mb-4">Test Módulo Propiedades</h3>
      
      <div className="space-y-4">
        <button
          onClick={handleTest}
          className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
          disabled={loading}
        >
          {loading ? 'Cargando...' : 'Probar Conexión'}
        </button>

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-md p-4">
            <p className="text-red-800">Error: {error}</p>
          </div>
        )}

        {propiedades.length > 0 && (
          <div className="bg-green-50 border border-green-200 rounded-md p-4">
            <p className="text-green-800">
              ✅ Conexión exitosa! Se encontraron {propiedades.length} propiedades.
            </p>
          </div>
        )}

        <div className="text-sm text-gray-600">
          <p><strong>Estado:</strong> {loading ? 'Cargando' : 'Listo'}</p>
          <p><strong>Propiedades:</strong> {propiedades.length}</p>
          <p><strong>Error:</strong> {error || 'Ninguno'}</p>
        </div>
      </div>
    </div>
  );
};

export default TestPropiedades;
