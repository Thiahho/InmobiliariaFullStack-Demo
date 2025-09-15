# Ejemplo de Integración del Módulo de Propiedades

## 📁 Archivos de Integración

### 1. Actualizar AdminPanel.jsx

```jsx
import React, { useState } from "react";
import { useAuthStore } from "../../store/authStore";
import PropiedadesAdminPanel from "./PropiedadesAdminPanel";
import {
  HomeIcon,
  UserGroupIcon,
  UserIcon,
  CalendarIcon,
  ChartBarIcon,
  CogIcon
} from "@heroicons/react/24/outline";

const cards = [
  { key: "propiedades", title: "Propiedades", count: 128, icon: HomeIcon },
  { key: "agentes", title: "Agentes", count: 12, icon: UserGroupIcon },
  { key: "leads", title: "Leads", count: 57, icon: UserIcon },
  { key: "visitas", title: "Visitas", count: 31, icon: CalendarIcon },
];

const AdminPanel = () => {
  const { hasPermission } = useAuthStore();
  const [activeModule, setActiveModule] = useState('dashboard');

  const handleModuleClick = (moduleKey) => {
    if (moduleKey === 'propiedades' && !hasPermission('view_dashboard')) {
      alert('No tienes permisos para acceder a este módulo');
      return;
    }
    setActiveModule(moduleKey);
  };

  const renderActiveModule = () => {
    switch (activeModule) {
      case 'propiedades':
        return <PropiedadesAdminPanel />;
      case 'agentes':
        return <div className="p-8">Módulo de Agentes (En desarrollo)</div>;
      case 'leads':
        return <div className="p-8">Módulo de Leads (En desarrollo)</div>;
      case 'visitas':
        return <div className="p-8">Módulo de Visitas (En desarrollo)</div>;
      default:
        return (
          <section className="py-16">
            <div className="container mx-auto px-4">
              <h2 className="text-2xl font-semibold mb-6">Panel Administrativo</h2>
              
              {/* Cards de estadísticas */}
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                {cards.map((card) => {
                  const IconComponent = card.icon;
                  return (
                    <div 
                      key={card.key} 
                      className="rounded-lg border bg-white p-5 shadow-sm hover:shadow-md transition-shadow cursor-pointer"
                      onClick={() => handleModuleClick(card.key)}
                    >
                      <div className="flex items-center justify-between">
                        <div>
                          <div className="text-sm text-gray-500">{card.title}</div>
                          <div className="text-3xl font-bold">{card.count}</div>
                        </div>
                        <IconComponent className="h-8 w-8 text-blue-600" />
                      </div>
                    </div>
                  );
                })}
              </div>

              {/* Widgets informativos */}
              <div className="mt-10 grid grid-cols-1 lg:grid-cols-2 gap-6">
                <div className="rounded-lg border bg-white p-5 shadow-sm">
                  <h3 className="font-semibold mb-2">Últimos leads</h3>
                  <ul className="space-y-2 text-sm text-gray-700">
                    <li>Juan Pérez • Dep. Palermo • 10/09</li>
                    <li>Ana Gómez • Casa Nordelta • 09/09</li>
                    <li>Carlos Ruiz • Monoambiente • 08/09</li>
                  </ul>
                </div>
                <div className="rounded-lg border bg-white p-5 shadow-sm">
                  <h3 className="font-semibold mb-2">Próximas visitas</h3>
                  <ul className="space-y-2 text-sm text-gray-700">
                    <li>11/09 • 15:00 • Nordelta</li>
                    <li>12/09 • 11:30 • Palermo</li>
                    <li>12/09 • 17:45 • Belgrano</li>
                  </ul>
                </div>
              </div>
            </div>
          </section>
        );
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Navigation Header */}
      {activeModule !== 'dashboard' && (
        <div className="bg-white shadow-sm border-b border-gray-200">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="flex items-center py-4">
              <button
                onClick={() => setActiveModule('dashboard')}
                className="inline-flex items-center px-3 py-2 border border-gray-300 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 mr-4"
              >
                <ChartBarIcon className="h-4 w-4 mr-2" />
                Dashboard
              </button>
              <nav className="flex space-x-4">
                {cards.map((card) => (
                  <button
                    key={card.key}
                    onClick={() => handleModuleClick(card.key)}
                    className={`px-3 py-2 rounded-md text-sm font-medium ${
                      activeModule === card.key
                        ? 'bg-blue-100 text-blue-700'
                        : 'text-gray-500 hover:text-gray-700'
                    }`}
                  >
                    {card.title}
                  </button>
                ))}
              </nav>
            </div>
          </div>
        </div>
      )}

      {/* Module Content */}
      {renderActiveModule()}
    </div>
  );
};

export default AdminPanel;
```

### 2. Actualizar authStore.js (si es necesario)

