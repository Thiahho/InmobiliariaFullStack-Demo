import React, { useState } from "react";
import Link from "next/link";
import PropiedadesPublic from "./propiedades/PropiedadesPublic";
import PropiedadDetail from "./propiedades/PropiedadDetail";
import {
  ArrowLeftIcon,
  HomeIcon,
  MagnifyingGlassIcon,
} from "@heroicons/react/24/outline";

type View = "list" | "detail";

export interface Propiedad {
  id: number | string;
  codigo: string;
  [key: string]: unknown;
}

const Propiedades: React.FC = () => {
  const [currentView, setCurrentView] = useState<View>("list");
  const [selectedPropiedad, setSelectedPropiedad] = useState<Propiedad | null>(
    null
  );

  const handleView = (propiedad: Propiedad): void => {
    setSelectedPropiedad(propiedad);
    setCurrentView("detail");
  };

  const handleBackToList = (): void => {
    setSelectedPropiedad(null);
    setCurrentView("list");
  };

  const getPageTitle = (): string => {
    switch (currentView) {
      case "detail":
        return "Detalle de Propiedad";
      default:
        return "Catálogo de Propiedades";
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Navbar adaptado para páginas internas */}
      <nav className="bg-white shadow-sm border-b border-gray-200 relative z-40">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            {/* Logo */}
            <div className="flex-shrink-0">
              <Link href="/" className="flex items-center">
                <img src="/logo.png" alt="Logo" className="w-32 h-auto" onError={(e) => {
                  e.target.style.display = 'none';
                  e.target.nextSibling.style.display = 'block';
                }} />
                <span className="ml-2 text-xl font-bold text-gray-900 hidden">Inmobiliaria</span>
              </Link>
            </div>

            {/* Navigation Links */}
            <div className="hidden md:block">
              <div className="ml-10 flex items-baseline space-x-4">
                <Link href="/" className="text-gray-600 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium">
                  Inicio
                </Link>
                <Link href="/propiedades" className="text-blue-600 bg-blue-50 px-3 py-2 rounded-md text-sm font-medium">
                  Propiedades
                </Link>
                <a href="/#about" className="text-gray-600 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium">
                  Nosotros
                </a>
                <a href="/#projects" className="text-gray-600 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium">
                  Destacadas
                </a>
              </div>
            </div>

            {/* Right side buttons */}
            <div className="hidden md:flex items-center space-x-4">
              <Link href="/admin" className="text-gray-600 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium">
                Panel Admin
              </Link>
            </div>

            {/* Mobile menu button */}
            <div className="md:hidden">
              <button
                type="button"
                className="inline-flex items-center justify-center p-2 rounded-md text-gray-600 hover:text-gray-900 hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-blue-500"
                aria-controls="mobile-menu"
                aria-expanded="false"
              >
                <span className="sr-only">Abrir menú principal</span>
                <svg className="block h-6 w-6" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden="true">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M4 6h16M4 12h16M4 18h16" />
                </svg>
              </button>
            </div>
          </div>
        </div>
      </nav>

      {/* Header */}
      <div className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-6">
            <div className="flex items-center space-x-4">
              {currentView !== "list" && (
                <button
                  onClick={handleBackToList}
                  className="inline-flex items-center p-2 border border-transparent text-sm leading-4 font-medium rounded-md text-gray-500 hover:text-gray-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                  type="button"
                >
                  <ArrowLeftIcon className="h-5 w-5 mr-2" />
                  Volver al Catálogo
                </button>
              )}

              <div>
                <h1 className="text-2xl font-bold text-gray-900">
                  {getPageTitle()}
                </h1>
                <p className="mt-1 text-sm text-gray-600">
                  {currentView === "list"
                    ? "Explora nuestro catálogo de propiedades disponibles"
                    : `Detalles de la propiedad ${selectedPropiedad?.codigo}`}
                </p>
              </div>
            </div>

            {/* Información para clientes */}
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {currentView === "list" && <PropiedadesPublic onView={handleView} />}
      </div>

      {/* Modal de detalle */}
      {currentView === "detail" && selectedPropiedad && (
        <PropiedadDetail
          propiedadId={selectedPropiedad.id}
          onClose={handleBackToList}
        />
      )}

      {/* Panel de ayuda para clientes */}
      {currentView === "list" && (
        <div className="fixed bottom-4 right-4 max-w-sm z-40">
          <div className="bg-green-50 border border-green-200 rounded-lg p-4 shadow-lg">
            <div className="flex items-start">
              <div className="flex-shrink-0">
                <MagnifyingGlassIcon className="h-5 w-5 text-green-600" />
              </div>
              <div className="ml-3">
                <h3 className="text-sm font-medium text-green-800">
                  Buscar Propiedades
                </h3>
                <p className="mt-1 text-sm text-green-700">
                  Usa los filtros para refinar tu búsqueda. Todas las
                  propiedades mostradas están disponibles para la venta o
                  alquiler.
                </p>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Propiedades;
