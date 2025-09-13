import React, { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { updateProfileSchema } from "../../schemas/userSchemas";
import { useAuthStore } from "../../store/authStore";
import { toast, Toaster } from "react-hot-toast";

const UserProfile = () => {
  const { user, updateProfile, role } = useAuthStore();

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm({
    resolver: zodResolver(updateProfileSchema),
    defaultValues: {
      nombre: user?.name || user?.nombre || "",
      email: user?.email || "",
      telefono: user?.telefono || "",
    },
  });

  // Sincroniza el formulario cuando cambie el usuario en el store
  useEffect(() => {
    reset({
      nombre: user?.nombre || user?.name || "",
      email: user?.email || "",
      telefono: user?.telefono || "",
      password: "",
    });
  }, [user, reset]);

  const onSubmit = async (values) => {
    try {
      const updated = await updateProfile(values);
      // Reflejar cambios en el formulario
      reset({
        nombre: updated?.nombre || values.nombre,
        email: updated?.email || values.email,
        telefono: updated?.telefono || values.telefono,
        password: "",
      });
      toast.success("Perfil actualizado con éxito");
    } catch (e) {
      toast.error(e.message || "Ocurrió un error al actualizar");
    }
  };

  return (
    <section className="py-16">
      <div className="container mx-auto px-4 max-w-2xl">
        <h2 className="text-2xl font-semibold mb-6">Mi perfil</h2>
        <Toaster position="top-right" />
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">Nombre</label>
            <input className="w-full rounded border px-3 py-2" {...register("nombre")} />
            {errors.nombre && <p className="text-sm text-red-600">{errors.nombre.message}</p>}
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Email</label>
            <input className="w-full rounded border px-3 py-2" type="email" {...register("email")} />
            {errors.email && <p className="text-sm text-red-600">{errors.email.message}</p>}
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Teléfono</label>
            <input className="w-full rounded border px-3 py-2" {...register("telefono")} />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Nueva contraseña (opcional)</label>
            <input className="w-full rounded border px-3 py-2" type="password" {...register("password")} />
            {errors.password && <p className="text-sm text-red-600">{errors.password.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">Rol</label>
            <input className="w-full rounded border px-3 py-2 bg-gray-50" value={role || ""} disabled readOnly />
            <p className="text-xs text-gray-500 mt-1">No es posible cambiar de rol desde el perfil.</p>
          </div>

          <div className="flex gap-2">
            <button type="submit" className="rounded bg-blue-600 text-white px-4 py-2">Guardar</button>
            <button type="button" onClick={() => reset()} className="rounded border px-4 py-2">Restablecer</button>
          </div>
        </form>
      </div>
    </section>
  );
};

export default UserProfile;


