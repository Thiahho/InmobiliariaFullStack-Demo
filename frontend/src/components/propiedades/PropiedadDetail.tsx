import React, { useEffect, useState } from "react";
import { usePropiedadesStore } from "../../store/propiedadesStore";
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
  GlobeAltIcon,
} from "@heroicons/react/24/outline";
import { HeartIcon as HeartSolidIcon } from "@heroicons/react/24/solid";
import { BotonAgendarVisita } from "../visitas";
import MapView from "./MapView";

// ==== Tipos ====
type ID = string | number;

type Media = {
  id: ID;
  url: string;
  orden: number;
  tipo: "image" | "video" | "tour" | string;
  tipoArchivo?: string;
  titulo?: string;
};

type Propiedad = {
  id: ID;
  codigo: string;
  titulo?: string;
  tipo?: string;
  barrio?: string;
  comuna?: string;
  direccion?: string;
  ambientes?: number;
  dormitorios?: number;
  banos?: number;
  metrosCubiertos?: number;
  metrosTotales?: number;
  precio: number;
  moneda?: string; // "USD" | "ARS" | ...
  operacion?: string; // "Venta" | "Alquiler" | ...
  expensas?: number;
  estado?: string;
  descripcion?: string;
  aptoCredito?: boolean;
  cochera?: boolean;
  antiguedad?: number;
  piso?: number;
  amenities?: Record<string, boolean>;
  fechaPublicacionUtc?: string | Date;
  latitud?: number;
  longitud?: number;
};

type StoreShape = {
  propiedadActual?: Propiedad | null;
  mediasPropiedad: Media[];
  loading: boolean;
  fetchPropiedadById: (id: ID) => Promise<void>;
  fetchMediasByPropiedad: (id: ID) => Promise<void>;
};

type Props = {
  propiedadId: ID;
  onClose: () => void;
};

