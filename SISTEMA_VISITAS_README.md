# Sistema de Visitas - Inmobiliaria Full Stack

## 📋 Descripción General

Sistema completo para la gestión de visitas a propiedades inmobiliarias que incluye funcionalidades avanzadas de agenda, notificaciones automáticas, recordatorios y una interfaz pública para solicitar visitas.

## 🏗️ Arquitectura

### Backend (ASP.NET Core 8)
- **Controllers**: VisitaController, LeadController (solicitar-visita)
- **Models**: Visita, VisitaAuditLog, AuditLog
- **Services**: VisitaService, EmailService, VisitaJobService, VisitaAuditoriaService
- **DTOs**: VisitaCreateDto, VisitaUpdateDto, VisitaResponseDto, etc.
- **Jobs**: Hangfire para recordatorios automáticos
- **Email**: MailKit con archivos ICS adjuntos

### Frontend (React + Next.js + TypeScript)
- **Store**: Zustand para manejo de estado global (visitasStore)
- **Components**: Componentes modulares y reutilizables
- **Schemas**: Validación con Zod
- **UI**: TailwindCSS + Heroicons
- **HTTP Client**: Axios con interceptores

## 🔧 Funcionalidades Principales

### 1. Gestión Admin de Visitas (/admin/visitas)
- ✅ **Vista Agenda Semanal** con drag & drop para reprogramar
- ✅ **Vista Lista** con filtros avanzados y acciones masivas
- ✅ **Formulario Nueva Visita** con validación de conflictos inline
- ✅ **Selector de Propiedad** con autocomplete por código/barrio
- ✅ **Selector de Lead** existente o alta rápida de cliente
- ✅ **Validación de Solapamiento** en tiempo real
- ✅ **Estados de Visita** con colores diferenciados
- ✅ **Acciones por Estado**: Confirmar, Cancelar, Marcar Realizada

### 2. Sistema de Notificaciones y Emails
- ✅ **Emails con ICS** automáticos al confirmar visitas
- ✅ **Recordatorios 24h antes** para visitas confirmadas
- ✅ **Notificaciones de cancelación** y reprogramación
- ✅ **Templates HTML** profesionales y responsivos
- ✅ **Archivos ICS** para agregar a calendario
- ✅ **Jobs en background** con Hangfire

### 3. UI Pública para Solicitar Visitas
- ✅ **Botón "Agendar Visita"** integrado en ficha de propiedad
- ✅ **Modal con formulario** para datos del cliente
- ✅ **Selector de fecha/hora** preferida
- ✅ **Validación completa** con Zod
- ✅ **Confirmación visual** del envío
- ✅ **Creación automática** de Lead tipo "Visita"

### 4. Sistema de Auditoría
- ✅ **Tabla audit_events** para tracking completo
- ✅ **Registro de cambios** de estado y reprogramaciones
- ✅ **Metadatos completos** (usuario, IP, timestamp)
- ✅ **Historial por visita** y por usuario
- ✅ **Logs estructurados** en JSON

## 📁 Estructura de Archivos

### Backend
```
LandingBack/
├── Controllers/
│   ├── VisitaController.cs                 # API endpoints para visitas
│   └── LeadController.cs                   # Endpoint solicitar-visita
├── Data/
│   ├── Modelos/
│   │   ├── Visita.cs                       # Modelo principal de visita
│   │   ├── VisitaAuditLog.cs              # Auditoría específica de visitas
│   │   └── AuditLog.cs                     # Auditoría general
│   └── Dtos/
│       └── VisitaDto.cs                    # DTOs para transferencia de datos
├── Services/
│   ├── VisitaService.cs                    # Lógica de negocio principal
│   ├── EmailService.cs                     # Servicio de emails con ICS
│   ├── VisitaJobService.cs                # Jobs de recordatorios
│   └── VisitaAuditoriaService.cs          # Servicio de auditoría
└── Filters/
    └── HangfireDashboardAuthorizationFilter.cs
```

### Frontend
```
frontend/src/
├── components/visitas/
│   ├── VisitasAdmin.tsx                    # Componente principal admin
│   ├── AgendaSemanal.tsx                   # Vista agenda con drag & drop
│   ├── VisitaForm.tsx                      # Formulario nueva/editar visita
│   ├── AgendarVisitaModal.tsx             # Modal público solicitar visita
│   ├── BotonAgendarVisita.tsx             # Botón integrable en propiedades
│   └── index.ts                            # Exports
├── store/
│   └── visitasStore.js                     # Estado global con Zustand
├── schemas/
│   └── visitaSchemas.js                    # Validaciones con Zod
└── app/admin/visitas/
    └── page.tsx                            # Página admin de visitas
```

## 🎨 Estados y Colores

### Estados de Visita
- **Pendiente** (Amarillo): Visita creada, esperando confirmación
- **Confirmada** (Azul): Visita confirmada, recordatorios activados
- **Realizada** (Verde): Visita completada con éxito
- **Cancelada** (Rojo): Visita cancelada por cualquier motivo

### Flujo de Estados
```
Pendiente → Confirmar → Confirmada → Marcar Realizada → Realizada
    ↓           ↓            ↓
Cancelar → Cancelada    Cancelar → Cancelada
```

