import React, { useState, useEffect } from "react";
import { usePropiedadesStore } from "../../store/propiedadesStore";
import { useAuthStore } from "../../store/authStore";
import { toast } from "react-hot-toast";
import {
  PencilIcon,
  TrashIcon,
  EyeIcon,
  PlusIcon,
  FunnelIcon,
  MagnifyingGlassIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  PhotoIcon,
  MapPinIcon,
  CurrencyDollarIcon,
  HomeIcon,
  CalendarIcon,
} from "@heroicons/react/24/outline";

// ==== Tipos base ====
type ID = string | number;

type Media = {
  id?: number;
  url: string;
  tipo?: string;
  tipoArchivo?: string;
  esPrincipal?: boolean;
};

export type Propiedad = {
  id: ID;
  codigo: string;
  titulo?: string;
  tipo?: string;
  barrio?: string;
  direccion?: string;
  ambientes?: number;
  dormitorios?: number;
  banos?: number;
  metrosCubiertos?: number;
  precio: number;
  moneda?: string; // e.g. "USD", "ARS"
  operacion?: "Venta" | "Alquiler" | string;
  expensas?: number;
  estado?: "Activo" | "Reservado" | "Vendido" | "Pausado" | string;
  destacado?: boolean;
  descripcion?: string;
  fechaPublicacionUtc?: string | Date;
  cochera?: boolean;
  aptoCredito?: boolean;
  medias?: Media[];
};

type Filtros = {
  operacion?: string;
  tipo?: string;
  estado?: string;
  barrio?: string;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
  [k: string]: unknown;
};

type Paginacion = {
  currentPage: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
};

type StoreShape = {
  propiedades: Propiedad[];
  filtros: Filtros;
  paginacion: Paginacion;
  loading: boolean;
  error?: string | null;
  fetchPropiedades: (f?: Filtros) => Promise<void>;
  deletePropiedad: (id: ID) => Promise<void>;
  setFiltros: (f: Filtros) => void;
  resetFiltros: () => void;
};

type AuthShape = {
  hasPermission: (perm: string) => boolean;
};

// ==== Props del componente ====
type Props = {
  onEdit: (p: Propiedad) => void;
  onView: (p: Propiedad) => void;
  onCreate: () => void;
  onDelete: (p: Propiedad) => void;
};

