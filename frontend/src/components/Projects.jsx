"use client";
import React, { useEffect, useState } from "react";
import { assets } from "../assets/assets/assets";
import { motion } from "framer-motion";
import PropiedadDetail from "./propiedades/PropiedadDetail";

const Projects = () => {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [cardsToShow, setCardsToShow] = useState(1);
  const [projectsData, setProjectsData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedPropertyId, setSelectedPropertyId] = useState(null);

  // Fetch featured projects from backend
  useEffect(() => {
    const fetchProjects = async () => {
      try {
        setLoading(true);
        // Buscar solo propiedades destacadas usando búsqueda avanzada
        const response = await fetch(
          "http://localhost:5174/api/propiedades/buscar-avanzada",
          {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({
              destacado: true,
              estado: "Activo",
              page: 1,
              pageSize: 10, // Limitar a 10 propiedades destacadas
            }),
          }
        );

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        // Extraer el array de propiedades del resultado
        const data = result.data || result.Data || [];

        setProjectsData(data);
        setError(null);
      } catch (err) {
        setError(err.message);
        // Fallback: intentar con el endpoint simple y filtrar localmente
        try {
          const response = await fetch("http://localhost:5174/api/propiedades");
          if (response.ok) {
            const allData = await response.json();
            const featuredData = allData.filter(
              (prop) => prop.destacado === true && prop.estado === "Activo"
            );
            setProjectsData(featuredData);
            setError(null);
          }
        } catch (fallbackErr) {
          // Error en fallback, mantener el error original
        }
      } finally {
        setLoading(false);
      }
    };

    fetchProjects();
  }, []);

  // Update cards to show based on screen size and data length
  useEffect(() => {
    const updateCardsToShow = () => {
      if (window.innerWidth >= 1024) {
        setCardsToShow(projectsData.length);
      } else {
        setCardsToShow(1);
      }
    };
    updateCardsToShow();
    window.addEventListener("resize", updateCardsToShow);
    return () => {
      window.removeEventListener("resize", updateCardsToShow);
    };
  }, [projectsData.length]);
  const nextProject = () => {
    setCurrentIndex((prevIndex) => (prevIndex + 1) % projectsData.length);
  };
  const prevProject = () => {
    setCurrentIndex((prevIndex) =>
      prevIndex === 0 ? projectsData.length - 1 : prevIndex - 1
    );
  };

  return (
    <motion.div
      initial={{ opacity: 0, x: -200 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 1 }}
      whileInView={{ opacity: 1, x: 0 }}
      viewport={{ once: true }}
      className="container mx-auto p-14 py-4 pt-20 px-6 md:px-20 lg:px-32 w-full my-20 overflow-hidden"
      id="Projects"
    >
      <h1 className="text-2xl sm:text-4xl font-bold mb-2 text-center">
        Propiedades{" "}
        <span className="underline underline-offset-4 decoration-1 under font-light">
          Destacadas
        </span>
      </h1>
      <p className="text-center text-gray-500 mb-8 max-w-80 mx-auto">
        Descubre nuestras mejores propiedades seleccionadas especialmente para
        ti
      </p>

      {/* Loading state */}
      {loading && (
        <div className="text-center py-8">
          <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
          <p className="mt-2 text-gray-600">
            Cargando propiedades destacadas...
          </p>
        </div>
      )}

      {/* Error state */}
      {error && (
        <div className="text-center py-8">
          <p className="text-red-600 mb-4">
            Error al cargar las propiedades destacadas: {error}
          </p>
          <button
            onClick={() => window.location.reload()}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            Reintentar
          </button>
        </div>
      )}

      {/* No featured properties message */}
      {!loading && !error && projectsData.length === 0 && (
        <div className="text-center py-12">
          <div className="text-gray-400 mb-4">
            <svg
              className="mx-auto h-12 w-12"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"
              />
            </svg>
          </div>
          <p className="text-gray-600 text-lg mb-2">
            No hay propiedades destacadas disponibles
          </p>
          <p className="text-gray-500 text-sm">
            Pronto tendremos nuevas propiedades destacadas para mostrar
          </p>
        </div>
      )}

      {/* Content - only show if not loading and no error and has data */}
      {!loading && !error && projectsData.length > 0 && (
        <>
          {/* slider buttons */}
          <div className="flex justify-end items-center mb-8 gap-2">
            <button
              onClick={prevProject}
              className="w-10 h-10 flex items-center justify-center rounded-full bg-gray-400 text-white hover:bg-gray-500 transition-colors duration-200"
              aria-label="Previous Project"
            >
              ←
            </button>
            <button
              onClick={nextProject}
              className="w-10 h-10 flex items-center justify-center rounded-full bg-gray-400 text-white hover:bg-gray-500 transition-colors duration-200"
              aria-label="Next Project"
            >
              →
            </button>
          </div>

          {/* projject slider container */}
          <div className="overflow-hidden">
            <div
              className="flex gap-8 transition-transform duration-500 ease-in-out"
              style={{
                transform: `translateX(-${
                  currentIndex * (100 / cardsToShow)
                }%)`,
              }}
            >
              {projectsData.map((project, index) => (
                <div
                  key={project.id || index}
                  className="relative flex-shrink-0 w-full sm:w-1/4"
                >
                  <img
                    src={(() => {
                      if (project.medias && project.medias.length > 0) {
                        // Buscar la imagen principal o la primera disponible
                        const media = project.medias.find((m) => m.esPrincipal) || project.medias[0];

                        // Si la URL es externa (YouTube, Google Drive, etc.), usar directamente
                        if (media.url.startsWith("http://") || media.url.startsWith("https://")) {
                          return media.url;
                        }

                        // Si es una imagen almacenada en la BD, usar el endpoint /api/media/{id}/image
                        if (media.id) {
                          return `http://localhost:5174/api/media/${media.id}/image`;
                        }

                        // Fallback a la URL relativa (por compatibilidad)
                        return `http://localhost:5174${media.url}`;
                      }
                      return "/image.png";
                    })()}
                    alt={
                      project.titulo || `${project.tipo} en ${project.barrio}`
                    }
                    className="w-full h-auto mb-14"
                    onError={(e) => {
                      e.target.src = "/image.png";
                    }}
                  />
                  <div className="absolute bottom-5 left-0 right-0 flex justify-center">
                    <div className="inline-block bg-white w-3/4 px-4 py-2 shadow-md">
                      <h2 className="text-xl font-semibold text-gray-800">
                        {project.titulo ||
                          `${project.tipo} en ${project.barrio}`}
                      </h2>
                      <p className="text-gray-500 text-sm mb-3">
                        {project.moneda} {project.precio?.toLocaleString()}{" "}
                        <span className="px-1">|</span> {project.barrio},{" "}
                        {project.comuna}
                      </p>
                      <button
                        onClick={() => setSelectedPropertyId(project.id)}
                        className="w-full bg-blue-600 text-white py-2 px-4 rounded hover:bg-blue-700 transition-colors duration-200 text-sm font-medium"
                      >
                        Ver Propiedad
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </>
      )}

      {/* Componente PropiedadDetail */}
      {selectedPropertyId && (
        <PropiedadDetail
          propiedadId={selectedPropertyId}
          onClose={() => setSelectedPropertyId(null)}
        />
      )}
    </motion.div>
  );
};

export default Projects;
