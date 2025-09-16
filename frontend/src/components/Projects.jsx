"use client";
import React, { useEffect, useState } from "react";
import { assets } from "../assets/assets/assets";
import { motion } from "framer-motion";

const Projects = () => {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [cardsToShow, setCardsToShow] = useState(1);
  const [projectsData, setProjectsData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Fetch projects from backend
  useEffect(() => {
    const fetchProjects = async () => {
      try {
        setLoading(true);
        const response = await fetch("http://localhost:5174/api/propiedades");
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        setProjectsData(data);
        setError(null);
      } catch (err) {
        setError(err.message);
        console.error("Error fetching projects:", err);
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
        Proyectos{" "}
        <span className="underline underline-offset-4 decoration-1 under font-light">
          Completos
        </span>
      </h1>
      <p className="text-center text-gray-500 mb-8 max-w-80 mx-auto">
        Creando espacios, construyendo legados: explora nuestro portafolio
      </p>

      {/* Loading state */}
      {loading && (
        <div className="text-center py-8">
          <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
          <p className="mt-2 text-gray-600">Cargando propiedades...</p>
        </div>
      )}

      {/* Error state */}
      {error && (
        <div className="text-center py-8">
          <p className="text-red-600 mb-4">
            Error al cargar las propiedades: {error}
          </p>
          <button
            onClick={() => window.location.reload()}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            Reintentar
          </button>
        </div>
      )}

      {/* Content - only show if not loading and no error */}
      {!loading && !error && (
        <>
          {/* slider buttons */}
          <div className="flex justify-end items-center mb-8">
            <button
              onClick={prevProject}
              className="p-3 bg-gray-200 rounded-full mr-2 hover:bg-gray-300"
              aria-label="Previous Project"
            >
              <img src={assets.left_arrow} alt="Previous" />
            </button>
            <button
              onClick={nextProject}
              className="p-3 bg-gray-200 rounded-full mr-2 hover:bg-gray-300"
              aria-label="Next Project"
            >
              <img src={assets.right_arrow} alt="Next" />
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
                        const mediaUrl = project.medias.find((media) => media.esPrincipal)?.url || project.medias[0]?.url;
                        return mediaUrl.startsWith('http') ? mediaUrl : `http://localhost:5174${mediaUrl}`;
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
                      <p className="text-gray-500 text-sm">
                        {project.moneda} {project.precio?.toLocaleString()}{" "}
                        <span className="px-1">|</span> {project.barrio},{" "}
                        {project.comuna}
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </>
      )}
    </motion.div>
  );
};

export default Projects;
