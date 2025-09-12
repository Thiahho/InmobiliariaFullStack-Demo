import React, { useState } from 'react';
import { useAuthStore } from '../../store/authStore';
import { usePropiedadesStore } from '../../store/propiedadesStore';
import PropiedadesList from './PropiedadesList';
import PropiedadForm from './PropiedadForm';
import PropiedadDetail from './PropiedadDetail';
import { toast } from 'react-hot-toast';
import {
  ArrowLeftIcon,
  PlusIcon,
  ListBulletIcon,
  PencilIcon,
  EyeIcon,
  TrashIcon
} from '@heroicons/react/24/outline';

const PropiedadesModule = () => {
  const { hasPermission } = useAuthStore();
  const { fetchPropiedades, deletePropiedad } = usePropiedadesStore();
  
  const [currentView, setCurrentView] = useState('list'); // 'list', 'create', 'edit', 'detail'
  const [selectedPropiedad, setSelectedPropiedad] = useState(null);

  const handleCreateNew = () => {
    if (!hasPermission('manage_propiedades')) {
      toast.error('No tiene permisos para crear propiedades');
      return;
    }
    setSelectedPropiedad(null);
    setCurrentView('create');
  };

  const handleEdit = (propiedad) => {
    if (!hasPermission('manage_propiedades')) {
      toast.error('No tiene permisos para editar propiedades');
      return;
    }
    setSelectedPropiedad(propiedad);
    setCurrentView('edit');
  };

  const handleView = (propiedad) => {
    setSelectedPropiedad(propiedad);
    setCurrentView('detail');
  };

  const handleDelete = async (propiedad) => {
    if (!hasPermission('manage_propiedades')) {
      toast.error('No tiene permisos para eliminar propiedades');
      return;
    }

    const confirmMessage = `¿Está seguro de que desea eliminar la propiedad "${propiedad.codigo}"?\n\nEsta acción no se puede deshacer.`;
    
    if (window.confirm(confirmMessage)) {
      try {
        await deletePropiedad(propiedad.id);
        await fetchPropiedades();
        toast.success('Propiedad eliminada exitosamente');
      } catch (error) {
        console.error('Error al eliminar propiedad:', error);
        toast.error('Error al eliminar la propiedad');
      }
    }
  };

  const handleBackToList = () => {
    setSelectedPropiedad(null);
    setCurrentView('list');
  };

  const handleFormSuccess = async () => {
    toast.success(currentView === 'create' ? 'Propiedad creada exitosamente' : 'Propiedad actualizada exitosamente');
    // Refrescar la lista de propiedades
    await fetchPropiedades();
    handleBackToList();
  };

  const getPageTitle = () => {
    switch (currentView) {
      case 'create':
        return 'Nueva Propiedad';
      case 'edit':
        return `Editar: ${selectedPropiedad?.codigo || 'Propiedad'}`;
      case 'detail':
        return 'Detalle de Propiedad';
      default:
        return 'Gestión de Propiedades';
    }
  };

  const getBreadcrumb = () => {
    const breadcrumb = [{ label: 'Propiedades', onClick: handleBackToList }];
    
    switch (currentView) {
      case 'create':
        breadcrumb.push({ label: 'Nueva Propiedad' });
        break;
      case 'edit':
        breadcrumb.push({ label: `Editar ${selectedPropiedad?.codigo}` });
        break;
      case 'detail':
        breadcrumb.push({ label: `${selectedPropiedad?.codigo}` });
        break;
    }
    
    return breadcrumb;
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-6">
            <div className="flex items-center space-x-4">
              {currentView !== 'list' && (
                <button
                  onClick={handleBackToList}
                  className="inline-flex items-center p-2 border border-transparent text-sm leading-4 font-medium rounded-md text-gray-500 hover:text-gray-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                >
                  <ArrowLeftIcon className="h-5 w-5 mr-2" />
                  Volver
                </button>
              )}
              
              <div>
                <h1 className="text-2xl font-bold text-gray-900">{getPageTitle()}</h1>
                
                {/* Breadcrumb */}
                <nav className="flex mt-1" aria-label="Breadcrumb">
                  <ol className="inline-flex items-center space-x-1 md:space-x-3">
                    {getBreadcrumb().map((item, index) => (
                      <li key={index} className="inline-flex items-center">
                        {index > 0 && (
                          <span className="mx-2 text-gray-400">/</span>
                        )}
                        {item.onClick ? (
                          <button
                            onClick={item.onClick}
                            className="text-sm text-blue-600 hover:text-blue-800"
                          >
                            {item.label}
                          </button>
                        ) : (
                          <span className="text-sm text-gray-500">{item.label}</span>
                        )}
                      </li>
                    ))}
                  </ol>
                </nav>
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center space-x-3">
              {currentView === 'list' && hasPermission('manage_propiedades') && (
                <button
                  onClick={handleCreateNew}
                  className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                >
                  <PlusIcon className="h-4 w-4 mr-2" />
                  Nueva Propiedad
                </button>
              )}

              {currentView === 'detail' && hasPermission('manage_propiedades') && (
                <button
                  onClick={() => handleEdit(selectedPropiedad)}
                  className="inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md shadow-sm text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                >
                  <PencilIcon className="h-4 w-4 mr-2" />
                  Editar
                </button>
              )}

              {(currentView === 'create' || currentView === 'edit') && (
                <button
                  onClick={handleBackToList}
                  className="inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md shadow-sm text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                >
                  <ListBulletIcon className="h-4 w-4 mr-2" />
                  Ver Lista
                </button>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {currentView === 'list' && (
          <PropiedadesList
            onEdit={handleEdit}
            onView={handleView}
            onCreate={handleCreateNew}
            onDelete={handleDelete}
          />
        )}

        {currentView === 'create' && (
          <div className="max-w-4xl mx-auto">
            <PropiedadForm
              onSuccess={handleFormSuccess}
            />
          </div>
        )}

        {currentView === 'edit' && selectedPropiedad && (
          <div className="max-w-4xl mx-auto">
            <PropiedadForm
              propiedadId={selectedPropiedad.id}
              onSuccess={handleFormSuccess}
            />
          </div>
        )}
      </div>

      {/* Modal de detalle */}
      {currentView === 'detail' && selectedPropiedad && (
        <PropiedadDetail
          propiedadId={selectedPropiedad.id}
          onClose={handleBackToList}
        />
      )}

      {/* Help Panel - Solo para usuarios con permisos limitados */}
      {!hasPermission('manage_propiedades') && currentView === 'list' && (
        <div className="fixed bottom-4 right-4 max-w-sm">
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 shadow-lg">
            <div className="flex items-start">
              <div className="flex-shrink-0">
                <EyeIcon className="h-5 w-5 text-blue-600" />
              </div>
              <div className="ml-3">
                <h3 className="text-sm font-medium text-blue-800">Modo Solo Lectura</h3>
                <p className="mt-1 text-sm text-blue-700">
                  Puedes ver las propiedades pero no crear o editar. Contacta al administrador para más permisos.
                </p>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Loading Overlay */}
      {false && ( // Se puede activar cuando hay operaciones que requieren loading global
        <div className="fixed inset-0 bg-black bg-opacity-25 z-50 flex items-center justify-center">
          <div className="bg-white rounded-lg p-6 shadow-xl">
            <div className="flex items-center space-x-4">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
              <span className="text-gray-700">Procesando...</span>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default PropiedadesModule;

