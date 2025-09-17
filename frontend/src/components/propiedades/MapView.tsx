import React, { useEffect, useState } from 'react';
import dynamic from 'next/dynamic';

// Importación dinámica para evitar problemas de SSR con Leaflet
const MapContainer = dynamic(() => import('react-leaflet').then(mod => mod.MapContainer), {
  ssr: false
});
const TileLayer = dynamic(() => import('react-leaflet').then(mod => mod.TileLayer), {
  ssr: false
});
const Marker = dynamic(() => import('react-leaflet').then(mod => mod.Marker), {
  ssr: false
});
const Popup = dynamic(() => import('react-leaflet').then(mod => mod.Popup), {
  ssr: false
});

interface MapViewProps {
  lat: number;
  lng: number;
  address?: string;
  height?: string | number;
  zoom?: number;
}

const MapView: React.FC<MapViewProps> = ({
  lat,
  lng,
  address = '',
  height = '300px',
  zoom = 15
}) => {
  const [isClient, setIsClient] = useState(false);

  useEffect(() => {
    if (typeof window !== 'undefined') {
      setIsClient(true);
      // Fix for default markers in react-leaflet cuando esté disponible
      import('leaflet').then((leaflet) => {
        const L = leaflet.default;
        delete (L.Icon.Default.prototype as any)._getIconUrl;
        L.Icon.Default.mergeOptions({
          iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
          iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
          shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
        });
      });
    }
  }, []);

  if (!isClient) {
    return (
      <div
        className="flex items-center justify-center bg-gray-50 border border-gray-200 rounded-lg"
        style={{ height, width: '100%' }}
      >
        <div className="text-gray-500 text-sm">Cargando mapa...</div>
      </div>
    );
  }

  const position: [number, number] = [lat, lng];

  return (
    <div style={{ height, width: '100%' }} className="relative z-0">
      <MapContainer
        center={position}
        zoom={zoom}
        style={{ height: '100%', width: '100%' }}
        scrollWheelZoom={false}
        zoomControl={true}
        className="rounded-lg z-0"
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          maxZoom={19}
        />
        <Marker position={position}>
          <Popup>
            <div className="text-center">
              <div className="font-medium text-gray-900 mb-1">📍 Ubicación</div>
              <div className="text-sm text-gray-600">
                {address || `${lat}, ${lng}`}
              </div>
            </div>
          </Popup>
        </Marker>
      </MapContainer>
    </div>
  );
};

export default MapView;