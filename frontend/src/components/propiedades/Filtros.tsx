import React from 'react';

export default function Filtros() {
  return (
    <div className="bg-gray-100 py-8">
      <div className="container mx-auto px-6 md:px-20 lg:px-32">
        <h2 className="text-2xl font-bold mb-6">Buscar Propiedades</h2>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <select className="p-3 border rounded-lg">
            <option>Tipo de Propiedad</option>
            <option>Casa</option>
            <option>Apartamento</option>
            <option>Local Comercial</option>
          </select>
          <select className="p-3 border rounded-lg">
            <option>Ubicación</option>
            <option>Ciudad</option>
            <option>Zona Norte</option>
            <option>Zona Sur</option>
          </select>
          <select className="p-3 border rounded-lg">
            <option>Precio</option>
            <option>$50,000 - $100,000</option>
            <option>$100,000 - $200,000</option>
            <option>$200,000+</option>
          </select>
          <button className="bg-blue-500 text-white p-3 rounded-lg hover:bg-blue-600">
            Buscar
          </button>
        </div>
      </div>
    </div>
  );
}