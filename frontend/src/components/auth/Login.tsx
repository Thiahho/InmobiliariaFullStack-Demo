import React, { useState } from "react";
import { useAuthStore } from "../../store/authStore";
import { axiosClient } from "../../lib/axiosClient";

type LoginProps = {
  isOpen?: boolean;
  setIsOpen?: (open: boolean) => void;
};

type LoginForm = {
  email: string;
  password: string;
};

type Agente = {
  Rol?: string;
  [k: string]: unknown;
};

type LoginResponse = {
  token?: string;
  AccessToken?: string;
  RefreshToken?: string;
  Agente?: Agente;
};

type AuthShape = {
  login: (user: Agente, role: string) => void;
};

const Login: React.FC<LoginProps> = (props) => {
  const { isOpen: propIsOpen, setIsOpen: propSetIsOpen } = props ?? {};
  const [internalIsOpen, setInternalIsOpen] = useState(false);

  const isOpen = propIsOpen ?? internalIsOpen;
  const setIsOpen = propSetIsOpen ?? setInternalIsOpen;

  const [formData, setFormData] = useState<LoginForm>({ email: "", password: "" });
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string>("");

  const { login } = useAuthStore() as unknown as AuthShape;

  const handleInputChange: React.ChangeEventHandler<HTMLInputElement> = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    setError("");
  };

  const handleSubmit: React.FormEventHandler<HTMLFormElement> = async (e) => {
    e.preventDefault();
    if (!formData.email || !formData.password) {
      setError("Por favor completa todos los campos");
      return;
    }

    setLoading(true);
    setError("");

    try {
      const response = await axiosClient.post<LoginResponse>("/auth/login", formData);
      const data = response.data;

      if (data) {
        const { token, AccessToken, RefreshToken, Agente } = data;
        const accessToken = token || AccessToken || "";

        localStorage.setItem("access_token", accessToken);
        if (RefreshToken) localStorage.setItem("refresh_token", RefreshToken);

        login(Agente || {}, Agente?.Rol || "Admin");

        setIsOpen(false);
        window.location.href = "/admin";
      } else {
        setError("Respuesta inválida del servidor");
      }
    } catch (err) {
      const msg =
        (err as any)?.response?.data?.message ??
        (err as Error)?.message ??
        "Error al iniciar sesión";
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      {isOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white p-8 rounded-lg max-w-md w-full mx-4">
            <div className="flex justify-between items-center mb-6">
              <h2 className="text-2xl font-bold">Iniciar Sesión</h2>
              <button
                onClick={() => setIsOpen(false)}
                className="text-gray-500 hover:text-gray-700"
                type="button"
              >
                ✕
              </button>
            </div>

            {error && (
              <div className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">
                {error}
              </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">Email</label>
                <input
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleInputChange}
                  className="w-full p-3 border rounded-lg focus:ring-2 focus:ring-blue-500"
                  placeholder="tu@email.com"
                  disabled={loading}
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">Contraseña</label>
                <input
                  type="password"
                  name="password"
                  value={formData.password}
                  onChange={handleInputChange}
                  className="w-full p-3 border rounded-lg focus:ring-2 focus:ring-blue-500"
                  placeholder="••••••••"
                  disabled={loading}
                  required
                />
              </div>

              <button
                type="submit"
                disabled={loading}
                className="w-full bg-blue-500 text-white py-3 rounded-lg hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {loading ? "Ingresando..." : "Ingresar"}
              </button>
            </form>
          </div>
        </div>
      )}
    </>
  );
};

export default Login;
