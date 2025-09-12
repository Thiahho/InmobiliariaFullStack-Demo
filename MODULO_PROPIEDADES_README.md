# Módulo de Propiedades - Sistema Inmobiliario

## 📋 Descripción General

El Módulo de Propiedades es una solución completa para la gestión de propiedades inmobiliarias que incluye funcionalidades avanzadas para crear, editar, eliminar y visualizar propiedades, así como gestionar multimedia asociada (imágenes locales y URLs externas).

## 🏗️ Arquitectura

### Backend (ASP.NET Core 8)
- **Controllers**: PropiedadesController, MediaController
- **Models**: Propiedad, PropiedadMedia, PropiedadHistorial
- **Services**: PropiedadesService, MediaService, ImageProcessingService
- **DTOs**: PropiedadCreateDto, PropiedadUpdateDto, PropiedadResponseDto, MediaDto
- **Base de datos**: Entity Framework Core con SQL Server

### Frontend (React + Next.js)
- **Store**: Zustand para manejo de estado global
- **Components**: Componentes modulares y reutilizables
- **Schemas**: Validación con Zod
- **UI**: TailwindCSS + Heroicons
- **HTTP Client**: Axios con interceptores

## 🔧 Funcionalidades Principales

### 1. Gestión de Propiedades
- ✅ **Crear propiedades** con validación completa
- ✅ **Editar propiedades** existentes
- ✅ **Eliminar propiedades** (solo Admin)
- ✅ **Listado paginado** con filtros avanzados
- ✅ **Vista detalle** con galería multimedia
- ✅ **Búsqueda avanzada** por múltiples criterios

### 2. Gestión de Multimedia
- ✅ **Subida de imágenes locales** con optimización automática
- ✅ **Subida de videos** hasta 50MB
- ✅ **URLs externas** (Google Drive, YouTube, Vimeo, etc.)
- ✅ **Reordenamiento** de medios por drag & drop
- ✅ **Conversión automática** a WebP para imágenes
- ✅ **Vista previa** y validación de URLs

### 3. Sistema de Permisos
- ✅ **Admin**: Acceso completo
- ✅ **Agente**: Gestión de propiedades y leads
- ✅ **Cargador**: Subida de multimedia y gestión básica

## 📁 Estructura de Archivos

```
Backend/
├── Controllers/
│   ├── PropiedadesController.cs      # API endpoints para propiedades
│   └── MediaController.cs            # API endpoints para multimedia
├── Data/
│   ├── Modelos/
│   │   ├── Propiedad.cs             # Modelo principal
│   │   ├── PropiedadMedia.cs        # Modelo de multimedia
│   │   └── PropiedadHistorial.cs    # Historial de cambios
│   └── Dtos/
│       ├── PropiedadDto.cs          # DTOs para propiedades
│       └── MediaDto.cs              # DTOs para multimedia
└── Services/
    ├── PropiedadesService.cs        # Lógica de negocio
    ├── MediaService.cs              # Gestión de archivos
    └── ImageProcessingService.cs    # Procesamiento de imágenes

Frontend/
├── store/
│   └── propiedadesStore.js          # Estado global con Zustand
├── components/propiedades/
│   ├── PropiedadesModule.jsx        # Componente principal
│   ├── PropiedadForm.jsx            # Formulario crear/editar
│   ├── PropiedadesList.jsx          # Lista con paginación
│   ├── PropiedadDetail.jsx          # Vista detalle
│   ├── MediaUploader.jsx            # Subida de archivos
│   └── ExternalUrlManager.jsx       # Gestión URLs externas
├── schemas/
│   └── propiedadSchemas.js          # Validaciones con Zod
└── components/admin/
    └── PropiedadesAdminPanel.jsx    # Panel de administración
```

## 🚀 Instalación y Configuración

### Requisitos Previos
- ASP.NET Core 8
- SQL Server
- Node.js 18+
- React 18+

### Backend
1. **Configurar base de datos** en `appsettings.json`
2. **Ejecutar migraciones**:
   ```bash
   dotnet ef database update
   ```
3. **Configurar servicios** en `Program.cs`:
   ```csharp
   builder.Services.AddScoped<IPropiedadesService, PropiedadesService>();
   builder.Services.AddScoped<IMediaService, MediaService>();
   ```

### Frontend
1. **Instalar dependencias**:
   ```bash
   npm install @heroicons/react react-hot-toast react-hook-form react-dropzone zod zustand
   ```
2. **Configurar variables de entorno**:
   ```env
   NEXT_PUBLIC_API_URL=http://localhost:5174/api
   NEXT_PUBLIC_API_BASE_URL=http://localhost:5174
   ```

## 📝 Uso del Módulo

