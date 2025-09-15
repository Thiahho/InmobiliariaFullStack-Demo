import React from "react";

interface Propiedad {
  id: number;
  titulo: string;
  precio: number;
  direccion: string;
  imagen: string;
  ambientes: number;
  metros: number;
}

const mockPropiedades: Propiedad[] = [
  {
    id: 1,
    titulo: "Departamento en Palermo",
    precio: 185000,
    direccion: "Av. Santa Fe 1234, CABA",
    imagen: "/project_img_1.jpg",
    ambientes: 3,
    metros: 75,
  },
  {
    id: 2,
    titulo: "Casa en Nordelta",
    precio: 430000,
    direccion: "Los Castores, Tigre",
    imagen: "/project_img_2.jpg",
    ambientes: 5,
    metros: 210,
  },
  {
    id: 3,
    titulo: "Monoambiente Belgrano",
    precio: 89000,
    direccion: "Monroe 2500, CABA",
    imagen: "/project_img_3.jpg",
    ambientes: 1,
    metros: 28,
  },
];

interface PropCardProps {
  p: Propiedad;
}

const PropCard: React.FC<PropCardProps> = ({ p }) => (
  <div className="rounded-lg border border-gray-200 overflow-hidden bg-white shadow-sm">
    <div className="h-40 w-full bg-gray-100">
      <img
        src={p.imagen}
        alt={p.titulo}
        className="h-full w-full object-cover"
      />
    </div>
    <div className="p-4 space-y-2">
      <h3 className="text-lg font-semibold">{p.titulo}</h3>
      <p className="text-sm text-gray-500">{p.direccion}</p>
      <div className="flex items-center justify-between">
        <span className="text-inmobiliaria-700 font-bold">
          ${p.precio.toLocaleString("es-AR")}
        </span>
        <span className="text-sm text-gray-600">
          {p.ambientes} amb • {p.metros} m²
        </span>
      </div>
    </div>
  </div>
);

const PropiedadesGrid: React.FC = () => {
  return (
    <section className="py-16 bg-gray-50">
      <div className="container mx-auto px-4">
        <div className="mb-8 flex items-end justify-between">
          <div>
            <h2 className="text-2xl font-semibold">Propiedades destacadas</h2>
            <p className="text-gray-600">Explora oportunidades seleccionadas.</p>
          </div>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {mockPropiedades.map((p) => (
            <PropCard key={p.id} p={p} />
          ))}
        </div>
      </div>
    </section>
  );
};

export default PropiedadesGrid;