const PropiedadDetail: React.FC<Props> = ({ propiedadId, onClose }) => {
  const {
    propiedadActual,
    mediasPropiedad,
    fetchPropiedadById,
    fetchMediasByPropiedad,
    loading,
  } = usePropiedadesStore() as unknown as StoreShape;

  const [currentMediaIndex, setCurrentMediaIndex] = useState<number>(0);
  const [isFavorite, setIsFavorite] = useState<boolean>(false);
  const [activeTab, setActiveTab] = useState<"descripcion" | "caracteristicas">(
    "descripcion"
  );
  const [mapPreloaded, setMapPreloaded] = useState<boolean>(false);

  useEffect(() => {
    if (propiedadId != null) {
      fetchPropiedadById(propiedadId);
      fetchMediasByPropiedad(propiedadId);
      setCurrentMediaIndex(0);
    }
  }, [propiedadId, fetchPropiedadById, fetchMediasByPropiedad]);

  // Precargar mapa después de que se cargan los datos de la propiedad
  useEffect(() => {
    if (
      propiedadActual &&
      propiedadActual.latitud &&
      propiedadActual.longitud &&
      !mapPreloaded
    ) {
      // Precargar recursos de leaflet en background
      if (typeof window !== "undefined") {
        import("leaflet").then(() => {
          import("react-leaflet").then(() => {
            setMapPreloaded(true);
          });
        });
      }
    }
  }, [propiedadActual, mapPreloaded]);

  if (loading || !propiedadActual) {
    return (
      <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center">
        <div className="bg-white rounded-lg p-8">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600 mx-auto" />
          <p className="text-center mt-4">Cargando propiedad...</p>
        </div>
      </div>
    );
  }

  const formatPrice = (price: number, currency: string = "USD") =>
    new Intl.NumberFormat("es-AR", {
      style: "currency",
      currency,
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(price);

  const formatDate = (date?: string | Date) =>
    date
      ? new Date(date).toLocaleDateString("es-AR", {
          year: "numeric",
          month: "long",
          day: "numeric",
        })
      : "";

  const getMediaUrl = (media: Media) => {
    // Si la URL es externa (YouTube, Google Drive, etc.), usar directamente
    if (media.url.startsWith("http://") || media.url.startsWith("https://")) {
      return media.url;
    }

    const base =
      (import.meta as any)?.env?.VITE_API_BASE_URL ||
      process.env.NEXT_PUBLIC_API_BASE_URL ||
      "http://localhost:5174";

    // Si la media tiene ID y es una imagen almacenada en la BD, usar el endpoint /api/media/{id}/image
    if (media.id && (media.tipo === "image" || media.tipoArchivo?.match(/^(jpg|jpeg|png|gif|webp|bmp)$/i))) {
      return `${base}/api/media/${media.id}/image`;
    }

    // Fallback a la URL relativa (por compatibilidad)
    return `${base}${media.url}`;
  };

  const isImage = (m: Media) =>
    m.tipo === "image" ||
    Boolean(m.tipoArchivo?.match(/^(jpg|jpeg|png|gif|webp|bmp)$/i));
  const isVideo = (m: Media) =>
    m.tipo === "video" ||
    Boolean(m.tipoArchivo?.match(/^(mp4|avi|mov|wmv|m4v|webm)$/i));
  const isExternalVideo = (m: Media) =>
    m.url.includes("youtube.com") ||
    m.url.includes("vimeo.com") ||
    m.url.includes("youtu.be");
  const isTour = (m: Media) => m.tipo === "tour";

  const totalMedias = mediasPropiedad?.length ?? 0;
  const sortedMedias = [...(mediasPropiedad ?? [])].sort(
    (a, b) => a.orden - b.orden
  );
  const currentMedia = sortedMedias[currentMediaIndex];

  const nextMedia = () => {
    setCurrentMediaIndex((prev) => (prev < totalMedias - 1 ? prev + 1 : 0));
  };
  const prevMedia = () => {
    setCurrentMediaIndex((prev) => (prev > 0 ? prev - 1 : totalMedias - 1));
  };

  const handleShare = async () => {
    const title =
      propiedadActual.titulo ||
      `${propiedadActual.tipo ?? ""} en ${propiedadActual.barrio ?? ""}`;
    const text = `${propiedadActual.tipo ?? "Propiedad"} de ${
      propiedadActual.ambientes ?? "-"
    } ambientes en ${propiedadActual.barrio ?? "-"}`;
    const url = typeof window !== "undefined" ? window.location.href : "";

    if (navigator.share) {
      try {
        await navigator.share({ title, text, url });
      } catch {
        /* noop */
      }
    } else if (navigator.clipboard && url) {
      try {
        await navigator.clipboard.writeText(url);
        alert("Link copiado al portapapeles");
      } catch {
        /* noop */
      }
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 z-50 overflow-y-auto">
      <div className="min-h-screen px-4 py-8">
        <div className="max-w-6xl mx-auto bg-white rounded-lg shadow-xl overflow-hidden">
          {/* Header */}
          <div className="flex justify-between items-center p-6 border-b border-gray-200">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">
                {propiedadActual.titulo ||
                  `${propiedadActual.tipo ?? ""} en ${
                    propiedadActual.barrio ?? ""
                  }`}
              </h1>
              <div className="flex items-center mt-2 text-gray-600">
                <MapPinIcon className="h-5 w-5 mr-1" />
                {propiedadActual.direccion}
                {propiedadActual.barrio ? `, ${propiedadActual.barrio}` : ""}
                {propiedadActual.comuna ? `, ${propiedadActual.comuna}` : ""}
              </div>
            </div>

            <div className="flex items-center space-x-2">
              <button
                onClick={handleShare}
                className="p-2 text-gray-400 hover:text-gray-600 rounded-full hover:bg-gray-100"
                title="Compartir"
                type="button"
              >
                <ShareIcon className="h-6 w-6" />
              </button>

              <button
                onClick={() => setIsFavorite((v) => !v)}
                className="p-2 text-gray-400 hover:text-red-500 rounded-full hover:bg-gray-100"
                title="Agregar a favoritos"
                type="button"
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
                type="button"
              >
                <XMarkIcon className="h-6 w-6" />
              </button>
            </div>
          </div>

          <div className="flex flex-col lg:flex-row">
            {/* Galería */}
            <div className="lg:w-2/3">
              {sortedMedias.length > 0 ? (
                <div className="relative">
                  <div className="aspect-video bg-gray-100">
                    {currentMedia && isImage(currentMedia) && (
                      <img
                        src={getMediaUrl(currentMedia)}
                        alt={currentMedia.titulo || "Imagen"}
                        className="w-full h-full object-cover"
                        onError={(
                          e: React.SyntheticEvent<HTMLImageElement>
                        ) => {
                          e.currentTarget.src = "/placeholder-property.jpg";
                        }}
                      />
                    )}

                    {currentMedia &&
                      isVideo(currentMedia) &&
                      !isExternalVideo(currentMedia) && (
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
                          frameBorder={0}
                          allowFullScreen
                          title={currentMedia.titulo || "video"}
                        />
                      </div>
                    )}

                    {currentMedia && isTour(currentMedia) && (
                      <div className="w-full h-full">
                        <iframe
                          src={getMediaUrl(currentMedia)}
                          className="w-full h-full"
                          frameBorder={0}
                          allowFullScreen
                          title={currentMedia.titulo || "tour 360"}
                        />
                      </div>
                    )}
                  </div>

                  {sortedMedias.length > 1 && (
                    <>
                      <button
                        onClick={prevMedia}
                        className="absolute left-4 top-1/2 -translate-y-1/2 bg-black bg-opacity-50 text-white p-2 rounded-full hover:bg-opacity-70 transition-all"
                        type="button"
                      >
                        <ChevronLeftIcon className="h-6 w-6" />
                      </button>

                      <button
                        onClick={nextMedia}
                        className="absolute right-4 top-1/2 -translate-y-1/2 bg-black bg-opacity-50 text-white p-2 rounded-full hover:bg-opacity-70 transition-all"
                        type="button"
                      >
                        <ChevronRightIcon className="h-6 w-6" />
                      </button>

                      <div className="absolute bottom-4 right-4 bg-black bg-opacity-50 text-white px-3 py-1 rounded-full text-sm">
                        {currentMediaIndex + 1} / {sortedMedias.length}
                      </div>
                    </>
                  )}

                  {currentMedia && (
                    <div className="absolute top-4 left-4 bg-black bg-opacity-50 text-white px-3 py-1 rounded-full text-sm flex items-center">
                      {isImage(currentMedia) && (
                        <PhotoIcon className="h-4 w-4 mr-1" />
                      )}
                      {isVideo(currentMedia) && (
                        <PlayIcon className="h-4 w-4 mr-1" />
                      )}
                      {isTour(currentMedia) && (
                        <GlobeAltIcon className="h-4 w-4 mr-1" />
                      )}
                      {String(currentMedia.tipo).charAt(0).toUpperCase() +
                        String(currentMedia.tipo).slice(1)}
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
              {sortedMedias.length > 1 && (
                <div className="p-4 border-t border-gray-2 00">
                  <div className="flex space-x-2 overflow-x-auto">
                    {sortedMedias.map((media, index) => (
                      <button
                        key={String(media.id)}
                        onClick={() => setCurrentMediaIndex(index)}
                        className={`flex-shrink-0 w-20 h-20 rounded-lg overflow-hidden border-2 transition-all ${
                          index === currentMediaIndex
                            ? "border-blue-500"
                            : "border-gray-200 hover:border-gray-300"
                        }`}
                        type="button"
                      >
                        {isImage(media) ? (
                          <img
                            src={getMediaUrl(media)}
                            alt={`Thumbnail ${index + 1}`}
                            className="w-full h-full object-cover"
                          />
                        ) : (
                          <div className="w-full h-full bg-gray-200 flex items-center justify-center">
                            {isVideo(media) && (
                              <PlayIcon className="h-6 w-6 text-gray-500" />
                            )}
                            {isTour(media) && (
                              <GlobeAltIcon className="h-6 w-6 text-gray-500" />
                            )}
                          </div>
                        )}
                      </button>
                    ))}
                  </div>
                </div>
              )}
              {/* Mapa de ubicación debajo de la imagen */}
              {propiedadActual.latitud != null &&
                propiedadActual.longitud != null && (
                  <div className="border-t border-gray-200 p-4">
                    <div className="mb-3">
                      <h3 className="font-medium text-gray-900 mb-1">
                        Ubicación
                      </h3>
                      <p className="text-sm text-gray-600">
                        {propiedadActual.direccion}
                        {propiedadActual.barrio
                          ? `, ${propiedadActual.barrio}`
                          : ""}
                        {propiedadActual.comuna
                          ? `, ${propiedadActual.comuna}`
                          : ""}
                      </p>
                    </div>
                    <div className="rounded-lg overflow-hidden border border-gray-200 mb-3">
                      <MapView
                        lat={propiedadActual.latitud}
                        lng={propiedadActual.longitud}
                        address={`${propiedadActual.direccion}${
                          propiedadActual.barrio
                            ? `, ${propiedadActual.barrio}`
                            : ""
                        }${
                          propiedadActual.comuna
                            ? `, ${propiedadActual.comuna}`
                            : ""
                        }`}
                        height="200px"
                      />
                    </div>
                    <div className="flex flex-wrap gap-2">
                      <a
                        href={`https://www.google.com/maps?q=${propiedadActual.latitud},${propiedadActual.longitud}`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="inline-flex items-center px-2 py-1 text-xs font-medium text-blue-600 bg-blue-50 border border-blue-200 rounded hover:bg-blue-100 transition-colors"
                      >
                        <MapPinIcon className="h-3 w-3 mr-1" />
                        Ver en Google Maps
                      </a>
                      <a
                        href={`https://www.google.com/maps/dir/?api=1&destination=${propiedadActual.latitud},${propiedadActual.longitud}`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="inline-flex items-center px-2 py-1 text-xs font-medium text-green-600 bg-green-50 border border-green-200 rounded hover:bg-green-100 transition-colors"
                      >
                        🚗 Cómo llegar
                      </a>
                    </div>
                  </div>
                )}
            </div>

            {/* Info */}
            <div className="lg:w-1/3 p-6 border-l border-gray-200">
              <div className="mb-6">
                <div className="flex items-center justify-between mb-4">
                  <div className="text-3xl font-bold text-green-600">
                    {formatPrice(
                      propiedadActual.precio,
                      propiedadActual.moneda || "USD"
                    )}
                  </div>
                  <span className="bg-blue-100 text-blue-800 px-3 py-1 rounded-full text-sm font-medium capitalize">
                    {propiedadActual.operacion}
                  </span>
                </div>

                {typeof propiedadActual.expensas === "number" && (
                  <p className="text-gray-600 mb-2">
                    +{" "}
                    {formatPrice(
                      propiedadActual.expensas,
                      propiedadActual.moneda || "USD"
                    )}{" "}
                    expensas
                  </p>
                )}

                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div className="flex items-center">
                    <HomeIcon className="h-4 w-4 mr-2 text-gray-400" />
                    {propiedadActual.ambientes} ambientes
                  </div>

                  {propiedadActual.dormitorios ? (
                    <div>🛏️ {propiedadActual.dormitorios} dormitorios</div>
                  ) : null}
                  {propiedadActual.banos ? (
                    <div>🚿 {propiedadActual.banos} baños</div>
                  ) : null}
                  {propiedadActual.metrosCubiertos ? (
                    <div>📐 {propiedadActual.metrosCubiertos} m² cubiertos</div>
                  ) : null}
                  {propiedadActual.metrosTotales ? (
                    <div>📏 {propiedadActual.metrosTotales} m² totales</div>
                  ) : null}
                  {propiedadActual.cochera ? <div>🚗 Cochera</div> : null}
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
                    { id: "descripcion", label: "Descripción" },
                    { id: "caracteristicas", label: "Características" },
                  ].map((tab) => (
                    <button
                      key={tab.id}
                      onClick={() =>
                        setActiveTab(
                          tab.id as "descripcion" | "caracteristicas"
                        )
                      }
                      className={`py-2 px-1 border-b-2 font-medium text-sm ${
                        activeTab === (tab.id as string)
                          ? "border-blue-500 text-blue-600"
                          : "border-transparent text-gray-500 hover:text-gray-700"
                      }`}
                      type="button"
                    >
                      {tab.label}
                    </button>
                  ))}
                </nav>
              </div>

              <div>
                {activeTab === "descripcion" && (
                  <div>
                    {propiedadActual.descripcion ? (
                      <p className="text-gray-700 leading-relaxed whitespace-pre-line">
                        {propiedadActual.descripcion}
                      </p>
                    ) : (
                      <p className="text-gray-500 italic">
                        Sin descripción disponible
                      </p>
                    )}
                  </div>
                )}

                {activeTab === "caracteristicas" && (
                  <div className="space-y-3">
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <span className="font-medium text-gray-700">Tipo:</span>
                        <p className="text-gray-600">{propiedadActual.tipo}</p>
                      </div>

                      <div>
                        <span className="font-medium text-gray-700">
                          Estado:
                        </span>
                        <p className="text-gray-600">
                          {propiedadActual.estado}
                        </p>
                      </div>

                      {typeof propiedadActual.antiguedad === "number" && (
                        <div>
                          <span className="font-medium text-gray-700">
                            Antigüedad:
                          </span>
                          <p className="text-gray-600">
                            {propiedadActual.antiguedad} años
                          </p>
                        </div>
                      )}

                      {typeof propiedadActual.piso === "number" && (
                        <div>
                          <span className="font-medium text-gray-700">
                            Piso:
                          </span>
                          <p className="text-gray-600">
                            {propiedadActual.piso}°
                          </p>
                        </div>
                      )}
                    </div>

                    {propiedadActual.amenities &&
                      Object.keys(propiedadActual.amenities).length > 0 && (
                        <div>
                          <h4 className="font-medium text-gray-700 mb-2">
                            Amenities:
                          </h4>
                          <div className="flex flex-wrap gap-2">
                            {Object.entries(propiedadActual.amenities).map(
                              ([amenity, value]) =>
                                value && (
                                  <span
                                    key={amenity}
                                    className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
                                  >
                                    {amenity}
                                  </span>
                                )
                            )}
                          </div>
                        </div>
                      )}
                  </div>
                )}
              </div>

              <div className="mt-6 pt-6 border-t border-gray-200">
                <div className="flex items-center text-sm text-gray-500">
                  <CalendarIcon className="h-4 w-4 mr-2" />
                  Publicado el {formatDate(propiedadActual.fechaPublicacionUtc)}
                </div>

                <div className="mt-2 text-sm text-gray-500">
                  Código: {propiedadActual.codigo}
                </div>
              </div>

              <div className="mt-6 space-y-3">
                <button
                  onClick={() => {
                    const phoneNumber = "+541122692061"; // Reemplazar con tu número de WhatsApp
                    const propertyInfo = `Hola! Me interesa la propiedad:
${propiedadActual.titulo || `${propiedadActual.tipo} en ${propiedadActual.barrio}`}
📍 ${propiedadActual.direccion}${propiedadActual.barrio ? `, ${propiedadActual.barrio}` : ''}
💰 ${formatPrice(propiedadActual.precio, propiedadActual.moneda || "USD")}
🏠 ${propiedadActual.ambientes} ambientes
📋 Código: ${propiedadActual.codigo}`;

                    const whatsappUrl = `https://wa.me/${phoneNumber}?text=${encodeURIComponent(propertyInfo)}`;
                    window.open(whatsappUrl, '_blank');
                  }}
                  className="w-full bg-blue-600 text-white py-3 px-4 rounded-lg hover:bg-blue-700 transition-colors font-medium"
                  type="button"
                >
                  Contactar por esta propiedad
                </button>

                <BotonAgendarVisita
                  propiedad={{
                    id: Number(propiedadActual.id),
                    codigo: propiedadActual.codigo,
                    tipo: propiedadActual.tipo || "",
                    direccion: propiedadActual.direccion || "",
                    barrio: propiedadActual.barrio || "",
                    precio: propiedadActual.precio,
                    moneda: propiedadActual.moneda || "USD",
                    ambientes: propiedadActual.ambientes || 0,
                    dormitorios: propiedadActual.dormitorios,
                    banos: propiedadActual.banos,
                    metrosCubiertos: propiedadActual.metrosCubiertos,
                  }}
                  variant="outline"
                  fullWidth
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PropiedadDetail;
