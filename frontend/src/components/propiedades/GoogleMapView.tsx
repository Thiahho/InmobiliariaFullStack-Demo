import React from 'react';

interface GoogleMapViewProps {
  lat: number;
  lng: number;
  address?: string;
  height?: string | number;
  zoom?: number;
}

const GoogleMapView: React.FC<GoogleMapViewProps> = ({
  lat,
  lng,
  address = '',
  height = '300px',
  zoom = 15
}) => {
  const mapSrc = `https://www.google.com/maps/embed/v1/place?key=${process.env.NEXT_PUBLIC_GOOGLE_MAPS_API_KEY}&q=${lat},${lng}&zoom=${zoom}`;

  // Si no hay API key, mostrar enlace a Google Maps
  if (!process.env.NEXT_PUBLIC_GOOGLE_MAPS_API_KEY) {
    return (
      <div
        className="flex items-center justify-center bg-gray-100 border border-gray-200 rounded-lg"
        style={{ height, width: '100%' }}
      >
        <div className="text-center p-4">
          <p className="text-gray-600 mb-3">Mapa no disponible</p>
          <a
            href={`https://www.google.com/maps?q=${lat},${lng}`}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center px-4 py-2 text-sm font-medium text-blue-600 bg-blue-50 border border-blue-200 rounded-md hover:bg-blue-100 transition-colors"
          >
            📍 Ver en Google Maps
          </a>
        </div>
      </div>
    );
  }

  return (
    <div style={{ height, width: '100%' }}>
      <iframe
        width="100%"
        height="100%"
        style={{ border: 0 }}
        loading="lazy"
        allowFullScreen
        referrerPolicy="no-referrer-when-downgrade"
        src={mapSrc}
        title={`Mapa de ${address || `${lat}, ${lng}`}`}
      />
    </div>
  );
};

export default GoogleMapView;