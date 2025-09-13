import { z } from "zod";

export const createUserSchema = z.object({
  nombre: z.string().min(2, "Nombre requerido"),
  email: z.string().email("Email inválido"),
  telefono: z.string().optional(),
  password: z.string().min(6, "Mínimo 6 caracteres"),
  rol: z.enum(["Agente", "Cargador"], {
    required_error: "Rol requerido",
  }),
  activo: z.boolean().default(true),
});

export const updateProfileSchema = z.object({
  nombre: z.string().min(2, "Nombre requerido"),
  email: z.string().email("Email inválido"),
  telefono: z.string().optional(),
  password: z.string().min(6).optional(),
});


