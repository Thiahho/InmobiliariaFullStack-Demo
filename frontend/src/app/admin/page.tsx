"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { useAuthStore } from "../../store/authStore";
import { useRouter } from "next/navigation";
import { axiosClient } from "../../lib/axiosClient";
import PropiedadesSection from "../../components/propiedades/PropiedadesSection";
// import TestPropiedades from '../../components/propiedades/TestPropiedades';

// Definir la interfaz Visita FUERA del componente con todas las propiedades necesarias
interface Visita {
  id: string | number;
  clienteNombre: string;
  propiedadDireccion?: string;
  fechaHora: string;
}

export default function AdminPage() {
  const { isAuthenticated, user, role, logout } = useAuthStore();
  const router = useRouter();
  const [loading, setLoading] = useState(true);
  const [userVisits, setUserVisits] = useState<Visita[]>([]); // ✅ Tipado correcto
  const [loadingVisits, setLoadingVisits] = useState(false);

  useEffect(() => {
    const checkAuth = async () => {
      try {
        const token = localStorage.getItem("access_token");
        if (!token) {
          router.push("/");
          return;
        }

        // Verificar si el token es válido
        const response = await axiosClient.get("/auth/me");
        if (response.data) {
          const userData = response.data;
          console.log("Datos del usuario desde /auth/me:", userData);
          useAuthStore
            .getState()
            .login(userData, userData.rol || userData.role);
        }
      } catch (error) {
        console.error("Error verificando autenticación:", error);
        localStorage.removeItem("access_token");
        localStorage.removeItem("refresh_token");
        router.push("/");
      } finally {
        setLoading(false);
      }
    };

    checkAuth();
  }, [router]);

  // Obtener visitas del usuario
  useEffect(() => {
    const fetchUserVisits = async () => {
      if (!user?.id || !isAuthenticated) return;

      setLoadingVisits(true);
      try {
        const response = await axiosClient.get(`/visita/agente/${user.id}`);
        setUserVisits(response.data.slice(0, 3)); // Mostrar solo las primeras 3
      } catch (error) {
        console.error("Error obteniendo visitas del usuario:", error);
      } finally {
        setLoadingVisits(false);
      }
    };

    fetchUserVisits();
  }, [user?.id, isAuthenticated]);

  const handleLogout = async () => {
    try {
      await axiosClient.post("/auth/logout");
    } catch (error) {
      console.error("Error al hacer logout:", error);
    } finally {
      localStorage.removeItem("access_token");
      localStorage.removeItem("refresh_token");
      logout();
      router.push("/");
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-500 mx-auto"></div>
          <p className="mt-4 text-gray-600">Cargando...</p>
        </div>
      </div>
    );
  }

  if (!isAuthenticated || !user) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-800 mb-4">
            Acceso no autorizado
          </h1>
          <button
            onClick={() => router.push("/")}
            className="bg-blue-500 text-white px-6 py-2 rounded-lg hover:bg-blue-600"
          >
            Volver al inicio
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      <header className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-6">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">
                Panel de Administración
              </h1>
              <p className="text-sm text-gray-500">Bienvenido, {user.nombre}</p>
            </div>
            <div className="flex items-center space-x-4">
              <div className="text-right">
                <p className="text-sm font-medium text-gray-900">
                  {user.nombre}
                </p>
                <p className="text-xs text-gray-500">{role}</p>
              </div>
              <Link
                href="/"
                className="bg-white border px-4 py-2 rounded-lg hover:bg-gray-50 text-sm"
              >
                Ir al Inicio
              </Link>
              <Link
                href="/perfil"
                className="bg-white border px-4 py-2 rounded-lg hover:bg-gray-50 text-sm"
              >
                Mi Perfil
              </Link>
              <button
                onClick={handleLogout}
                className="bg-red-500 text-white px-4 py-2 rounded-lg hover:bg-red-600 text-sm"
              >
                Cerrar Sesión
              </button>
            </div>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {/* Card: Gestión de Propiedades */}
            <PropiedadesSection />

            {/* Card: Gestión de Leads */}
            <div className="bg-white overflow-hidden shadow rounded-lg">
              <div className="p-5">
                <div className="flex items-center">
                  <div className="flex-shrink-0">
                    <div className="w-8 h-8 bg-green-500 rounded-lg flex items-center justify-center">
                      👥
                    </div>
                  </div>
                  <div className="ml-5 w-0 flex-1">
                    <dl>
                      <dt className="text-sm font-medium text-gray-500 truncate">
                        Leads
                      </dt>
                      <dd className="text-lg font-medium text-gray-900">
                        Gestionar clientes
                      </dd>
                    </dl>
                  </div>
                </div>
              </div>
              <div className="bg-gray-50 px-5 py-3">
                <div className="text-sm">
                  <Link
                    href="/admin/leads"
                    className="font-medium text-green-600 hover:text-green-500"
                  >
                    Ver todos los leads →
                  </Link>
                </div>
              </div>
            </div>

            {/* Card: Gestión de Visitas */}
            <div className="bg-white overflow-hidden shadow rounded-lg">
              <div className="p-5">
                <div className="flex items-center">
                  <div className="flex-shrink-0">
                    <div className="w-8 h-8 bg-yellow-500 rounded-lg flex items-center justify-center">
                      📅
                    </div>
                  </div>
                  <div className="ml-5 w-0 flex-1">
                    <dl>
                      <dt className="text-sm font-medium text-gray-500 truncate">
                        Visitas
                      </dt>
                      <dd className="text-lg font-medium text-gray-900">
                        Programar visitas
                      </dd>
                    </dl>
                  </div>
                </div>
              </div>
              <div className="bg-gray-50 px-5 py-3">
                <div className="text-sm">
                  <Link
                    href="/admin/visitas"
                    className="font-medium text-yellow-600 hover:text-yellow-500"
                  >
                    Ver todas las visitas →
                  </Link>
                </div>
              </div>
            </div>

            {role === "Admin" && (
              <>
                {/* Card: Gestión de Agentes */}
                <div className="bg-white overflow-hidden shadow rounded-lg">
                  <div className="p-5">
                    <div className="flex items-center">
                      <div className="flex-shrink-0">
                        <div className="w-8 h-8 bg-purple-500 rounded-lg flex items-center justify-center">
                          👤
                        </div>
                      </div>
                      <div className="ml-5 w-0 flex-1">
                        <dl>
                          <dt className="text-sm font-medium text-gray-500 truncate">
                            Agentes
                          </dt>
                          <dd className="text-lg font-medium text-gray-900">
                            Gestionar usuarios
                          </dd>
                        </dl>
                      </div>
                    </div>
                  </div>
                  <div className="bg-gray-50 px-5 py-3">
                    <div className="text-sm">
                      <button
                        onClick={() => router.push("/admin/agentes")}
                        className="font-medium text-purple-600 hover:text-purple-500"
                      >
                        Ver todos los agentes →
                      </button>
                    </div>
                  </div>
                </div>

                {/* Card: Reportes */}
                {/* <div className="bg-white overflow-hidden shadow rounded-lg">
                  <div className="p-5">
                    <div className="flex items-center">
                      <div className="flex-shrink-0">
                        <div className="w-8 h-8 bg-red-500 rounded-lg flex items-center justify-center">
                          📊
                        </div>
                      </div>
                      <div className="ml-5 w-0 flex-1">
                        <dl>
                          <dt className="text-sm font-medium text-gray-500 truncate">
                            Reportes
                          </dt>
                          <dd className="text-lg font-medium text-gray-900">
                            Análisis y estadísticas
                          </dd>
                        </dl>
                      </div>
                    </div>
                  </div>
                  <div className="bg-gray-50 px-5 py-3">
                    <div className="text-sm">
                      <button className="font-medium text-red-600 hover:text-red-500">
                        Ver reportes →
                      </button>
                    </div>
                  </div>
                </div> */}
              </>
            )}
          </div>

          {/* Test de Propiedades - Comentado temporalmente */}
          {/* <div className="mt-8">
            <TestPropiedades />
          </div> */}

          {/* Información del usuario */}
          <div className="mt-8 bg-white shadow rounded-lg">
            <div className="px-6 py-4">
              <h3 className="text-lg font-medium text-gray-900">
                Información del Usuario
              </h3>
              <div className="mt-4 grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-500">
                    Email
                  </label>
                  <p className="mt-1 text-sm text-gray-900">{user.email}</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-500">
                    Rol
                  </label>
                  <p className="mt-1 text-sm text-gray-900">{role}</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-500">
                    Teléfono
                  </label>
                  <p className="mt-1 text-sm text-gray-900">
                    {user.telefono || "No especificado"}
                  </p>
                </div>
                {/* <div>
                  <label className="block text-sm font-medium text-gray-500">
                    Último Login
                  </label>
                  <p className="mt-1 text-sm text-gray-900">
                    {user.ultimoLogin
                      ? new Date(user.ultimoLogin).toLocaleString()
                      : "Nunca"}
                  </p>
                </div> */}
              </div>

              {/* Visitas Asignadas */}
              <div className="mt-6 border-t pt-6">
                <div className="flex justify-between items-center mb-4">
                  <h4 className="text-md font-medium text-gray-900">
                    Mis Próximas Visitas
                  </h4>
                  <Link
                    href="/admin/visitas"
                    className="text-sm text-blue-600 hover:text-blue-800"
                  >
                    Ver todas →
                  </Link>
                </div>

                {loadingVisits ? (
                  <div className="text-center py-4">
                    <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600 mx-auto"></div>
                  </div>
                ) : userVisits.length > 0 ? (
                  <div className="space-y-3">
                    {userVisits.map((visita, index) => (
                      <div
                        key={visita.id || index}
                        className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                      >
                        <div>
                          <p className="text-sm font-medium text-gray-900">
                            {visita.clienteNombre}
                          </p>
                          <p className="text-xs text-gray-500">
                            {visita.propiedadDireccion ||
                              "Dirección no disponible"}
                          </p>
                        </div>
                        <div className="text-right">
                          <p className="text-sm text-gray-900">
                            {new Date(visita.fechaHora).toLocaleDateString()}
                          </p>
                          <p className="text-xs text-gray-500">
                            {new Date(visita.fechaHora).toLocaleTimeString([], {
                              hour: "2-digit",
                              minute: "2-digit",
                            })}
                          </p>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div className="text-center py-4">
                    <p className="text-sm text-gray-500">
                      No tienes visitas asignadas
                    </p>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}
