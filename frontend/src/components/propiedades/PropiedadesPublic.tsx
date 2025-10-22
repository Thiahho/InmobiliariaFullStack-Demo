import React, { useState, useEffect } from "react";
import { axiosPublic } from "../../lib/axiosPublic";
import { toast } from "react-hot-toast";
import {
  EyeIcon,
  FunnelIcon,
  MagnifyingGlassIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  PhotoIcon,
  MapPinIcon,
  CurrencyDollarIcon,
  HomeIcon,
  CalendarIcon,
  HeartIcon,
} from "@heroicons/react/24/outline";

// ==== Tipos base ====
type ID = string | number;

type Media = {
  url: string;
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
  moneda?: string;
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

// ==== Props del componente ====
type Props = {
  onView: (p: Propiedad) => void;
};

const PropiedadesPublic: React.FC<Props> = ({ onView }) => {
  // Estado local del componente
  const [propiedades, setPropiedades] = useState<Propiedad[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [showFilters, setShowFilters] = useState<boolean>(false);
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [favorites, setFavorites] = useState<Set<ID>>(new Set());

  // Filtros locales
  const [filtros, setFiltros] = useState<Filtros>({
    operacion: "",
    tipo: "",
    barrio: "",
    estado: "Activo",
    searchTerm: "",
    page: 1,
    pageSize: 20,
  });

  // Paginación local
  const [paginacion, setPaginacion] = useState<Paginacion>({
    currentPage: 1,
    pageSize: 20,
    totalPages: 0,
    totalCount: 0,
  });

  // Función para cargar propiedades desde el backend
  const fetchPropiedades = async (filtrosCustom?: Filtros) => {
    setLoading(true);
    setError(null);

    try {
      const filtrosFinales = filtrosCustom || { ...filtros, estado: "Activo" };

      // Verificar si hay filtros activos
      const hasFilters = Object.entries(filtrosFinales).some(([key, value]) => {
        if (key === "page" || key === "pageSize" || key === "estado")
          return false;
        if (key === "searchTerm")
          return value !== "" && value !== null && value !== undefined;
        return value !== "" && value !== null && value !== undefined;
      });

      let response;

      if (hasFilters) {
        // Usar búsqueda avanzada
        const searchData = {
          page: filtrosFinales.page || 1,
          pageSize: filtrosFinales.pageSize || 20,
          operacion: filtrosFinales.operacion || null,
          tipo: filtrosFinales.tipo || null,
          barrio: filtrosFinales.barrio || null,
          estado: "Activo", // Siempre filtrar solo activas para público
          searchTerm: filtrosFinales.searchTerm || null,
        };

        console.log("🚀 Enviando búsqueda pública:", searchData);
        response = await axiosPublic.post(
          "/propiedades/buscar-avanzada",
          searchData
        );
      } else {
        // Usar endpoint simple
        const params = new URLSearchParams();
        params.append("pagina", String(filtrosFinales.page || 1));
        params.append("tamanoPagina", String(filtrosFinales.pageSize || 20));
        params.append("estado", "Activo");

        response = await axiosPublic.get(`/propiedades/paginadas?${params}`);
      }

      const payload = response.data || {};
      const data = payload.data ?? payload.Data ?? [];
      const totalCount = payload.totalCount ?? payload.TotalCount ?? 0;
      const pagina = payload.pagina ?? payload.Pagina ?? 1;
      const tamanoPagina = payload.tamanoPagina ?? payload.TamanoPagina ?? 20;
      const totalPaginas = payload.totalPaginas ?? payload.TotalPaginas ?? 0;

      setPropiedades(Array.isArray(data) ? data : []);
      setPaginacion({
        totalCount,
        totalPages: totalPaginas,
        currentPage: pagina,
        pageSize: tamanoPagina,
      });

      console.log("✅ Propiedades cargadas:", data.length);
    } catch (error) {
      console.error("❌ Error al cargar propiedades:", error);
      const errorMessage =
        error instanceof Error ? error.message : "Error al cargar propiedades";
      setError(errorMessage);
      setPropiedades([]);
      toast.error("Error al cargar propiedades");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPropiedades();
  }, []);

  const handleFilterChange = (key: keyof Filtros, value: unknown) => {
    const newFilters = { ...filtros, [key]: value, page: 1 };
    setFiltros(newFilters);
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

  const handleResetFilters = () => {
    const defaultFilters = {
      operacion: "",
      tipo: "",
      barrio: "",
      estado: "Activo",
      searchTerm: "",
      page: 1,
      pageSize: 20,
    };
    setFiltros(defaultFilters);
    setSearchTerm("");
    fetchPropiedades(defaultFilters);
  };

  const toggleFavorite = (propiedadId: ID) => {
    const newFavorites = new Set(favorites);
    if (newFavorites.has(propiedadId)) {
      newFavorites.delete(propiedadId);
      toast.success("Removido de favoritos");
    } else {
      newFavorites.add(propiedadId);
      toast.success("Agregado a favoritos");
    }
    setFavorites(newFavorites);
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

  const convertGoogleDriveUrl = (url: string) => {
    if (url.includes("drive.google.com")) {
      let fileId: string | null = null;
      const fileIdMatch = url.match(/\/d\/([a-zA-Z0-9_-]+)/);
      if (fileIdMatch) fileId = fileIdMatch[1];
      const openIdMatch = url.match(/[?&]id=([a-zA-Z0-9_-]+)/);
      if (openIdMatch && !fileId) fileId = openIdMatch[1];
      if (fileId)
        return `https://drive.google.com/thumbnail?id=${fileId}&sz=w400`;
    }
    return url;
  };

  const getImageUrl = (medias?: Media[] | null) => {
    if (!medias || medias.length === 0) return null;
    const principalMedia = medias.find((m) => m.esPrincipal) ?? medias[0];
    if (principalMedia.url.startsWith("http")) {
      return convertGoogleDriveUrl(principalMedia.url);
    }
    const base =
      (import.meta as any)?.env?.VITE_API_BASE_URL ||
      process.env.NEXT_PUBLIC_API_BASE_URL ||
      "http://localhost:5174";
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
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 pt-4 border-t border-gray-200">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Operación
                </label>
                <select
                  value={String(filtros.operacion ?? "")}
                  onChange={(e) =>
                    handleFilterChange("operacion", e.target.value)
                  }
                  className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                >
                  <option value="">Todas</option>
                  <option value="Venta">Venta</option>
                  <option value="Alquiler">Alquiler</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Tipo
                </label>
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
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Barrio
                </label>
                <input
                  type="text"
                  value={String(filtros.barrio ?? "")}
                  onChange={(e) => handleFilterChange("barrio", e.target.value)}
                  placeholder="Filtrar por barrio"
                  className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
              </div>

              <div className="md:col-span-2 lg:col-span-3 flex space-x-4">
                <button
                  onClick={handleResetFilters}
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
              const isFavorite = favorites.has(propiedad.id);

              return (
                <div
                  key={String(propiedad.id)}
                  className="border border-gray-200 rounded-lg overflow-hidden hover:shadow-md transition-shadow cursor-pointer"
                  onClick={() => onView(propiedad)}
                >
                  <div className="flex flex-col lg:flex-row">
                    {/* Imagen */}
                    <div className="lg:w-64 h-48 lg:h-auto bg-gray-100 flex-shrink-0 relative">
                      {imgUrl ? (
                        <img
                          src={imgUrl}
                          alt={propiedad.titulo || propiedad.codigo}
                          className="w-full h-full object-cover"
                          onError={(
                            e: React.SyntheticEvent<HTMLImageElement>
                          ) => {
                            const el = e.currentTarget;
                            const originalUrl = imgUrl;
                            if (
                              originalUrl.includes("thumbnail") &&
                              originalUrl.includes("drive.google.com")
                            ) {
                              const fileIdMatch = originalUrl.match(
                                /[?&]id=([a-zA-Z0-9_-]+)/
                              );
                              if (fileIdMatch) {
                                el.src = `https://drive.google.com/uc?export=view&id=${fileIdMatch[1]}`;
                                return;
                              }
                            }
                            el.style.display = "none";
                            el.parentElement &&
                              ((el.parentElement as any).innerHTML = `
                              <div class="w-full h-full flex items-center justify-center bg-gray-200">
                                <div class="text-center">
                                  <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                                  </svg>
                                  <p class="mt-2 text-sm text-gray-500">Error al cargar imagen</p>
                                </div>
                              </div>
                            `);
                          }}
                        />
                      ) : (
                        <div className="w-full h-full flex items-center justify-center bg-gray-200">
                          <div className="text-center">
                            <PhotoIcon className="mx-auto h-12 w-12 text-gray-400" />
                            <p className="mt-2 text-sm text-gray-500">
                              Sin imagen
                            </p>
                          </div>
                        </div>
                      )}

                      {/* Botón de favoritos */}
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          toggleFavorite(propiedad.id);
                        }}
                        className={`absolute top-2 right-2 p-2 rounded-full ${
                          isFavorite
                            ? "bg-red-500 text-white"
                            : "bg-white bg-opacity-80 text-gray-600 hover:bg-red-500 hover:text-white"
                        } transition-colors`}
                        type="button"
                      >
                        <HeartIcon className="h-4 w-4" />
                      </button>

                      {propiedad.destacado && (
                        <div className="absolute top-2 left-2 bg-yellow-500 text-white px-2 py-1 rounded text-xs font-medium">
                          ⭐ Destacado
                        </div>
                      )}
                    </div>

                    {/* Contenido */}
                    <div className="flex-1 p-6">
                      <div className="flex justify-between items-start mb-4">
                        <div className="flex-1">
                          <div className="flex items-center space-x-2 mb-2">
                            <h3 className="text-lg font-semibold text-gray-900">
                              {propiedad.titulo ||
                                `${propiedad.tipo ?? ""} en ${
                                  propiedad.barrio ?? ""
                                }`}
                            </h3>
                          </div>

                          <div className="flex items-center text-sm text-gray-600 mb-2">
                            <MapPinIcon className="h-4 w-4 mr-1" />
                            {propiedad.direccion}, {propiedad.barrio}
                          </div>

                          <div className="flex items-center space-x-4 text-sm text-gray-600 mb-3">
                            <div className="flex items-center">
                              <HomeIcon className="h-4 w-4 mr-1" />
                              {propiedad.ambientes} amb
                            </div>
                            {propiedad.dormitorios ? (
                              <div>🛏️ {propiedad.dormitorios} dorm</div>
                            ) : null}
                            {propiedad.banos ? (
                              <div>🚿 {propiedad.banos} baños</div>
                            ) : null}
                            {propiedad.metrosCubiertos ? (
                              <div>📐 {propiedad.metrosCubiertos} m²</div>
                            ) : null}
                          </div>

                          <div className="flex items-center space-x-4 mb-3">
                            <div className="flex items-center text-lg font-bold text-green-600">
                              <CurrencyDollarIcon className="h-5 w-5 mr-1" />
                              {formatPrice(
                                propiedad.precio,
                                propiedad.moneda || "USD"
                              )}
                            </div>
                            <span className="text-sm text-gray-500 capitalize">
                              {propiedad.operacion}
                            </span>
                            {typeof propiedad.expensas === "number" && (
                              <span className="text-sm text-gray-500">
                                +{" "}
                                {formatPrice(
                                  propiedad.expensas,
                                  propiedad.moneda || "USD"
                                )}{" "}
                                exp.
                              </span>
                            )}
                          </div>

                          <div className="flex items-center text-xs text-gray-500">
                            <CalendarIcon className="h-4 w-4 mr-1" />
                            Publicado:{" "}
                            {formatDate(propiedad.fechaPublicacionUtc)}
                          </div>
                        </div>

                        {/* Acciones */}
                        <div className="flex space-x-2 ml-4">
                          <button
                            onClick={(e) => {
                              e.stopPropagation();
                              onView(propiedad);
                            }}
                            className="p-2 text-blue-400 hover:text-blue-500"
                            title="Ver detalle"
                            type="button"
                          >
                            <EyeIcon className="h-5 w-5" />
                          </button>
                        </div>
                      </div>

                      {propiedad.descripcion ? (
                        <p className="text-sm text-gray-600 mb-3 line-clamp-2">
                          {propiedad.descripcion}
                        </p>
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
                {loading
                  ? "Cargando propiedades..."
                  : "No hay propiedades disponibles con los filtros seleccionados"}
              </p>
              {!loading && (
                <button
                  onClick={handleResetFilters}
                  className="mt-4 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700"
                  type="button"
                >
                  Limpiar Filtros
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
                    {Math.min(
                      paginacion.currentPage * paginacion.pageSize,
                      paginacion.totalCount
                    )}
                  </span>{" "}
                  de{" "}
                  <span className="font-medium">{paginacion.totalCount}</span>{" "}
                  resultados
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

                  {[...Array(Math.min(5, paginacion.totalPages))].map(
                    (_, i) => {
                      const pageNumber = i + 1;
                      const isActive = pageNumber === paginacion.currentPage;
                      return (
                        <button
                          key={pageNumber}
                          onClick={() => handlePageChange(pageNumber)}
                          className={`relative inline-flex items-center px-4 py-2 border text-sm font-medium ${
                            isActive
                              ? "z-10 bg-blue-50 border-blue-500 text-blue-600"
                              : "bg-white border-gray-300 text-gray-500 hover:bg-gray-50"
                          }`}
                          type="button"
                        >
                          {pageNumber}
                        </button>
                      );
                    }
                  )}

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

export default PropiedadesPublic;