### 1. Integración en la Aplicación
```jsx
import PropiedadesAdminPanel from './components/admin/PropiedadesAdminPanel';

function AdminDashboard() {
  return (
    <div>
      <PropiedadesAdminPanel />
    </div>
  );
}
```

### 2. Uso del Store
```jsx
import { usePropiedadesStore } from './store/propiedadesStore';

function MiComponente() {
  const { 
    propiedades, 
    loading, 
    fetchPropiedades, 
    createPropiedad 
  } = usePropiedadesStore();

  // Cargar propiedades
  useEffect(() => {
    fetchPropiedades();
  }, []);

  // Crear nueva propiedad
  const handleCreate = async (data) => {
    await createPropiedad(data);
  };
}
```

### 3. Validación de Datos
```jsx
import { propiedadSchema } from './schemas/propiedadSchemas';

const validatePropiedad = (data) => {
  try {
    propiedadSchema.parse(data);
    return { valid: true };
  } catch (error) {
    return { valid: false, errors: error.errors };
  }
};
```

## 🎨 Componentes Principales

### PropiedadesModule
Componente principal que maneja la navegación entre vistas:
- Lista de propiedades
- Formulario de creación/edición
- Vista detalle

### PropiedadForm
Formulario con tabs para organizar la información:
- **Datos Básicos**: Código, tipo, precio, etc.
- **Ubicación**: Dirección, coordenadas
- **Características**: Ambientes, metros, etc.
- **Amenities**: Servicios disponibles
- **Media**: Gestión de multimedia

### MediaUploader
Componente para subida de archivos con:
- Drag & drop
- Validación de tipos
- Progreso de subida
- Reordenamiento visual

### ExternalUrlManager
Gestión de URLs externas con:
- Validación automática
- Detección de tipo de contenido
- Soporte para múltiples plataformas

## 🔒 Seguridad

### Autenticación
- JWT tokens con refresh automático
- Middleware de autenticación en cada endpoint

### Autorización
- Sistema de roles granular
- Validación de permisos en frontend y backend
- Endpoints protegidos por roles específicos

### Validación
- DTOs con DataAnnotations en backend
- Esquemas Zod en frontend
- Validación de archivos por tipo y tamaño

## 🎯 Características Técnicas

### Performance
- Paginación optimizada
- Carga lazy de imágenes
- Conversión automática a WebP
- Compresión de imágenes

### UX/UI
- Interfaz responsive
- Feedback visual inmediato
- Estados de loading
- Manejo de errores

### Multimedia
- Soporte para múltiples formatos
- Optimización automática
- URLs externas procesadas
- Reordenamiento intuitivo

## 🔧 Configuraciones Avanzadas

### Límites de Archivos
```csharp
// En MediaService.cs
private readonly long _maxFileSize = 50 * 1024 * 1024; // 50MB
private readonly string[] _allowedImageTypes = { ".jpg", ".jpeg", ".png", ".webp" };
```

### Calidad de Imágenes
```csharp
// Configuración de compresión WebP
var optimizedBytes = await _imageProcessing.ResizeAndConvertAsync(
    file, 
    maxWidth: 1920, 
    maxHeight: 1080, 
    quality: 85
);
```

### Filtros de Búsqueda
```javascript
const filtros = {
  operacion: 'Venta',
  tipo: 'Departamento',
  precioMin: 100000,
  precioMax: 500000,
  ambientes: 3,
  barrio: 'Palermo'
};
```

## 🐛 Troubleshooting

### Problemas Comunes

1. **Error al subir imágenes**
   - Verificar tamaño del archivo (máx 50MB)
   - Verificar formato soportado
   - Verificar permisos de escritura

2. **URLs externas no funcionan**
   - Verificar que la URL sea pública
   - Verificar formato de la URL
   - Verificar conectividad

3. **Problemas de permisos**
   - Verificar rol del usuario
   - Verificar token JWT válido
   - Verificar configuración de roles

## 📊 Métricas y Monitoreo

### Logs Disponibles
- Creación/edición de propiedades
- Subida de multimedia
- Errores de validación
- Accesos no autorizados

### Performance
- Tiempo de carga de listas
- Tiempo de subida de archivos
- Uso de memoria en procesamiento

## 🔄 Versionado y Migración

### Versión Actual: 1.0.0

### Migraciones Futuras
- Soporte para tours virtuales 3D
- Integración con mapas interactivos
- Sistema de favoritos
- Comparador de propiedades

## 👥 Contribución

Para contribuir al módulo:
1. Fork del repositorio
2. Crear branch feature
3. Seguir convenciones de código
4. Escribir tests unitarios
5. Crear pull request

## 📞 Soporte

Para soporte técnico:
- Documentación: Este README
- Issues: GitHub Issues
- Contact: Equipo de desarrollo

---

**Última actualización**: Septiembre 2024  
**Versión**: 1.0.0  
**Compatibilidad**: ASP.NET Core 8, React 18+
