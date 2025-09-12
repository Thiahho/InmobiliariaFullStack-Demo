import React, { useState } from 'react';
import { useAuthStore } from '../../store/authStore';
import { axiosClient } from '../../lib/axiosClient';
// import { useNavigate } from 'react-router-dom';

interface LoginProps {
  isOpen?: boolean;
  setIsOpen?: (open: boolean) => void;
}

export default function Login({ isOpen: propIsOpen, setIsOpen: propSetIsOpen }: LoginProps = {}) {
  const [internalIsOpen, setInternalIsOpen] = useState(false);
  
  const isOpen = propIsOpen !== undefined ? propIsOpen : internalIsOpen;
  const setIsOpen = propSetIsOpen || setInternalIsOpen;
  const [formData, setFormData] = useState({
    email: '',
    password: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  
  const login = useAuthStore(state => state.login);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData(prev => ({
      ...prev,
      [e.target.name]: e.target.value
    }));
    setError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.email || !formData.password) {
      setError('Por favor completa todos los campos');
      return;
    }

    setLoading(true);
    setError('');

    try {
      console.log("Enviando datos de login:", formData);
      const response = await axiosClient.post('/auth/login', formData);
      console.log("Respuesta completa del login:", response);
      console.log("Data de la respuesta:", response.data);
      
      if (response.data) {
        const { token, AccessToken, RefreshToken, Agente } = response.data;
        console.log("Tokens extraídos:", { token, AccessToken, RefreshToken, Agente });
        
        // Guardar tokens en localStorage (usar 'token' como en DrCell)
        const accessToken = token || AccessToken; // Priorizar 'token'
        localStorage.setItem('access_token', accessToken);
        localStorage.setItem('refresh_token', RefreshToken);
        console.log("Tokens guardados en localStorage");
        
        // Verificar que se guardaron correctamente
        console.log("Verificación - Access Token guardado:", localStorage.getItem('access_token'));
        console.log("Verificación - Refresh Token guardado:", localStorage.getItem('refresh_token'));
        
        // Actualizar el store de autenticación
        login(Agente || {}, Agente?.Rol || 'Admin');
        
        // Cerrar modal y rediriger al panel
        setIsOpen(false);
        window.location.href = '/admin';
      } else {
        console.error("No hay data en la respuesta:", response);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'Error al iniciar sesión');
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      {/* <button 
        onClick={() => setIsOpen(true)}
        className="fixed top-4 right-4 bg-blue-500 text-white px-4 py-2 rounded-lg hover:bg-blue-600 z-50"
      >
        Iniciar Sesión
      </button> */}
      
      {isOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white p-8 rounded-lg max-w-md w-full mx-4">
            <div className="flex justify-between items-center mb-6">
              <h2 className="text-2xl font-bold">Iniciar Sesión</h2>
              <button 
                onClick={() => setIsOpen(false)}
                className="text-gray-500 hover:text-gray-700"
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
                {loading ? 'Ingresando...' : 'Ingresar'}
              </button>
            </form>
          </div>
        </div>
      )}
    </>
  );
}