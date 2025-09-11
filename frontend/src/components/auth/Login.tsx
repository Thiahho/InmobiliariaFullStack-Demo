import React, { useState } from 'react';
import { useAuthStore } from '../../store/authStore';
import { axiosClient } from '../../lib/axiosClient';
// import { useNavigate } from 'react-router-dom';

export default function Login() {
  const [isOpen, setIsOpen] = useState(false);
  const [formData, setFormData] = useState({
    email: '',
    password: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  
  const login = useAuthStore(state => state.login);

  const handleInputChange = (e) => {
    setFormData(prev => ({
      ...prev,
      [e.target.name]: e.target.value
    }));
    setError('');
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!formData.email || !formData.password) {
      setError('Por favor completa todos los campos');
      return;
    }

    setLoading(true);
    setError('');

    try {
      const response = await axiosClient.post('/auth/login', formData);
      
      if (response.data) {
        const { AccessToken, RefreshToken, Agente } = response.data;
        
        // Guardar tokens en localStorage
        localStorage.setItem('access_token', AccessToken);
        localStorage.setItem('refresh_token', RefreshToken);
        
        // Actualizar el store de autenticación
        login(Agente || {}, Agente?.Rol || 'Admin');
        
        // Cerrar modal y rediriger al panel
        setIsOpen(false);
        window.location.href = '/admin';
      }
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Error al iniciar sesión');
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <button 
        onClick={() => setIsOpen(true)}
        className="fixed top-4 right-4 bg-blue-500 text-white px-4 py-2 rounded-lg hover:bg-blue-600 z-50"
      >
        Iniciar Sesión
      </button>
      
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