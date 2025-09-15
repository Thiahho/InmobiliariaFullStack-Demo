import { z } from 'zod';

// Schema base para propiedades
export const propiedadSchema = z.object({
  codigo: z.string().min(1, 'El código es requerido').max(20, 'Código muy largo'),
  tipo: z.string().min(1, 'El tipo es requerido'),
  operacion: z.enum(['Venta', 'Alquiler'], {
    errorMap: () => ({ message: 'Operación debe ser Venta o Alquiler' })
  }),
  barrio: z.string().min(1, 'El barrio es requerido').max(100, 'Barrio muy largo'),
  comuna: z.string().min(1, 'La comuna es requerida').max(50, 'Comuna muy larga'),
  direccion: z.string().min(1, 'La dirección es requerida').max(200, 'Dirección muy larga'),
  latitud: z.number().optional().nullable(),
  longitud: z.number().optional().nullable(),
  moneda: z.enum(['USD', 'ARS']).default('USD'),
  precio: z.number().min(0, 'El precio debe ser mayor a 0'),
  expensas: z.number().min(0, 'Las expensas deben ser mayor a 0').optional().nullable(),
  ambientes: z.number().int().min(1, 'Debe tener al menos 1 ambiente').max(20, 'Demasiados ambientes'),
  dormitorios: z.number().int().min(0, 'Los dormitorios no pueden ser negativos').max(20, 'Demasiados dormitorios').optional().nullable(),
  banos: z.number().int().min(0, 'Los baños no pueden ser negativos').max(10, 'Demasiados baños').optional().nullable(),
  cochera: z.boolean().default(false),
  metrosCubiertos: z.number().int().min(0, 'Los metros cubiertos no pueden ser negativos').max(10000, 'Demasiados metros').optional().nullable(),
  metrosTotales: z.number().int().min(0, 'Los metros totales no pueden ser negativos').max(10000, 'Demasiados metros').optional().nullable(),
  antiguedad: z.number().int().min(0, 'La antigüedad no puede ser negativa').max(200, 'Antigüedad muy alta').optional().nullable(),
  piso: z.number().int().min(-10, 'Piso muy bajo').max(100, 'Piso muy alto').optional().nullable(),
  aptoCredito: z.boolean().default(false),
  estado: z.enum(['Activo', 'Reservado', 'Vendido', 'Pausado']).default('Activo'),
  destacado: z.boolean().default(false),
  titulo: z.string().max(200, 'Título muy largo').optional(),
  descripcion: z.string().max(2000, 'Descripción muy larga').optional(),
  amenities: z.record(z.boolean()).optional()
});

// Schema para crear propiedad
export const createPropiedadSchema = propiedadSchema;

// Schema para actualizar propiedad
export const updatePropiedadSchema = propiedadSchema.extend({
  id: z.number().int().positive('ID inválido')
});

// Schema para filtros de búsqueda
export const propiedadFiltersSchema = z.object({
  operacion: z.string().optional(),
  tipo: z.string().optional(),
  barrio: z.string().optional(),
  comuna: z.string().optional(),
  precioMin: z.number().min(0).optional(),
  precioMax: z.number().min(0).optional(),
  ambientes: z.number().int().min(1).optional(),
  dormitorios: z.number().int().min(0).optional(),
  cochera: z.boolean().optional(),
  estado: z.string().optional(),
  destacado: z.boolean().optional(),
  page: z.number().int().min(1).default(1),
  pageSize: z.number().int().min(1).max(100).default(20),
  orderBy: z.string().default('FechaPublicacionUtc'),
  orderDesc: z.boolean().default(true)
});

// Schema para media
export const mediaSchema = z.object({
  url: z.string().url('URL inválida'),
  titulo: z.string().max(100, 'Título muy largo').optional(),
  tipo: z.enum(['image', 'video', 'tour'], {
    errorMap: () => ({ message: 'Tipo debe ser image, video o tour' })
  }).default('image'),
  orden: z.number().int().min(0).max(999).default(0)
});

// Schema para validación de archivo
export const fileValidationSchema = z.object({
  file: z.instanceof(File, { message: 'Debe ser un archivo válido' }),
  maxSize: z.number().default(50 * 1024 * 1024), // 50MB por defecto
  allowedTypes: z.array(z.string()).default([
    'image/jpeg',
    'image/jpg', 
    'image/png',
    'image/webp',
    'image/gif',
    'video/mp4',
    'video/avi',
    'video/mov',
    'video/wmv'
  ])
});

