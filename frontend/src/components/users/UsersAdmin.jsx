import React, { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { createUserSchema } from "../../schemas/userSchemas";
import { useUsersStore } from "../../store/usersStore";
import { useAuthStore, Roles } from "../../store/authStore";

const UsersAdmin = () => {
  const { users, loading, fetchUsers, createUser, toggleActivo } = useUsersStore();
  const { role } = useAuthStore();
  const isAdmin = role === Roles.Admin;

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm({ resolver: zodResolver(createUserSchema) });

  const onSubmit = async (values) => {
    if (!isAdmin) return;
    await createUser(values);
    reset();
  };

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  return (
    <section className="py-16">
      <div className="container mx-auto px-4">
        <h2 className="text-2xl font-semibold mb-6">Gestión de Usuarios</h2>

        {/* Alta de usuarios (solo Admin) */}
        {isAdmin && (
          <div className="rounded-lg border bg-white p-5 shadow-sm mb-8">
            <h3 className="font-semibold mb-4">Crear nuevo usuario</h3>
            <form onSubmit={handleSubmit(onSubmit)} className="grid grid-cols-1 md:grid-cols-5 gap-3">
              <input className="rounded border px-3 py-2" placeholder="Nombre" {...register("nombre")} />
              {errors.nombre && <p className="text-sm text-red-600 md:col-span-5">{errors.nombre.message}</p>}

              <input className="rounded border px-3 py-2" placeholder="Email" type="email" {...register("email")} />
              {errors.email && <p className="text-sm text-red-600 md:col-span-5">{errors.email.message}</p>}

              <input className="rounded border px-3 py-2" placeholder="Teléfono" {...register("telefono")} />

              <select className="rounded border px-3 py-2" {...register("rol")}>
                <option value="Agente">Agente</option>
                <option value="Cargador">Cargador</option>
              </select>

              <input className="rounded border px-3 py-2" placeholder="Contraseña" type="password" {...register("password")} />
              {errors.password && <p className="text-sm text-red-600 md:col-span-5">{errors.password.message}</p>}

              <div className="flex items-center gap-2">
                <input type="checkbox" id="activo" {...register("activo")} defaultChecked />
                <label htmlFor="activo">Activo</label>
              </div>

              <div className="md:col-span-5">
                <button type="submit" className="rounded bg-blue-600 text-white px-4 py-2">Crear usuario</button>
              </div>
            </form>
          </div>
        )}

        {/* Lista de usuarios */}
        <div className="rounded-lg border bg-white p-5 shadow-sm">
          <h3 className="font-semibold mb-4">Usuarios</h3>
          {loading && <p className="text-sm text-gray-500 mb-2">Cargando usuarios...</p>}
          <div className="overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead>
                <tr className="text-left text-gray-600">
                  <th className="py-2 pr-4">Nombre</th>
                  <th className="py-2 pr-4">Email</th>
                  <th className="py-2 pr-4">Teléfono</th>
                  <th className="py-2 pr-4">Rol</th>
                  <th className="py-2 pr-4">Estado</th>
                  {isAdmin && <th className="py-2 pr-4">Acciones</th>}
                </tr>
              </thead>
              <tbody>
                {users.map((u) => (
                  <tr key={u.id} className="border-t">
                    <td className="py-2 pr-4">{u.nombre}</td>
                    <td className="py-2 pr-4">{u.email}</td>
                    <td className="py-2 pr-4">{u.telefono || "-"}</td>
                    <td className="py-2 pr-4">{u.rol}</td>
                    <td className="py-2 pr-4">
                      <span className={`px-2 py-1 rounded text-xs ${u.activo ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-600"}`}>
                        {u.activo ? "Activo" : "Inactivo"}
                      </span>
                    </td>
                    {isAdmin && (
                      <td className="py-2 pr-4">
                        <button onClick={() => toggleActivo(u.id)} className="rounded border px-3 py-1">
                          {u.activo ? "Desactivar" : "Activar"}
                        </button>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </section>
  );
  return (
    <div className="min-h-screen bg-gray-100">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
            <div className="flex items-center justify-between mb-4">
                <h1 className="text-3xl font-bold">Mi perfil</h1>
                <a href="/admin" className="inline-flex items-center gap-2 rounded bg-blue-600 text-white px-4 py-2 hover:bg-blue-700">Volver al Panel</a>
            </div>
            <UserProfile />
        </div>
    </div>
);
};

export default UsersAdmin;


