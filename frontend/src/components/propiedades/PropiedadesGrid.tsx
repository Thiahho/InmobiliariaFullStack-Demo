"use client";
import React from 'react';
import { projectsData } from '../../assets/assets/assets';

export default function PropiedadesGrid() {
  return (
    <div className="container mx-auto py-16 px-6 md:px-20 lg:px-32">
      <h2 className="text-3xl font-bold text-center mb-10">
        Propiedades <span className="text-blue-500">Disponibles</span>
      </h2>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
        {projectsData.map((propiedad, index) => (
          <div key={index} className="bg-white rounded-lg shadow-lg overflow-hidden hover:shadow-xl transition-shadow">
            <img 
              src={typeof propiedad.image === 'string' ? propiedad.image : propiedad.image.src} 
              alt={propiedad.title}
              className="w-full h-48 object-cover"
            />
            <div className="p-6">
              <h3 className="text-xl font-semibold mb-2">{propiedad.title}</h3>
              <p className="text-gray-600 mb-2">{propiedad.location}</p>
              <p className="text-2xl font-bold text-blue-500 mb-4">{propiedad.price}</p>
              <button className="w-full bg-blue-500 text-white py-2 rounded-lg hover:bg-blue-600 transition-colors">
                Ver Detalles
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}