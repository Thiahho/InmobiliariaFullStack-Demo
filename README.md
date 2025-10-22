🔹 README – Sistema Inmobiliario Full Stack
1. Sistema Inmobiliario Full Stack

Sistema Inmobiliario Full Stack – Gestión de Propiedades y Visitas

Breve descripción:

Plataforma web completa para la gestión de propiedades inmobiliarias, agenda de visitas, leads y usuarios, con panel administrativo y funcionalidades avanzadas de búsqueda, auditoría y notificaciones automáticas.

2. Funcionalidades Principales
Sistema de Gestión de Propiedades

CRUD completo de propiedades con filtros avanzados (tipo, operación, precio, ubicación, amenities)

Gestión multimedia (imágenes locales, videos, URLs externas)

Validación robusta con Zod schemas y DataAnnotations

Paginación optimizada para grandes volúmenes de datos

Sistema de Visitas

Agenda interactiva semanal con drag & drop

Validación automática de conflictos de horarios

Notificaciones por email con archivos ICS adjuntos

Recordatorios automáticos 24h antes usando Hangfire

Interfaz pública para solicitar visitas

Sistema de Usuarios y Autenticación

Autenticación JWT con refresh tokens

Roles de usuario: Admin, Agente, Cargador

Autorización granular por permisos específicos

Gestión de agentes y propiedades asignadas

Gestión de Leads

Captura automática desde formularios públicos

Clasificación por tipo (Consulta, Visita, Contacto)

Seguimiento del estado del lead y conexión con sistema de visitas

Sistema de Auditoría

Tracking completo de todas las operaciones

Logs estructurados con metadatos (usuario, IP, timestamp)

Historial de cambios por entidad

Monitoreo de rendimiento y errores

3. Stack Tecnológico

Backend:

.NET 8, Entity Framework Core, PostgreSQL

Clean Architecture + Repository/Service/DTO Patterns

AutoMapper, Serilog, Hangfire, MailKit, ImageSharp

Autenticación JWT y validaciones multi-capa

Frontend:

React + Next.js, TypeScript

Zustand para estado global, React Hook Form + Zod

TailwindCSS + Heroicons para UI, Framer Motion para animaciones

Axios con interceptores, React Query para cache y sincronización

DevOps / Calidad:

Health Checks, CORS configurado para producción

Rate limiting preparado

Error handling robusto y testing-ready

4. Logros Técnicos Destacados

UX/UI avanzada y responsive con drag & drop, validaciones en tiempo real y feedback inmediato

Seguridad robusta: JWT, permisos granulares, sanitización de inputs y HTTPS enforced

Arquitectura modular, escalable y optimizada para performance

Automatización inteligente de notificaciones, recordatorios y exportación de datos

Sistema listo para producción con documentación completa y mantenibilidad garantizada

5. Instalación y Deployment
Requisitos

Node.js + npm/yarn

.NET 8 SDK

PostgreSQL
