import React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { loginSchema } from "../../schemas/authSchemas";

const Login = () => {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm({ resolver: zodResolver(loginSchema) });

  const onSubmit = (data) => {
    // Estático: sin llamadas a API
    // eslint-disable-next-line no-console
    console.log("login submit", data);
  };

  return (
    <section className="py-16 bg-white">
      <div className="container mx-auto px-4 max-w-md">
        <h2 className="text-2xl font-semibold mb-6">Iniciar sesión</h2>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1" htmlFor="email">
              Correo electrónico
            </label>
            <input
              id="email"
              type="email"
              className="w-full rounded-md border border-gray-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-inmobiliaria-500"
              placeholder="tu@email.com"
              {...register("email")}
            />
            {errors.email && (
              <p className="mt-1 text-sm text-red-600">{errors.email.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium mb-1" htmlFor="password">
              Contraseña
            </label>
            <input
              id="password"
              type="password"
              className="w-full rounded-md border border-gray-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-inmobiliaria-500"
              placeholder="********"
              {...register("password")}
            />
            {errors.password && (
              <p className="mt-1 text-sm text-red-600">{errors.password.message}</p>
            )}
          </div>

          <button
            type="submit"
            className="w-full rounded-md bg-inmobiliaria-600 px-4 py-2 text-white hover:bg-inmobiliaria-700 transition"
          >
            Acceder
          </button>
        </form>
      </div>
    </section>
  );
};

export default Login;


