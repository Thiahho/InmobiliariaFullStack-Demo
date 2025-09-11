import React from "react";

const CargadorPanel = () => {
  return (
    <section className="py-16">
      <div className="container mx-auto px-4">
        <h2 className="text-2xl font-semibold mb-6">Panel de Cargador</h2>
        <div className="rounded-lg border bg-white p-5 shadow-sm">
          <h3 className="font-semibold mb-2">Subidas recientes</h3>
          <ul className="text-sm text-gray-700 space-y-2">
            <li>prop_123 • 6 imágenes • OK</li>
            <li>prop_221 • 2 imágenes • Error (peso)</li>
            <li>prop_431 • 10 imágenes • OK</li>
          </ul>
        </div>
      </div>
    </section>
  );
};

export default CargadorPanel;


