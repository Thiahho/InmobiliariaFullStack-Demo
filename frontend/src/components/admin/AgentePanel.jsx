import React from "react";

const AgentePanel = () => {
  return (
    <section className="py-16">
      <div className="container mx-auto px-4">
        <h2 className="text-2xl font-semibold mb-6">Panel de Agente</h2>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="rounded-lg border bg-white p-5 shadow-sm">
            <h3 className="font-semibold mb-2">Mis propiedades</h3>
            <ul className="text-sm text-gray-700 space-y-2">
              <li>#123 • Palermo • Publicada</li>
              <li>#221 • Nordelta • Borrador</li>
              <li>#431 • Belgrano • En revisión</li>
            </ul>
          </div>
          <div className="rounded-lg border bg-white p-5 shadow-sm">
            <h3 className="font-semibold mb-2">Mis visitas</h3>
            <ul className="text-sm text-gray-700 space-y-2">
              <li>11/09 • 15:00 • Nordelta</li>
              <li>12/09 • 11:30 • Palermo</li>
              <li>12/09 • 17:45 • Belgrano</li>
            </ul>
          </div>
        </div>
      </div>
    </section>
  );
};

export default AgentePanel;