// Función de validación personalizada para archivos
export const validateFile = (file, options = {}) => {
  const { maxSize = 50 * 1024 * 1024, allowedTypes = [
    'image/jpeg',
    'image/jpg', 
    'image/png',
    'image/webp',
    'image/gif',
    'video/mp4',
    'video/avi',
    'video/mov',
    'video/wmv'
  ] } = options;

  const errors = [];

  if (!file) {
    errors.push('Archivo requerido');
    return { valid: false, errors };
  }

  if (file.size > maxSize) {
    errors.push(`El archivo es demasiado grande. Máximo ${Math.round(maxSize / 1024 / 1024)}MB`);
  }

  if (!allowedTypes.includes(file.type)) {
    errors.push(`Tipo de archivo no permitido. Tipos válidos: ${allowedTypes.join(', ')}`);
  }

  return {
    valid: errors.length === 0,
    errors
  };
};

// Función para validar múltiples archivos
export const validateFiles = (files, options = {}) => {
  if (!Array.isArray(files) && files.length > 0) {
    return { valid: false, errors: ['Debe proporcionar un array de archivos'] };
  }

  const results = files.map((file, index) => ({
    index,
    file,
    ...validateFile(file, options)
  }));

  const validFiles = results.filter(r => r.valid);
  const invalidFiles = results.filter(r => !r.valid);

  return {
    valid: invalidFiles.length === 0,
    validFiles: validFiles.map(r => r.file),
    invalidFiles,
    errors: invalidFiles.flatMap(r => r.errors.map(error => `Archivo ${r.index + 1}: ${error}`))
  };
};

// Función para validar URLs externas comunes
export const validateExternalUrl = (url) => {
  const errors = [];

  if (!url) {
    errors.push('URL requerida');
    return { valid: false, errors };
  }

  try {
    const urlObj = new URL(url);
    
    // Validar protocolos permitidos
    if (!['http:', 'https:'].includes(urlObj.protocol)) {
      errors.push('Solo se permiten URLs HTTP y HTTPS');
    }

    // Detectar tipo automáticamente
    let detectedType = 'image';
    const urlLower = url.toLowerCase();
    
    if (urlLower.includes('youtube.com') || urlLower.includes('youtu.be') || urlLower.includes('vimeo.com')) {
      detectedType = 'video';
    } else if (urlLower.includes('matterport.com') || urlLower.includes('360') || urlLower.includes('tour')) {
      detectedType = 'tour';
    }

    // Verificar dominios soportados
    const supportedDomains = [
      'drive.google.com',
      'docs.google.com',
      'youtube.com',
      'youtu.be',
      'vimeo.com',
      'player.vimeo.com',
      'matterport.com',
      'imgur.com',
      'dropbox.com'
    ];

    const isSupported = supportedDomains.some(domain => urlLower.includes(domain)) || 
                       urlLower.match(/\.(jpg|jpeg|png|gif|webp|mp4|avi|mov)$/i);

    if (!isSupported) {
      // Warning, no error - permite URLs personalizadas
      console.warn('URL no está en la lista de dominios soportados oficialmente');
    }

    return {
      valid: errors.length === 0,
      errors,
      detectedType,
      isSupported,
      processedUrl: processUrlForEmbed(url)
    };

  } catch (error) {
    errors.push('URL no válida');
    return { valid: false, errors };
  }
};

// Función para procesar URLs para embed
const processUrlForEmbed = (url) => {
  // Google Drive
  if (url.includes('drive.google.com/file/d/')) {
    const fileIdMatch = url.match(/\/file\/d\/([a-zA-Z0-9-_]+)/);
    if (fileIdMatch) {
      return `https://drive.google.com/uc?export=view&id=${fileIdMatch[1]}`;
    }
  }

  // YouTube
  if (url.includes('youtube.com/watch?v=') || url.includes('youtu.be/')) {
    const videoIdMatch = url.match(/(?:youtube\.com\/watch\?v=|youtu\.be\/)([a-zA-Z0-9-_]+)/);
    if (videoIdMatch) {
      return `https://www.youtube.com/embed/${videoIdMatch[1]}`;
    }
  }

  return url;
};

// Utilidades de formato
export const formatPrice = (price, currency = 'USD') => {
  return new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency: currency,
    minimumFractionDigits: 0,
    maximumFractionDigits: 0
  }).format(price);
};

export const formatDate = (date) => {
  return new Date(date).toLocaleDateString('es-AR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  });
};

export const getEstadoColor = (estado) => {
  const colors = {
    'Activo': 'bg-green-100 text-green-800',
    'Reservado': 'bg-yellow-100 text-yellow-800',
    'Vendido': 'bg-blue-100 text-blue-800',
    'Pausado': 'bg-gray-100 text-gray-800'
  };
  return colors[estado] || 'bg-gray-100 text-gray-800';
};

export const getTipoIcon = (tipo) => {
  const icons = {
    'Departamento': '🏢',
    'Casa': '🏠',
    'PH': '🏘️',
    'Local': '🏪',
    'Oficina': '🏢',
    'Galpon': '🏭',
    'Terreno': '🌱',
    'Quinta': '🌳',
    'Chacra': '🌾',
    'Campo': '🌿',
    'Cochera': '🚗'
  };
  return icons[tipo] || '🏠';
};


