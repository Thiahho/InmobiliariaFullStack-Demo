import React, { useState } from 'react';
import { usePropiedadesStore } from '../../store/propiedadesStore';
import { toast } from 'react-hot-toast';
import {
  LinkIcon,
  VideoCameraIcon,
  PhotoIcon,
  GlobeAltIcon,
  CheckCircleIcon,
  XCircleIcon,
  PlusIcon
} from '@heroicons/react/24/outline';

const ExternalUrlManager = ({ propiedadId }) => {
  const { addExternalUrl, validateUrl, loading } = usePropiedadesStore();
  
  const [newUrl, setNewUrl] = useState('');
  const [newTitulo, setNewTitulo] = useState('');
  const [selectedTipo, setSelectedTipo] = useState('image');
  const [validationResult, setValidationResult] = useState(null);
  const [isValidating, setIsValidating] = useState(false);

  const tiposDisponibles = [
    { value: 'image', label: 'Imagen', icon: PhotoIcon },
    { value: 'video', label: 'Video', icon: VideoCameraIcon },
    { value: 'tour', label: 'Tour Virtual', icon: GlobeAltIcon }
  ];

  const urlExamples = {
    image: [
      'https://drive.google.com/file/d/ID_DEL_ARCHIVO/view',
      'https://imgur.com/imagen.jpg',
      'https://example.com/imagen.png'
    ],
    video: [
      'https://www.youtube.com/watch?v=VIDEO_ID',
      'https://youtu.be/VIDEO_ID',
      'https://vimeo.com/VIDEO_ID',
      'https://drive.google.com/file/d/ID_DEL_VIDEO/view'
    ],
    tour: [
      'https://matterport.com/tour/ID_DEL_TOUR',
      'https://example.com/tour-360'
    ]
  };

  const handleValidateUrl = async () => {
    if (!newUrl) {
      toast.error('Ingrese una URL');
      return;
    }

    setIsValidating(true);
    try {
      const result = await validateUrl(newUrl);
      setValidationResult(result);
      
      if (result.valid) {
        // Auto-detectar tipo si no está seleccionado
        if (result.tipo && result.tipo !== selectedTipo) {
          setSelectedTipo(result.tipo);
        }
        
        // Auto-generar título si no hay uno
        if (!newTitulo && result.processedUrl) {
          const autoTitle = generateAutoTitle(result.processedUrl, result.tipo);
          setNewTitulo(autoTitle);
        }
      }
    } catch (error) {
      console.error('Error al validar URL:', error);
      setValidationResult({ valid: false, message: 'Error al validar URL' });
    } finally {
      setIsValidating(false);
    }
  };

  const generateAutoTitle = (url, tipo) => {
    try {
      if (url.includes('youtube.com') || url.includes('youtu.be')) {
        return 'Video de YouTube';
      }
      if (url.includes('vimeo.com')) {
        return 'Video de Vimeo';
      }
      if (url.includes('drive.google.com')) {
        return `Archivo de Google Drive (${tipo})`;
      }
      if (url.includes('matterport.com')) {
        return 'Tour Virtual 3D';
      }
      
      // Intentar extraer nombre del archivo
      const urlObj = new URL(url);
      const pathParts = urlObj.pathname.split('/');
      const fileName = pathParts[pathParts.length - 1];
      if (fileName && fileName.includes('.')) {
        return fileName.split('.')[0];
      }
      
      return `${tipo.charAt(0).toUpperCase() + tipo.slice(1)} externa`;
    } catch {
      return `${tipo.charAt(0).toUpperCase() + tipo.slice(1)} externa`;
    }
  };

  const handleAddUrl = async () => {
    if (!newUrl) {
      toast.error('Ingrese una URL');
      return;
    }

    if (!propiedadId) {
      toast.error('Primero debe guardar la propiedad');
      return;
    }

    try {
      const urlData = {
        url: newUrl,
        titulo: newTitulo || generateAutoTitle(newUrl, selectedTipo),
        tipo: selectedTipo,
        orden: 0 // El backend calculará el orden automáticamente
      };

      await addExternalUrl(propiedadId, urlData);
      
      // Limpiar formulario
      setNewUrl('');
      setNewTitulo('');
      setSelectedTipo('image');
      setValidationResult(null);
      
      toast.success('URL externa agregada exitosamente');
    } catch (error) {
      console.error('Error al agregar URL:', error);
    }
  };

  const handleUrlChange = (e) => {
    setNewUrl(e.target.value);
    setValidationResult(null); // Limpiar validación previa
  };

  const isValidUrl = (url) => {
    try {
      new URL(url);
      return true;
    } catch {
      return false;
    }
  };

  return (
    <div className="space-y-6">
      <div className="bg-gray-50 rounded-lg p-6">
        <h4 className="text-lg font-medium text-gray-900 mb-4">
          Agregar URL Externa
        </h4>
        
        <div className="space-y-4">
          {/* Selector de tipo */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Tipo de contenido
            </label>
            <div className="grid grid-cols-3 gap-3">
              {tiposDisponibles.map((tipo) => {
                const IconComponent = tipo.icon;
                return (
                  <button
                    key={tipo.value}
                    type="button"
                    onClick={() => setSelectedTipo(tipo.value)}
                    className={`flex items-center justify-center p-3 rounded-lg border-2 transition-colors ${
                      selectedTipo === tipo.value
                        ? 'border-blue-500 bg-blue-50 text-blue-700'
                        : 'border-gray-200 hover:border-gray-300 text-gray-600'
                    }`}
                  >
                    <IconComponent className="h-5 w-5 mr-2" />
                    <span className="text-sm font-medium">{tipo.label}</span>
                  </button>
                );
              })}
            </div>
          </div>

          {/* Campo URL */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              URL del archivo
            </label>
            <div className="flex space-x-2">
              <div className="flex-1 relative">
                <input
                  type="url"
                  value={newUrl}
                  onChange={handleUrlChange}
                  placeholder={`Pegue aquí la URL de ${tiposDisponibles.find(t => t.value === selectedTipo)?.label.toLowerCase()}`}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
                />
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <LinkIcon className="h-5 w-5 text-gray-400" />
                </div>
              </div>
              <button
                type="button"
                onClick={handleValidateUrl}
                disabled={!newUrl || isValidating}
                className="px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isValidating ? 'Validando...' : 'Validar'}
              </button>
            </div>
          </div>

          {/* Resultado de validación */}
          {validationResult && (
            <div className={`flex items-center p-3 rounded-md ${
              validationResult.valid 
                ? 'bg-green-50 text-green-800 border border-green-200'
                : 'bg-red-50 text-red-800 border border-red-200'
            }`}>
              {validationResult.valid ? (
                <CheckCircleIcon className="h-5 w-5 mr-2" />
              ) : (
                <XCircleIcon className="h-5 w-5 mr-2" />
              )}
              <span className="text-sm">{validationResult.message}</span>
            </div>
          )}

          {/* Campo título */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Título (opcional)
            </label>
            <input
              type="text"
              value={newTitulo}
              onChange={(e) => setNewTitulo(e.target.value)}
              placeholder="Título descriptivo del contenido"
              className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          {/* Botón agregar */}
          <button
            type="button"
            onClick={handleAddUrl}
            disabled={!newUrl || !isValidUrl(newUrl) || loading}
            className="w-full flex items-center justify-center px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <PlusIcon className="h-5 w-5 mr-2" />
            {loading ? 'Agregando...' : 'Agregar URL Externa'}
          </button>
        </div>
      </div>

      {/* Ejemplos de URLs */}
      <div className="bg-blue-50 rounded-lg p-4">
        <h5 className="font-medium text-blue-900 mb-2">
          Ejemplos de URLs para {tiposDisponibles.find(t => t.value === selectedTipo)?.label}:
        </h5>
        <ul className="space-y-1">
          {urlExamples[selectedTipo]?.map((example, index) => (
            <li key={index} className="text-sm text-blue-700 font-mono">
              {example}
            </li>
          ))}
        </ul>
      </div>

      {/* Notas importantes */}
      <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
        <h5 className="font-medium text-yellow-800 mb-2">Notas importantes:</h5>
        <ul className="text-sm text-yellow-700 space-y-1">
          <li>• Para Google Drive: Asegúrese de que el archivo sea público o tenga permisos de visualización</li>
          <li>• Para YouTube: Puede usar URLs normales o enlaces acortados (youtu.be)</li>
          <li>• Las URLs se procesarán automáticamente para optimizar la visualización</li>
          <li>• El sistema detectará automáticamente el tipo de contenido cuando sea posible</li>
        </ul>
      </div>
    </div>
  );
};

export default ExternalUrlManager;
