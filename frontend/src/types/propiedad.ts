// Tipos TypeScript para el módulo de propiedades

export interface Propiedad {
  id: number;
  codigo: string;
  tipo: string;
  operacion: 'Venta' | 'Alquiler';
  barrio: string;
  comuna: string;
  direccion: string;
  latitud?: number | null;
  longitud?: number | null;
  moneda: 'USD' | 'ARS';
  precio: number;
  expensas?: number | null;
  ambientes: number;
  dormitorios?: number | null;
  banos?: number | null;
  cochera?: boolean;
  metrosCubiertos?: number | null;
  metrosTotales?: number | null;
  antiguedad?: number | null;
  piso?: number | null;
  aptoCredito?: boolean;
  estado: 'Activo' | 'Reservado' | 'Vendido' | 'Pausado';
  destacado: boolean;
  titulo?: string;
  descripcion?: string;
  amenities?: Record<string, boolean>;
  fechaPublicacionUtc: string;
  fechaCreacion: string;
  fechaActualizacion: string;
  medias?: PropiedadMedia[];
}

export interface PropiedadMedia {
  id: number;
  propiedadId: number;
  url: string;
  titulo?: string;
  tipo: 'image' | 'video' | 'tour';
  tipoArchivo: string;
  orden: number;
  esPrincipal: boolean;
}

export interface PropiedadCreateDto {
  codigo: string;
  tipo: string;
  operacion: 'Venta' | 'Alquiler';
  barrio: string;
  comuna: string;
  direccion: string;
  latitud?: number;
  longitud?: number;
  moneda: 'USD' | 'ARS';
  precio: number;
  expensas?: number;
  ambientes: number;
  dormitorios?: number;
  banos?: number;
  cochera?: boolean;
  metrosCubiertos?: number;
  metrosTotales?: number;
  antiguedad?: number;
  piso?: number;
  aptoCredito?: boolean;
  estado: 'Activo' | 'Reservado' | 'Vendido' | 'Pausado';
  destacado: boolean;
  titulo?: string;
  descripcion?: string;
  amenities?: Record<string, boolean>;
}

export interface PropiedadUpdateDto extends PropiedadCreateDto {
  id: number;
}

export interface PropiedadFilters {
  operacion?: string;
  tipo?: string;
  barrio?: string;
  comuna?: string;
  precioMin?: number;
  precioMax?: number;
  ambientes?: number;
  dormitorios?: number;
  cochera?: boolean;
  estado?: string;
  destacado?: boolean;
  page: number;
  pageSize: number;
  orderBy: string;
  orderDesc: boolean;
  searchTerm?: string;
}

export interface PaginationInfo {
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
}

export interface MediaUploadData {
  propiedadId: number;
  file: File;
  titulo?: string;
  orden?: number;
}

export interface MediaUrlData {
  propiedadId: number;
  url: string;
  titulo?: string;
  tipo: 'image' | 'video' | 'tour';
  orden?: number;
}

export interface UrlValidationResult {
  valid: boolean;
  errors?: string[];
  detectedType?: string;
  isSupported?: boolean;
  processedUrl?: string;
  message?: string;
}