```javascript
// Agregar nuevos permisos si no existen
const rolePermissions = {
  [Roles.Admin]: [
    "view_dashboard",
    "manage_propiedades",
    "delete_propiedades", // Nuevo permiso
    "manage_agentes",
    "manage_leads",
    "manage_visitas",
    "upload_media",
  ],
  [Roles.Agente]: [
    "view_dashboard",
    "manage_propiedades",
    "manage_leads",
    "manage_visitas",
    "upload_media", // Agregar si no existe
  ],
  [Roles.Cargador]: [
    "upload_media", 
    "manage_propiedades"
  ],
};
```

### 3. Crear página de propiedades (si usas Next.js)

```jsx
// pages/admin/propiedades.js o app/admin/propiedades/page.tsx
import { useEffect } from 'react';
import { useRouter } from 'next/router';
import { useAuthStore } from '../../store/authStore';
import PropiedadesAdminPanel from '../../components/admin/PropiedadesAdminPanel';
import Layout from '../../components/Layout';

export default function PropiedadesPage() {
  const { isAuthenticated, hasPermission } = useAuthStore();
  const router = useRouter();

  useEffect(() => {
    if (!isAuthenticated) {
      router.push('/login');
      return;
    }

    if (!hasPermission('view_dashboard') && !hasPermission('manage_propiedades')) {
      router.push('/unauthorized');
      return;
    }
  }, [isAuthenticated, hasPermission, router]);

  if (!isAuthenticated) {
    return <div>Cargando...</div>;
  }

  return (
    <Layout>
      <PropiedadesAdminPanel />
    </Layout>
  );
}
```

### 4. Configurar rutas (ejemplo con React Router)

```jsx
// App.js o Router.js
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { useAuthStore } from './store/authStore';
import PropiedadesAdminPanel from './components/admin/PropiedadesAdminPanel';
import ProtectedRoute from './components/ProtectedRoute';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route 
          path="/admin/propiedades" 
          element={
            <ProtectedRoute requiredPermission="view_dashboard">
              <PropiedadesAdminPanel />
            </ProtectedRoute>
          } 
        />
        {/* Otras rutas */}
      </Routes>
    </Router>
  );
}

// ProtectedRoute.jsx
import { useAuthStore } from '../store/authStore';
import { Navigate } from 'react-router-dom';

const ProtectedRoute = ({ children, requiredPermission }) => {
  const { isAuthenticated, hasPermission } = useAuthStore();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (requiredPermission && !hasPermission(requiredPermission)) {
    return <Navigate to="/unauthorized" replace />;
  }

  return children;
};

export default ProtectedRoute;
```

### 5. Configurar variables de entorno

```env
# .env.local
NEXT_PUBLIC_API_URL=http://localhost:5174/api
NEXT_PUBLIC_API_BASE_URL=http://localhost:5174

# Para producción
# NEXT_PUBLIC_API_URL=https://tu-api.com/api
# NEXT_PUBLIC_API_BASE_URL=https://tu-api.com
```

### 6. Inicializar el store al cargar la app

```jsx
// _app.js (Next.js) o main.jsx/App.jsx
import { useEffect } from 'react';
import { useAuthStore } from '../store/authStore';
import { Toaster } from 'react-hot-toast';

function MyApp({ Component, pageProps }) {
  const { login } = useAuthStore();

  useEffect(() => {
    // Verificar si hay una sesión guardada
    const token = localStorage.getItem('access_token');
    const user = localStorage.getItem('user');
    const role = localStorage.getItem('role');

    if (token && user && role) {
      login(JSON.parse(user), role);
    }
  }, [login]);

  return (
    <>
      <Component {...pageProps} />
      <Toaster 
        position="top-right"
        toastOptions={{
          duration: 4000,
          style: {
            background: '#363636',
            color: '#fff',
          },
        }}
      />
    </>
  );
}

export default MyApp;
```

## 🔧 Pasos de Integración

1. **Instalar dependencias**:
   ```bash
   npm install @heroicons/react react-hot-toast react-hook-form react-dropzone zod zustand
   ```

2. **Copiar archivos del módulo** a tu proyecto

3. **Actualizar el panel de administración** con la navegación

4. **Configurar rutas** según tu router

5. **Verificar permisos** en authStore

6. **Configurar variables de entorno**

7. **Probar funcionalidad** completa

## 🎯 Verificación de Funcionalidad

### Lista de verificación:
- [ ] Login con diferentes roles
- [ ] Navegación entre módulos
- [ ] Crear nueva propiedad
- [ ] Editar propiedad existente
- [ ] Subir imágenes locales
- [ ] Agregar URLs externas
- [ ] Eliminar propiedades (solo Admin)
- [ ] Filtros y búsqueda
- [ ] Paginación
- [ ] Vista detalle
- [ ] Responsive design

### Casos de prueba:
1. **Usuario sin permisos**: Debe mostrar mensaje de acceso denegado
2. **Cargador**: Solo puede subir media y editar propiedades
3. **Agente**: Puede gestionar propiedades pero no eliminar
4. **Admin**: Acceso completo a todas las funciones

## 🚀 Despliegue

### Frontend
```bash
npm run build
npm start
```

### Backend
```bash
dotnet publish -c Release
```

Asegúrate de configurar las URLs correctas en producción y verificar que los permisos CORS estén configurados apropiadamente.