const PropiedadesList: React.FC<Props> = ({ onEdit, onView, onCreate, onDelete }) => {
  const {
    propiedades,
    filtros,
    paginacion,
    loading,
    error,
    fetchPropiedades,
    deletePropiedad,
    setFiltros,
    resetFiltros,
  } = usePropiedadesStore() as unknown as StoreShape;

  const { hasPermission } = useAuthStore() as unknown as AuthShape;

  const [showFilters, setShowFilters] = useState<boolean>(false);
  const [searchTerm, setSearchTerm] = useState<string>("");

  useEffect(() => {
    fetchPropiedades();
  }, [fetchPropiedades]);

  const handleFilterChange = (key: keyof Filtros, value: unknown) => {
    setFiltros({ ...filtros, [key]: value, page: 1 });
  };

  const handleSearch = () => {
    const newFiltros: Filtros = { ...filtros, searchTerm, page: 1 };
    setFiltros(newFiltros);
    fetchPropiedades(newFiltros);
  };

  const handlePageChange = (newPage: number) => {
    const newFiltros: Filtros = { ...filtros, page: newPage };
    setFiltros(newFiltros);
    fetchPropiedades(newFiltros);
  };

  const handleLocalDelete = async (propiedad: Propiedad) => {
    if (window.confirm(`¿Está seguro de eliminar la propiedad "${propiedad.codigo}"?`)) {
      try {
        await deletePropiedad(propiedad.id);
        toast.success("Propiedad eliminada exitosamente");
      } catch (err) {
        // eslint-disable-next-line no-console
        console.error("Error al eliminar:", err);
      }
    }
  };

  const formatPrice = (price: number, currency: string = "USD") => {
    const formatter = new Intl.NumberFormat("es-AR", {
      style: "currency",
      currency,
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    });
    return formatter.format(price);
  };

  const formatDate = (date: string | Date | undefined) => {
    if (!date) return "";
    return new Date(date).toLocaleDateString("es-AR", {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  };

  const getEstadoColor = (estado?: string) => {
    const colors: Record<string, string> = {
      Activo: "bg-green-100 text-green-800",
      Reservado: "bg-yellow-100 text-yellow-800",
      Vendido: "bg-blue-100 text-blue-800",
      Pausado: "bg-gray-100 text-gray-800",
    };
    return colors[estado ?? ""] || "bg-gray-100 text-gray-800";
  };

  const convertGoogleDriveUrl = (url: string) => {
    if (url.includes("drive.google.com")) {
      let fileId: string | null = null;

      const fileIdMatch = url.match(/\/d\/([a-zA-Z0-9_-]+)/);
      if (fileIdMatch) fileId = fileIdMatch[1];

      const openIdMatch = url.match(/[?&]id=([a-zA-Z0-9_-]+)/);
      if (openIdMatch && !fileId) fileId = openIdMatch[1];

      if (fileId) return `https://drive.google.com/thumbnail?id=${fileId}&sz=w400`;
    }
    return url;
  };

  const getImageUrl = (medias?: Media[] | null) => {
    if (!medias || medias.length === 0) return null;
    const principalMedia = medias.find((m) => m.esPrincipal) ?? medias[0];

    // Si la URL es externa (YouTube, Google Drive, etc.), usar directamente
    if (principalMedia.url.startsWith("http://") || principalMedia.url.startsWith("https://")) {
      return convertGoogleDriveUrl(principalMedia.url);
    }

    const base = (import.meta as any)?.env?.VITE_API_BASE_URL || process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5174";

    // Si la media tiene ID y es una imagen almacenada en la BD, usar el endpoint /api/media/{id}/image
    if (principalMedia.id && (principalMedia.tipo === "image" || principalMedia.tipoArchivo?.match(/^(jpg|jpeg|png|gif|webp|bmp)$/i))) {
      return `${base}/api/media/${principalMedia.id}/image`;
    }

    // Fallback a la URL relativa (por compatibilidad)
    return `${base}${principalMedia.url}`;
  };

  if (loading && (!propiedades || propiedades.length === 0)) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Búsqueda y filtros */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="space-y-4">
          <div className="flex space-x-4">
            <div className="flex-1 relative">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <MagnifyingGlassIcon className="h-5 w-5 text-gray-400" />
              </div>
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                onKeyDown={(e) => e.key === "Enter" && handleSearch()}
                placeholder="Buscar por código, dirección, barrio..."
                className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:placeholder-gray-400 focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
            <button
              onClick={handleSearch}
              className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              type="button"
            >
              Buscar
            </button>
            <button
              onClick={() => setShowFilters((v) => !v)}
              className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              type="button"
            >
              <FunnelIcon className="h-4 w-4 mr-2" />
              Filtros
            </button>
          </div>

          {showFilters && (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 pt-4 border-t border-gray-200">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Operación</label>
                <select
                  value={String(filtros.operacion ?? "")}
                  onChange={(e) => handleFilterChange("operacion", e.target.value)}
                  className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                >
                  <option value="">Todas</option>
                  <option value="Venta">Venta</option>
                  <option value="Alquiler">Alquiler</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Tipo</label>
                <select
                  value={String(filtros.tipo ?? "")}
                  onChange={(e) => handleFilterChange("tipo", e.target.value)}
                  className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                >
                  <option value="">Todos</option>
                  <option value="Departamento">Departamento</option>
                  <option value="Casa">Casa</option>
                  <option value="PH">PH</option>
                  <option value="Local">Local</option>
                  <option value="Oficina">Oficina</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Estado</label>
                <select
                  value={String(filtros.estado ?? "")}
                  onChange={(e) => handleFilterChange("estado", e.target.value)}
                  className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                >
                  <option value="">Todos</option>
                  <option value="Activo">Activo</option>
                  <option value="Reservado">Reservado</option>
                  <option value="Vendido">Vendido</option>
                  <option value="Pausado">Pausado</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Barrio</label>
                <input
                  type="text"
                  value={String(filtros.barrio ?? "")}
                  onChange={(e) => handleFilterChange("barrio", e.target.value)}
                  placeholder="Filtrar por barrio"
                  className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
              </div>

              <div className="md:col-span-2 lg:col-span-4 flex space-x-4">
                <button
                  onClick={() => {
                    resetFiltros();
                    setSearchTerm("");
                    fetchPropiedades();
                  }}
                  className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                  type="button"
                >
                  Limpiar Filtros
                </button>
                <button
                  onClick={() => fetchPropiedades(filtros)}
                  className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700"
                  type="button"
                >
                  Aplicar Filtros
                </button>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Lista */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-md p-4">
          <p className="text-red-800">{error}</p>
        </div>
      )}

      <div className="bg-white shadow rounded-lg overflow-hidden">
        <div className="grid gap-6 p-6">
          {propiedades && propiedades.length > 0 ? (
            propiedades.map((propiedad) => {
              const imgUrl = getImageUrl(propiedad.medias);
              return (
                <div
                  key={String(propiedad.id)}
                  className="border border-gray-200 rounded-lg overflow-hidden hover:shadow-md transition-shadow"
                >
                  <div className="flex flex-col lg:flex-row">
                    {/* Imagen */}
                    <div className="lg:w-64 h-48 lg:h-auto bg-gray-100 flex-shrink-0">
                      {imgUrl ? (
                        <img
                          src={imgUrl}
                          alt={propiedad.titulo || propiedad.codigo}
                          className="w-full h-full object-cover"
                          onError={(e: React.SyntheticEvent<HTMLImageElement>) => {
                            const el = e.currentTarget;
                            const originalUrl = imgUrl;
                            // fallback a uc?export=view cuando es thumbnail de Drive
                            if (originalUrl.includes("thumbnail") && originalUrl.includes("drive.google.com")) {
                              const fileIdMatch = originalUrl.match(/[?&]id=([a-zA-Z0-9_-]+)/);
                              if (fileIdMatch) {
                                el.src = `https://drive.google.com/uc?export=view&id=${fileIdMatch[1]}`;
                                return;
                              }
                            }
                            // placeholder
                            el.style.display = "none";
                            el.parentElement && ((el.parentElement as any).innerHTML = `
                              <div class="w-full h-full flex items-center justify-center bg-gray-200">
                                <div class="text-center">
                                  <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                                  </svg>
                                  <p class="mt-2 text-sm text-gray-500">Error al cargar imagen</p>
                                  <p class="text-xs text-gray-400">Google Drive bloqueado</p>
                                </div>
                              </div>
                            `);
                          }}
                        />
                      ) : (
                        <div className="w-full h-full flex items-center justify-center bg-gray-200">
                          <div className="text-center">
                            <PhotoIcon className="mx-auto h-12 w-12 text-gray-400" />
                            <p className="mt-2 text-sm text-gray-500">Sin imagen</p>
                          </div>
                        </div>
                      )}
                    </div>

                    {/* Contenido */}
                    <div className="flex-1 p-6">
                      <div className="flex justify-between items-start mb-4">
                        <div className="flex-1">
                          <div className="flex items-center space-x-2 mb-2">
                            <h3 className="text-lg font-semibold text-gray-900">
                              {propiedad.titulo || `${propiedad.tipo ?? ""} en ${propiedad.barrio ?? ""}`}
                            </h3>
                            <span
                              className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getEstadoColor(
                                propiedad.estado
                              )}`}
                            >
                              {propiedad.estado}
                            </span>
                            {propiedad.destacado && (
                              <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
                                ⭐ Destacado
                              </span>
                            )}
                          </div>

                          <p className="text-sm text-gray-600 mb-1">Código: {propiedad.codigo}</p>

                          <div className="flex items-center text-sm text-gray-600 mb-2">
                            <MapPinIcon className="h-4 w-4 mr-1" />
                            {propiedad.direccion}, {propiedad.barrio}
                          </div>

                          <div className="flex items-center space-x-4 text-sm text-gray-600 mb-3">
                            <div className="flex items-center">
                              <HomeIcon className="h-4 w-4 mr-1" />
                              {propiedad.ambientes} amb
                            </div>
                            {propiedad.dormitorios ? <div>🛏️ {propiedad.dormitorios} dorm</div> : null}
                            {propiedad.banos ? <div>🚿 {propiedad.banos} baños</div> : null}
                            {propiedad.metrosCubiertos ? <div>📐 {propiedad.metrosCubiertos} m²</div> : null}
                          </div>

                          <div className="flex items-center space-x-4 mb-3">
                            <div className="flex items-center text-lg font-bold text-green-600">
                              <CurrencyDollarIcon className="h-5 w-5 mr-1" />
                              {formatPrice(propiedad.precio, propiedad.moneda || "USD")}
                            </div>
                            <span className="text-sm text-gray-500 capitalize">{propiedad.operacion}</span>
                            {typeof propiedad.expensas === "number" && (
                              <span className="text-sm text-gray-500">
                                + {formatPrice(propiedad.expensas, propiedad.moneda || "USD")} exp.
                              </span>
                            )}
                          </div>

                          <div className="flex items-center text-xs text-gray-500">
                            <CalendarIcon className="h-4 w-4 mr-1" />
                            Publicado: {formatDate(propiedad.fechaPublicacionUtc)}
                          </div>
                        </div>

                        {/* Acciones */}
                        <div className="flex space-x-2 ml-4">
                          <button
                            onClick={() => onView(propiedad)}
                            className="p-2 text-gray-400 hover:text-gray-500"
                            title="Ver detalle"
                            type="button"
                          >
                            <EyeIcon className="h-5 w-5" />
                          </button>

                          {hasPermission("manage_propiedades") && (
                            <>
                              <button
                                onClick={() => onEdit(propiedad)}
                                className="p-2 text-blue-400 hover:text-blue-500"
                                title="Editar"
                                type="button"
                              >
                                <PencilIcon className="h-5 w-5" />
                              </button>

                              <button
                                onClick={() => {
                                  onDelete(propiedad);
                                  // Opcional: si quieres que también elimine desde la lista local
                                  // handleLocalDelete(propiedad);
                                }}
                                className="p-2 text-red-400 hover:text-red-500"
                                title="Eliminar"
                                type="button"
                              >
                                <TrashIcon className="h-5 w-5" />
                              </button>
                            </>
                          )}
                        </div>
                      </div>

                      {propiedad.descripcion ? (
                        <p className="text-sm text-gray-600 mb-3 line-clamp-2">{propiedad.descripcion}</p>
                      ) : null}

                      <div className="flex flex-wrap gap-2">
                        {propiedad.cochera && (
                          <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                            🚗 Cochera
                          </span>
                        )}
                        {propiedad.aptoCredito && (
                          <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                            💳 Apto Crédito
                          </span>
                        )}
                        {propiedad.medias && propiedad.medias.length > 0 && (
                          <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
                            <PhotoIcon className="h-3 w-3 mr-1" />
                            {propiedad.medias.length} fotos
                          </span>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              );
            })
          ) : (
            <div className="text-center py-8">
              <HomeIcon className="h-12 w-12 mx-auto text-gray-400" />
              <p className="mt-2 text-sm text-gray-500">
                {loading ? "Cargando propiedades..." : "No hay propiedades disponibles"}
              </p>
              {!loading && hasPermission("manage_propiedades") && (
                <button
                  onClick={onCreate}
                  className="mt-4 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700"
                  type="button"
                >
                  <PlusIcon className="h-4 w-4 mr-2" />
                  Crear Primera Propiedad
                </button>
              )}
            </div>
          )}
        </div>

        {/* Paginación */}
        {paginacion?.totalPages > 1 && (
          <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
            <div className="flex-1 flex justify-between sm:hidden">
              <button
                onClick={() => handlePageChange(paginacion.currentPage - 1)}
                disabled={paginacion.currentPage <= 1}
                className="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
                type="button"
              >
                Anterior
              </button>
              <button
                onClick={() => handlePageChange(paginacion.currentPage + 1)}
                disabled={paginacion.currentPage >= paginacion.totalPages}
                className="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
                type="button"
              >
                Siguiente
              </button>
            </div>

            <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
              <div>
                <p className="text-sm text-gray-700">
                  Mostrando{" "}
                  <span className="font-medium">
                    {(paginacion.currentPage - 1) * paginacion.pageSize + 1}
                  </span>{" "}
                  -{" "}
                  <span className="font-medium">
                    {Math.min(paginacion.currentPage * paginacion.pageSize, paginacion.totalCount)}
                  </span>{" "}
                  de <span className="font-medium">{paginacion.totalCount}</span> resultados
                </p>
              </div>

              <div>
                <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px">
                  <button
                    onClick={() => handlePageChange(paginacion.currentPage - 1)}
                    disabled={paginacion.currentPage <= 1}
                    className="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                    type="button"
                  >
                    <ChevronLeftIcon className="h-5 w-5" />
                  </button>

                  {[...Array(Math.min(5, paginacion.totalPages))].map((_, i) => {
                    const pageNumber = i + 1;
                    const isActive = pageNumber === paginacion.currentPage;
                    return (
                      <button
                        key={pageNumber}
                        onClick={() => handlePageChange(pageNumber)}
                        className={`relative inline-flex items-center px-4 py-2 border text-sm font-medium ${
                          isActive ? "z-10 bg-blue-50 border-blue-500 text-blue-600" : "bg-white border-gray-300 text-gray-500 hover:bg-gray-50"
                        }`}
                        type="button"
                      >
                        {pageNumber}
                      </button>
                    );
                  })}

                  <button
                    onClick={() => handlePageChange(paginacion.currentPage + 1)}
                    disabled={paginacion.currentPage >= paginacion.totalPages}
                    className="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                    type="button"
                  >
                    <ChevronRightIcon className="h-5 w-5" />
                  </button>
                </nav>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default PropiedadesList;
