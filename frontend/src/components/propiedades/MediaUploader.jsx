import React, { useState, useEffect, useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { usePropiedadesStore } from '../../store/propiedadesStore';
import { toast } from 'react-hot-toast';
import { 
  PhotoIcon, 
  VideoCameraIcon,
  TrashIcon,
  EyeIcon,
  CloudArrowUpIcon,
  XMarkIcon
} from '@heroicons/react/24/outline';

const MediaUploader = ({ propiedadId }) => {
  const {
    mediasPropiedad,
    fetchMediasByPropiedad,
    uploadMedia,
    bulkUploadMedia,
    deleteMedia,
    reorderMedia,
    loading
  } = usePropiedadesStore();

  const [draggedItem, setDraggedItem] = useState(null);
  const [uploadProgress, setUploadProgress] = useState({});
  const [previewModal, setPreviewModal] = useState(null);

  useEffect(() => {
    if (propiedadId) {
      fetchMediasByPropiedad(propiedadId);
    }
  }, [propiedadId, fetchMediasByPropiedad]);

  const onDrop = useCallback(async (acceptedFiles) => {
    if (!propiedadId) {
      toast.error('Primero debe guardar la propiedad');
      return;
    }

    // Filtrar solo archivos de imagen y video
    const validFiles = acceptedFiles.filter(file => {
      const isValid = file.type.startsWith('image/') || file.type.startsWith('video/');
      if (!isValid) {
        toast.error(`${file.name} no es un archivo válido`);
      }
      return isValid;
    });

    if (validFiles.length === 0) return;

    try {
      if (validFiles.length === 1) {
        // Subida individual
        setUploadProgress({ [validFiles[0].name]: 0 });
        await uploadMedia(propiedadId, validFiles[0]);
        setUploadProgress({});
      } else {
        // Subida múltiple
        const progressInit = {};
        validFiles.forEach(file => {
          progressInit[file.name] = 0;
        });
        setUploadProgress(progressInit);

        await bulkUploadMedia(propiedadId, validFiles);
        setUploadProgress({});
      }
    } catch (error) {
      console.error('Error al subir archivos:', error);
      setUploadProgress({});
    }
  }, [propiedadId, uploadMedia, bulkUploadMedia]);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'image/*': ['.jpeg', '.jpg', '.png', '.webp', '.gif'],
      'video/*': ['.mp4', '.avi', '.mov', '.wmv']
    },
    multiple: true,
    maxSize: 50 * 1024 * 1024 // 50MB
  });

  const handleDelete = async (mediaId) => {
    if (window.confirm('¿Está seguro de que desea eliminar esta imagen/video?')) {
      try {
        await deleteMedia(mediaId);
      } catch (error) {
        console.error('Error al eliminar media:', error);
      }
    }
  };

  const handleDragStart = (e, media) => {
    setDraggedItem(media);
    e.dataTransfer.effectAllowed = 'move';
  };

  const handleDragOver = (e) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
  };

  const handleDrop = async (e, targetMedia) => {
    e.preventDefault();
    
    if (!draggedItem || draggedItem.id === targetMedia.id) {
      setDraggedItem(null);
      return;
    }

    try {
      // Crear nuevo orden
      const sortedMedias = [...mediasPropiedad].sort((a, b) => a.orden - b.orden);
      const draggedIndex = sortedMedias.findIndex(m => m.id === draggedItem.id);
      const targetIndex = sortedMedias.findIndex(m => m.id === targetMedia.id);

      if (draggedIndex === -1 || targetIndex === -1) return;

      // Reordenar array
      const newOrder = [...sortedMedias];
      const [draggedMedia] = newOrder.splice(draggedIndex, 1);
      newOrder.splice(targetIndex, 0, draggedMedia);

      // Actualizar orden
      const ordenItems = newOrder.map((media, index) => ({
        id: media.id,
        orden: index + 1
      }));

      await reorderMedia(propiedadId, ordenItems);
    } catch (error) {
      console.error('Error al reordenar:', error);
    }

    setDraggedItem(null);
  };

  const openPreview = (media) => {
    setPreviewModal(media);
  };

  const isImage = (media) => {
    return media.tipo === 'image' || media.tipoArchivo?.match(/^(jpg|jpeg|png|gif|webp|bmp)$/i);
  };

  const isVideo = (media) => {
    return media.tipo === 'video' || media.tipoArchivo?.match(/^(mp4|avi|mov|wmv)$/i);
  };

  const getMediaUrl = (media) => {
    if (media.url.startsWith('http')) {
      return media.url;
    }
    // Para archivos locales, construir URL completa
    return `${process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5174'}${media.url}`;
  };

  const sortedMedias = [...mediasPropiedad].sort((a, b) => a.orden - b.orden);

  return (
    <div className="space-y-6">
      {/* Zona de Drop */}
      <div
        {...getRootProps()}
        className={`border-2 border-dashed rounded-lg p-8 text-center transition-colors ${
          isDragActive
            ? 'border-blue-500 bg-blue-50'
            : 'border-gray-300 hover:border-gray-400'
        }`}
      >
        <input {...getInputProps()} />
        <CloudArrowUpIcon className="mx-auto h-12 w-12 text-gray-400" />
        <div className="mt-2">
          <p className="text-sm text-gray-600">
            {isDragActive
              ? 'Suelta los archivos aquí...'
              : 'Arrastra imágenes y videos aquí, o haz clic para seleccionar'}
          </p>
          <p className="text-xs text-gray-500 mt-1">
            Formatos soportados: JPG, PNG, WebP, GIF, MP4, AVI, MOV, WMV (máx. 50MB)
          </p>
        </div>
      </div>

      {/* Progreso de subida */}
      {Object.keys(uploadProgress).length > 0 && (
        <div className="space-y-2">
          <h4 className="font-medium text-gray-900">Subiendo archivos...</h4>
          {Object.entries(uploadProgress).map(([fileName, progress]) => (
            <div key={fileName} className="bg-gray-50 rounded-lg p-3">
              <div className="flex justify-between items-center mb-1">
                <span className="text-sm text-gray-700">{fileName}</span>
                <span className="text-sm text-gray-500">{progress}%</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                  style={{ width: `${progress}%` }}
                />
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Grid de Medias */}
      {sortedMedias.length > 0 && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h4 className="font-medium text-gray-900">
              Imágenes y Videos ({sortedMedias.length})
            </h4>
            <p className="text-sm text-gray-500">
              Arrastra para reordenar
            </p>
          </div>

          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {sortedMedias.map((media) => (
              <div
                key={media.id}
                draggable
                onDragStart={(e) => handleDragStart(e, media)}
                onDragOver={handleDragOver}
                onDrop={(e) => handleDrop(e, media)}
                className={`relative group bg-white rounded-lg shadow-sm border-2 transition-all cursor-move ${
                  draggedItem?.id === media.id
                    ? 'border-blue-500 shadow-lg scale-105'
                    : 'border-gray-200 hover:border-gray-300'
                }`}
              >
                {/* Thumbnail */}
                <div className="aspect-video rounded-t-lg overflow-hidden bg-gray-100">
                  {isImage(media) ? (
                    <img
                      src={getMediaUrl(media)}
                      alt={media.titulo || 'Imagen'}
                      className="w-full h-full object-cover"
                      onError={(e) => {
                        console.error('Error al cargar imagen:', e.target.src);
                        e.target.style.display = 'none';
                        e.target.parentElement.innerHTML = `
                          <div class="w-full h-full flex items-center justify-center bg-gray-200">
                            <div class="text-center">
                              <svg class="mx-auto h-8 w-8 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                              </svg>
                              <p class="mt-2 text-xs text-gray-500">Error al cargar</p>
                            </div>
                          </div>
                        `;
                      }}
                    />
                  ) : isVideo(media) ? (
                    <div className="w-full h-full flex items-center justify-center bg-gray-200">
                      <VideoCameraIcon className="h-8 w-8 text-gray-400" />
                      <span className="ml-2 text-sm text-gray-600">Video</span>
                    </div>
                  ) : (
                    <div className="w-full h-full flex items-center justify-center bg-gray-200">
                      <PhotoIcon className="h-8 w-8 text-gray-400" />
                    </div>
                  )}
                </div>

                {/* Información */}
                <div className="p-3">
                  <h5 className="text-sm font-medium text-gray-900 truncate">
                    {media.titulo || `Media ${media.id}`}
                  </h5>
                  <div className="flex items-center justify-between mt-2">
                    <span className="text-xs text-gray-500 capitalize">
                      {media.tipo} • #{media.orden}
                    </span>
                    {media.esPrincipal && (
                      <span className="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded">
                        Principal
                      </span>
                    )}
                  </div>
                </div>

                {/* Overlay con acciones */}
                <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-50 transition-all duration-200 rounded-lg flex items-center justify-center opacity-0 group-hover:opacity-100">
                  <div className="flex space-x-2">
                    <button
                      onClick={() => openPreview(media)}
                      className="p-2 bg-white rounded-full shadow-lg hover:bg-gray-50 transition-colors"
                      title="Ver"
                    >
                      <EyeIcon className="h-4 w-4 text-gray-600" />
                    </button>
                    <button
                      onClick={() => handleDelete(media.id)}
                      className="p-2 bg-white rounded-full shadow-lg hover:bg-gray-50 transition-colors"
                      title="Eliminar"
                    >
                      <TrashIcon className="h-4 w-4 text-red-600" />
                    </button>
                  </div>
                </div>

                {/* Indicador de orden */}
                <div className="absolute top-2 left-2 bg-black bg-opacity-70 text-white text-xs px-2 py-1 rounded">
                  {media.orden}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Modal de Preview */}
      {previewModal && (
        <div className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50 p-4">
          <div className="relative max-w-4xl max-h-full bg-white rounded-lg overflow-hidden">
            <div className="absolute top-4 right-4 z-10">
              <button
                onClick={() => setPreviewModal(null)}
                className="p-2 bg-black bg-opacity-50 text-white rounded-full hover:bg-opacity-70 transition-colors"
              >
                <XMarkIcon className="h-6 w-6" />
              </button>
            </div>
            
            <div className="p-4">
              {isImage(previewModal) ? (
                <img
                  src={getMediaUrl(previewModal)}
                  alt={previewModal.titulo || 'Preview'}
                  className="max-w-full max-h-96 object-contain mx-auto"
                  onError={(e) => {
                    console.error('Error al cargar imagen en preview:', e.target.src);
                    e.target.style.display = 'none';
                    e.target.parentElement.innerHTML = `
                      <div class="text-center py-8">
                        <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                        </svg>
                        <p class="mt-2 text-sm text-gray-500">No se pudo cargar la imagen</p>
                        <p class="text-xs text-gray-400">Verifica tu ad blocker o extensiones del navegador</p>
                      </div>
                    `;
                  }}
                />
              ) : isVideo(previewModal) ? (
                <video
                  src={getMediaUrl(previewModal)}
                  controls
                  className="max-w-full max-h-96 mx-auto"
                />
              ) : (
                <div className="text-center py-8">
                  <p className="text-gray-600">Vista previa no disponible</p>
                </div>
              )}
              
              <div className="mt-4 text-center">
                <h3 className="font-medium text-gray-900">
                  {previewModal.titulo || `Media ${previewModal.id}`}
                </h3>
                <p className="text-sm text-gray-600 mt-1">
                  Tipo: {previewModal.tipo} • Orden: {previewModal.orden}
                </p>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default MediaUploader;
