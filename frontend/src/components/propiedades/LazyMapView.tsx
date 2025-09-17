import React, { useState, useEffect, useRef } from 'react';
import MapView from './MapView';

interface LazyMapViewProps {
  lat: number;
  lng: number;
  address?: string;
  height?: string | number;
  zoom?: number;
  isVisible?: boolean;
}

const LazyMapView: React.FC<LazyMapViewProps> = ({
  lat,
  lng,
  address = '',
  height = '300px',
  zoom = 15,
  isVisible = true
}) => {
  const [shouldRender, setShouldRender] = useState(false);
  const [isLoaded, setIsLoaded] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (isVisible && !shouldRender) {
      // Comenzar a precargar inmediatamente cuando se indica que es visible
      setShouldRender(true);

      // Precargar recursos de Leaflet
      if (typeof window !== 'undefined') {
        Promise.all([
          import('leaflet'),
          import('react-leaflet')
        ]).then(() => {
          setTimeout(() => setIsLoaded(true), 150);
        });
      }
    }
  }, [isVisible, shouldRender]);

  // Observador de intersección para cargar el mapa cuando está cerca de ser visible
  useEffect(() => {
    if (!containerRef.current || shouldRender) return;

    const observer = new IntersectionObserver(
      (entries) => {
        const [entry] = entries;
        if (entry.isIntersecting) {
          setShouldRender(true);
        }
      },
      {
        rootMargin: '100px' // Comenzar a cargar 100px antes de que sea visible
      }
    );

    observer.observe(containerRef.current);

    return () => observer.disconnect();
  }, [shouldRender]);

  if (!shouldRender) {
    return (
      <div
        ref={containerRef}
        className="flex flex-col items-center justify-center bg-gradient-to-br from-blue-50 to-green-50 border border-gray-200 rounded-lg"
        style={{ height, width: '100%' }}
      >
        <div className="text-center p-4">
          <div className="h-6 w-6 mx-auto mb-2">📍</div>
          <div className="text-gray-600 text-sm">Mapa listo para cargar</div>
          <div className="text-gray-400 text-xs mt-1">{address || `${lat}, ${lng}`}</div>
        </div>
      </div>
    );
  }

  if (!isLoaded) {
    return (
      <div
        className="flex flex-col items-center justify-center bg-gradient-to-br from-blue-50 to-green-50 border border-gray-200 rounded-lg"
        style={{ height, width: '100%' }}
      >
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mb-3"></div>
        <div className="text-gray-600 text-sm">Cargando mapa...</div>
        <div className="text-gray-400 text-xs mt-1">📍 {address || `${lat}, ${lng}`}</div>
      </div>
    );
  }

  return (
    <MapView
      lat={lat}
      lng={lng}
      address={address}
      height={height}
      zoom={zoom}
    />
  );
};

export default LazyMapView;