import { z } from 'zod';

export const visitaCreateSchema = z.object({
  propiedadId: z.number().min(1, 'Propiedad es requerida'),
  agenteId: z.number().min(1, 'Agente es requerido'),
  clienteNombre: z.string().min(2, 'Nombre del cliente es requerido').max(100),
  clienteTelefono: z.string().optional().nullable(),
  clienteEmail: z.union([
    z.string().email('Email inválido'),
    z.string().length(0),
    z.literal(null),
    z.literal(undefined)
  ]).optional().nullable(),
  fechaHora: z.date('Fecha y hora son requeridas'),
  duracionMinutos: z.number().min(30, 'Duración mínima 30 minutos').max(480, 'Duración máxima 8 horas').default(60),
  observaciones: z.string().max(500).optional().nullable()
});

export const visitaUpdateSchema = visitaCreateSchema.extend({
  id: z.number().min(1),
  estado: z.enum(['Pendiente', 'Confirmada', 'Realizada', 'Cancelada']).optional(),
  notasVisita: z.string().max(1000).optional().nullable()
});

export const visitaSearchSchema = z.object({
  agenteId: z.number().optional(),
  propiedadId: z.number().optional(),
  estado: z.string().optional(),
  fechaDesde: z.date().optional(),
  fechaHasta: z.date().optional(),
  orderBy: z.string().default('FechaHora'),
  orderDesc: z.boolean().default(false),
  page: z.number().min(1).default(1),
  pageSize: z.number().min(1).max(100).default(20)
});

export const visitaBulkActionSchema = z.object({
  visitaIds: z.array(z.number()).min(1, 'Debe seleccionar al menos una visita'),
  accion: z.enum(['Confirmar', 'Cancelar', 'Reagendar']),
  nuevaFecha: z.date().optional(),
  motivo: z.string().optional()
});

export const agendarVisitaPublicSchema = z.object({
  nombre: z.string().min(2, 'Nombre es requerido').max(100),
  telefono: z.string().min(8, 'Teléfono es requerido').max(20),
  email: z.string().email('Email inválido'),
  fechaPreferida: z.date('Fecha preferida es requerida'),
  horaPreferida: z.string().regex(/^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/, 'Formato de hora inválido'),
  mensaje: z.string().max(500).optional()
});