## ⚙️ Configuración

### Variables de Entorno (appsettings.json)
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "FromName": "Inmobiliaria - Sistema de Visitas",
    "FromAddress": "noreply@inmobiliaria.com",
    "Username": "tu-email@gmail.com",
    "Password": "tu-password-app"
  }
}
```

### Dependencias Requeridas
```xml
<!-- Backend -->
<PackageReference Include="Hangfire.Core" Version="1.8.14" />
<PackageReference Include="Hangfire.SqlServer" Version="1.8.14" />
<PackageReference Include="Hangfire.AspNetCore" Version="1.8.14" />
<PackageReference Include="MailKit" Version="4.7.1.1" />
<PackageReference Include="MimeKit" Version="4.7.1" />
```

```json
// Frontend
{
  "date-fns": "^3.6.0",
  "react-hook-form": "^7.52.1",
  "zod": "^3.23.8",
  "@hookform/resolvers": "^3.9.0",
  "react-hot-toast": "^2.4.1"
}
```

## 🚀 Uso del Sistema

### 1. Vista Admin
1. Acceder a `/admin/visitas`
2. Cambiar entre vista Agenda y Lista
3. Filtrar por agente, estado, fechas
4. Crear nueva visita con validación automática
5. Usar drag & drop para reprogramar en agenda
6. Gestionar estados con botones de acción

### 2. Uso Público
1. Botón "Agendar Visita" aparece en ficha de propiedad
2. Modal se abre con formulario pre-cargado
3. Cliente llena datos y selecciona fecha/hora preferida
4. Sistema crea Lead automáticamente
5. Agente recibe notificación para coordinar

### 3. Emails Automáticos
- **Confirmación**: Email con ICS al confirmar visita
- **Recordatorio**: 24h antes si estado = "Confirmada"
- **Cancelación**: Email de notificación con motivo
- **Reprogramación**: Email con nueva fecha e ICS actualizado

## 🔧 Endpoints API

### Visitas
- `GET /api/visita` - Listar visitas con filtros
- `GET /api/visita/{id}` - Obtener visita específica
- `POST /api/visita` - Crear nueva visita
- `PUT /api/visita/{id}` - Actualizar visita
- `DELETE /api/visita/{id}` - Eliminar visita
- `POST /api/visita/{id}/confirmar` - Confirmar visita
- `POST /api/visita/{id}/cancelar` - Cancelar visita
- `POST /api/visita/{id}/reagendar` - Reprogramar visita
- `POST /api/visita/{id}/realizada` - Marcar como realizada
- `GET /api/visita/calendar` - Obtener visitas para calendario
- `GET /api/visita/{id}/ics` - Descargar archivo ICS
- `POST /api/visita/validate-timeslot` - Validar disponibilidad
- `POST /api/visita/bulk-action` - Acciones masivas

### Leads/Solicitudes
- `POST /api/leads/solicitar-visita` - Solicitud pública de visita

## 🎯 Características Avanzadas

### Validación de Conflictos
- Verificación en tiempo real de solapamientos
- Consideración de horarios laborales
- Exclusión de domingos y feriados
- Validación tanto en cliente como servidor

### Drag & Drop en Agenda
- Reprogramación visual intuitiva
- Validación automática al soltar
- Feedback visual de conflictos
- Actualización inmediata del estado

### Sistema de Auditoría
- Tracking completo de cambios
- Metadatos de usuario, IP, timestamp
- Histórico por visita y por usuario
- Logs estructurados para análisis

### Notificaciones Inteligentes
- Templates dinámicos según tipo de acción
- Archivos ICS automáticos
- Recordatorios programados con Hangfire
- Cancelación automática de jobs obsoletos

## 📊 Dashboard Hangfire

Acceso: `/hangfire` (solo Admin en producción)
- Monitoreo de jobs de recordatorios
- Estadísticas de emails enviados
- Gestión de trabajos en cola
- Logs de errores y reintentos

## ✅ Definition of Done (DoD)

- ✅ **Crear/editar con antisolape** en app y DB
- ✅ **Emails con ICS** a lead/agente en confirmación
- ✅ **Recordatorio 24h** activo con jobs automáticos
- ✅ **Agenda semanal usable** con permisos y drag & drop
- ✅ **Estados consistentes** y auditados completamente
- ✅ **Modal público funcional** integrado en propiedades
- ✅ **Sistema completo** de backend a frontend

## 🔐 Seguridad

- Autorización por roles (Admin, Agente)
- Validación de entrada en todas las capas
- Sanitización de datos en emails
- Rate limiting (configurado en middleware)
- Logs de auditoría para compliance
- Dashboard Hangfire protegido en producción

## 🚀 Próximas Mejoras

- [ ] Integración con WhatsApp Business API
- [ ] Notificaciones push en tiempo real
- [ ] Sincronización con Google Calendar
- [ ] Analytics avanzados de conversión
- [ ] Templates de email personalizables
- [ ] API para integraciones externas
- [ ] Exportación de reportes en PDF
- [ ] Sistema de ratings post-visita

