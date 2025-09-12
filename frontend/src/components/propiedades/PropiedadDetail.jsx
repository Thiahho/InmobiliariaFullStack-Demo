import React, { useEffect, useState } from 'react';
import { usePropiedadesStore } from '../../store/propiedadesStore';
import {
  XMarkIcon,
  MapPinIcon,
  CurrencyDollarIcon,
  HomeIcon,
  CalendarIcon,
  ShareIcon,
  HeartIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  PlayIcon,
  PhotoIcon,
  GlobeAltIcon
} from '@heroicons/react/24/outline';
import { HeartIcon as HeartSolidIcon } from '@heroicons/react/24/solid';

const PropiedadDetail = ({ propiedadId, onClose }) => {
  const {
    propiedadActual,
    mediasPropiedad,
    fetchPropiedadById,
    fetchMediasByPropiedad,
    loading
  } = usePropiedadesStore();

  const [currentMediaIndex, setCurrentMediaIndex] = useState(0);
  const [isFavorite, setIsFavorite] = useState(false);
  const [activeTab, setActiveTab] = useState('descripcion');

  useEffect(() => {
    if (propiedadId) {
      fetchPropiedadById(propiedadId);
      fetchMediasByPropiedad(propiedadId);
    }
  }, [propiedadId, fetchPropiedadById, fetchMediasByPropiedad]);

  if (loading || !propiedadActual) {
    return (
      <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center">
        <div className="bg-white rounded-lg p-8">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600 mx-auto"></div>
          <p className="text-center mt-4">Cargando propiedad...</p>
        </div>
      </div>
    );
  }

  const formatPrice = (price, currency = 'USD') => {
    const formatter = new Intl.NumberFormat('es-AR', {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    });
    return formatter.format(price);
  };

  const formatDate = (date) => {
    return new Date(date).toLocaleDateString('es-AR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  const getMediaUrl = (media) => {
    if (media.url.startsWith('http')) {
      return media.url;
    }
    return `${process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5174'}${media.url}`;
  };

  const isImage = (media) => {
    return media.tipo === 'image' || media.tipoArchivo?.match(/^(jpg|jpeg|png|gif|webp|bmp)$/i);
  };

  const isVideo = (media) => {
    return media.tipo === 'video' || media.tipoArchivo?.match(/^(mp4|avi|mov|wmv)$/i);
  };

  const isExternalVideo = (media) => {
    return media.url.includes('youtube.com') || media.url.includes('vimeo.com') || media.url.includes('youtu.be');
  };

  const isTour = (media) => {
    return media.tipo === 'tour';
  };

  const nextMedia = () => {
    setCurrentMediaIndex((prev) => 
      prev < mediasPropiedad.length - 1 ? prev + 1 : 0
    );
  };

  const prevMedia = () => {
    setCurrentMediaIndex((prev) => 
      prev > 0 ? prev - 1 : mediasPropiedad.length - 1
    );
  };

  const handleShare = async () => {
    if (navigator.share) {
      try {
        await navigator.share({
          title: propiedadActual.titulo || `${propiedadActual.tipo} en ${propiedadActual.barrio}`,
          text: `${propiedadActual.tipo} de ${propiedadActual.ambientes} ambientes en ${propiedadActual.barrio}`,
          url: window.location.href,
        });
      } catch (error) {
        console.log('Error sharing:', error);
      }
    } else {
      // Fallback: copiar al portapapeles
      navigator.clipboard.writeText(window.location.href);
      alert('Link copiado al portapapeles');
    }
  };

  const sortedMedias = [...mediasPropiedad].sort((a, b) => a.orden - b.orden);
  const currentMedia = sortedMedias[currentMediaIndex];

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 z-50 overflow-y-auto">
      <div className="min-h-screen px-4 py-8">
        <div className="max-w-6xl mx-auto bg-white rounded-lg shadow-xl overflow-hidden">
          {/* Header */}
          <div className="flex justify-between items-center p-6 border-b border-gray-200">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">
                {propiedadActual.titulo || `${propiedadActual.tipo} en ${propiedadActual.barrio}`}
              </h1>
              <div className="flex items-center mt-2 text-gray-600">
                <MapPinIcon className="h-5 w-5 mr-1" />
                {propiedadActual.direccion}, {propiedadActual.barrio}, {propiedadActual.comuna}
              </div>
            </div>
            
            <div className="flex items-center space-x-2">
              <button
                onClick={handleShare}
                className="p-2 text-gray-400 hover:text-gray-600 rounded-full hover:bg-gray-100"
                title="Compartir"
              >
                <ShareIcon className="h-6 w-6" />
              </button>
              
              <button
                onClick={() => setIsFavorite(!isFavorite)}
                className="p-2 text-gray-400 hover:text-red-500 rounded-full hover:bg-gray-100"
                title="Agregar a favoritos"
              >
                {isFavorite ? (
                  <HeartSolidIcon className="h-6 w-6 text-red-500" />
                ) : (
                  <HeartIcon className="h-6 w-6" />
                )}
              </button>
              
              <button
                onClick={onClose}
                className="p-2 text-gray-400 hover:text-gray-600 rounded-full hover:bg-gray-100"
              >
                <XMarkIcon className="h-6 w-6" />
              </button>
            </div>
          </div>

          <div className="flex flex-col lg:flex-row">
            {/* Galería de medios */}
            <div className="lg:w-2/3">
              {sortedMedias.length > 0 ? (
                <div className="relative">
                  {/* Media principal */}
                  <div className="aspect-video bg-gray-100">
                    {currentMedia && isImage(currentMedia) && (
                      <img
                        src={getMediaUrl(currentMedia)}
                        alt={currentMedia.titulo || 'Imagen'}
                        className="w-full h-full object-cover"
                        onError={(e) => {
                          e.target.src = '/placeholder-property.jpg';
                        }}
                      />
                    )}
                    
                    {currentMedia && isVideo(currentMedia) && !isExternalVideo(currentMedia) && (
                      <video
                        src={getMediaUrl(currentMedia)}
                        controls
                        className="w-full h-full object-cover"
                      />
                    )}
                    
                    {currentMedia && isExternalVideo(currentMedia) && (
                      <div className="w-full h-full">
                        <iframe
                          src={getMediaUrl(currentMedia)}
                          className="w-full h-full"
                          frameBorder="0"
                          allowFullScreen
                        />
                      </div>
                    )}
                    
                    {currentMedia && isTour(currentMedia) && (
                      <div className="w-full h-full">
                        <iframe
                          src={getMediaUrl(currentMedia)}
                          className="w-full h-full"
                          frameBorder="0"
                          allowFullScreen
                        />
                      </div>
                    )}
                  </div>

                  {/* Controles de navegación */}
                  {sortedMedias.length > 1 && (
                    <>
                      <button
                        onClick={prevMedia}
                        className="absolute left-4 top-1/2 transform -translate-y-1/2 bg-black bg-opacity-50 text-white p-2 rounded-full hover:bg-opacity-70 transition-all"
                      >
                        <ChevronLeftIcon className="h-6 w-6" />
                      </button>
                      
                      <button
                        onClick={nextMedia}
                        className="absolute right-4 top-1/2 transform -translate-y-1/2 bg-black bg-opacity-50 text-white p-2 rounded-full hover:bg-opacity-70 transition-all"
                      >
                        <ChevronRightIcon className="h-6 w-6" />
                      </button>
                      
                      {/* Contador */}
                      <div className="absolute bottom-4 right-4 bg-black bg-opacity-50 text-white px-3 py-1 rounded-full text-sm">
                        {currentMediaIndex + 1} / {sortedMedias.length}
                      </div>
                    </>
                  )}

                  {/* Indicador de tipo de media */}
                  {currentMedia && (
                    <div className="absolute top-4 left-4 bg-black bg-opacity-50 text-white px-3 py-1 rounded-full text-sm flex items-center">
                      {isImage(currentMedia) && <PhotoIcon className="h-4 w-4 mr-1" />}
                      {isVideo(currentMedia) && <PlayIcon className="h-4 w-4 mr-1" />}
                      {isTour(currentMedia) && <GlobeAltIcon className="h-4 w-4 mr-1" />}
                      {currentMedia.tipo.charAt(0).toUpperCase() + currentMedia.tipo.slice(1)}
                    </div>
                  )}
                </div>
              ) : (
                <div className="aspect-video bg-gray-100 flex items-center justify-center">
                  <div className="text-center">
                    <PhotoIcon className="h-12 w-12 text-gray-400 mx-auto mb-2" />
                    <p className="text-gray-500">Sin imágenes disponibles</p>
                  </div>
                </div>
              )}

              {/* Thumbnails */}
              {sortedMedias.length > 1 && (
                <div className="p-4 border-t border-gray-200">
                  <div className="flex space-x-2 overflow-x-auto">
                    {sortedMedias.map((media, index) => (
                      <button
                        key={media.id}
                        onClick={() => setCurrentMediaIndex(index)}
                        className={`flex-shrink-0 w-20 h-20 rounded-lg overflow-hidden border-2 transition-all ${
                          index === currentMediaIndex
                            ? 'border-blue-500'
                            : 'border-gray-200 hover:border-gray-300'
                        }`}
                      >
                        {isImage(media) ? (
                          <img
                            src={getMediaUrl(media)}
                            alt={`Thumbnail ${index + 1}`}
                            className="w-full h-full object-cover"
                          />
                        ) : (
                          <div className="w-full h-full bg-gray-200 flex items-center justify-center">
                            {isVideo(media) && <PlayIcon className="h-6 w-6 text-gray-500" />}
                            {isTour(media) && <GlobeAltIcon className="h-6 w-6 text-gray-500" />}
                          </div>
                        )}
                      </button>
                    ))}
                  </div>
                </div>
              )}
            </div>

            {/* Información de la propiedad */}
            <div className="lg:w-1/3 p-6 border-l border-gray-200">
              {/* Precio y datos básicos */}
              <div className="mb-6">
                <div className="flex items-center justify-between mb-4">
                  <div className="text-3xl font-bold text-green-600">
                    {formatPrice(propiedadActual.precio, propiedadActual.moneda)}
                  </div>
                  <span className="bg-blue-100 text-blue-800 px-3 py-1 rounded-full text-sm font-medium capitalize">
                    {propiedadActual.operacion}
                  </span>
                </div>
                
                {propiedadActual.expensas && (
                  <p className="text-gray-600 mb-2">
                    + {formatPrice(propiedadActual.expensas, propiedadActual.moneda)} expensas
                  </p>
                )}

                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div className="flex items-center">
                    <HomeIcon className="h-4 w-4 mr-2 text-gray-400" />
                    {propiedadActual.ambientes} ambientes
                  </div>
                  
                  {propiedadActual.dormitorios && (
                    <div>🛏️ {propiedadActual.dormitorios} dormitorios</div>
                  )}
                  
                  {propiedadActual.banos && (
                    <div>🚿 {propiedadActual.banos} baños</div>
                  )}
                  
                  {propiedadActual.metrosCubiertos && (
                    <div>📐 {propiedadActual.metrosCubiertos} m² cubiertos</div>
                  )}
                  
                  {propiedadActual.metrosTotales && (
                    <div>📏 {propiedadActual.metrosTotales} m² totales</div>
                  )}
                  
                  {propiedadActual.cochera && (
                    <div>🚗 Cochera</div>
                  )}
                </div>

                {propiedadActual.aptoCredito && (
                  <div className="mt-3">
                    <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-800">
                      💳 Apto Crédito
                    </span>
                  </div>
                )}
              </div>

              {/* Tabs */}
              <div className="border-b border-gray-200 mb-4">
                <nav className="flex space-x-8">
                  {[
                    { id: 'descripcion', label: 'Descripción' },
                    { id: 'caracteristicas', label: 'Características' },
                    { id: 'ubicacion', label: 'Ubicación' }
                  ].map((tab) => (
                    <button
                      key={tab.id}
                      onClick={() => setActiveTab(tab.id)}
                      className={`py-2 px-1 border-b-2 font-medium text-sm ${
                        activeTab === tab.id
                          ? 'border-blue-500 text-blue-600'
                          : 'border-transparent text-gray-500 hover:text-gray-700'
                      }`}
                    >
                      {tab.label}
                    </button>
                  ))}
                </nav>
              </div>

              {/* Contenido de tabs */}
              <div className="space-y-4">
                {activeTab === 'descripcion' && (
                  <div>
                    {propiedadActual.descripcion ? (
                      <p className="text-gray-700 leading-relaxed whitespace-pre-line">
                        {propiedadActual.descripcion}
                      </p>
                    ) : (
                      <p className="text-gray-500 italic">Sin descripción disponible</p>
                    )}
                  </div>
                )}

                {activeTab === 'caracteristicas' && (
                  <div className="space-y-3">
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <span className="font-medium text-gray-700">Tipo:</span>
                        <p className="text-gray-600">{propiedadActual.tipo}</p>
                      </div>
                      
                      <div>
                        <span className="font-medium text-gray-700">Estado:</span>
                        <p className="text-gray-600">{propiedadActual.estado}</p>
                      </div>
                      
                      {propiedadActual.antiguedad && (
                        <div>
                          <span className="font-medium text-gray-700">Antigüedad:</span>
                          <p className="text-gray-600">{propiedadActual.antiguedad} años</p>
                        </div>
                      )}
                      
                      {propiedadActual.piso && (
                        <div>
                          <span className="font-medium text-gray-700">Piso:</span>
                          <p className="text-gray-600">{propiedadActual.piso}°</p>
                        </div>
                      )}
                    </div>

                    {/* Amenities */}
                    {propiedadActual.amenities && Object.keys(propiedadActual.amenities).length > 0 && (
                      <div>
                        <h4 className="font-medium text-gray-700 mb-2">Amenities:</h4>
                        <div className="flex flex-wrap gap-2">
                          {Object.entries(propiedadActual.amenities).map(([amenity, value]) => (
                            value && (
                              <span
                                key={amenity}
                                className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
                              >
                                {amenity}
                              </span>
                            )
                          ))}
                        </div>
                      </div>
                    )}
                  </div>
                )}

                {activeTab === 'ubicacion' && (
                  <div className="space-y-3">
                    <div>
                      <span className="font-medium text-gray-700">Dirección completa:</span>
                      <p className="text-gray-600">
                        {propiedadActual.direccion}, {propiedadActual.barrio}, {propiedadActual.comuna}
                      </p>
                    </div>
                    
                    {(propiedadActual.latitud && propiedadActual.longitud) && (
                      <div>
                        <span className="font-medium text-gray-700">Coordenadas:</span>
                        <p className="text-gray-600 text-sm">
                          {propiedadActual.latitud}, {propiedadActual.longitud}
                        </p>
                      </div>
                    )}
                  </div>
                )}
              </div>

              {/* Información adicional */}
              <div className="mt-6 pt-6 border-t border-gray-200">
                <div className="flex items-center text-sm text-gray-500">
                  <CalendarIcon className="h-4 w-4 mr-2" />
                  Publicado el {formatDate(propiedadActual.fechaPublicacionUtc)}
                </div>
                
                <div className="mt-2 text-sm text-gray-500">
                  Código: {propiedadActual.codigo}
                </div>
              </div>

              {/* Botones de acción */}
              <div className="mt-6 space-y-3">
                <button className="w-full bg-blue-600 text-white py-3 px-4 rounded-lg hover:bg-blue-700 transition-colors font-medium">
                  Contactar por esta propiedad
                </button>
                
                <button className="w-full border border-gray-300 text-gray-700 py-3 px-4 rounded-lg hover:bg-gray-50 transition-colors font-medium">
                  Agendar visita
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PropiedadDetail;

