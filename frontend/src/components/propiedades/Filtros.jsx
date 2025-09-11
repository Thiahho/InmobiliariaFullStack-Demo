import React from "react";
import { useForm } from "react-hook-form";

const Filtros = () => {
  const { register, handleSubmit, reset } = useForm({
    defaultValues: {
      q: "",
      tipo: "",
      minPrecio: "",
      maxPrecio: "",
      ambientes: "",
    },
  });

  const onSubmit = (data) => {
    // Estático: solo mostramos por consola
    // eslint-disable-next-line no-console
    console.log("filtros", data);
  };

  return (
    <section className="bg-white border-y">
      <form
        onSubmit={handleSubmit(onSubmit)}
        className="container mx-auto px-4 py-4 grid grid-cols-2 md:grid-cols-6 gap-3"
      >
        <input
          className="col-span-2 md:col-span-2 rounded-md border px-3 py-2"
          placeholder="Buscar por ubicación o título"
          {...register("q")}
        />
        <select className="rounded-md border px-3 py-2" {...register("tipo")}>
          <option value="">Tipo</option>
          <option value="departamento">Departamento</option>
          <option value="casa">Casa</option>
          <option value="monoambiente">Monoambiente</option>
        </select>
        <input
          className="rounded-md border px-3 py-2"
          placeholder="Precio min"
          type="number"
          {...register("minPrecio")}
        />
        <input
          className="rounded-md border px-3 py-2"
          placeholder="Precio max"
          type="number"
          {...register("maxPrecio")}
        />
        <select className="rounded-md border px-3 py-2" {...register("ambientes")}>
          <option value="">Ambientes</option>
          <option value="1">1</option>
          <option value="2">2</option>
          <option value="3">3</option>
          <option value="4">4+</option>
        </select>

        <div className="flex gap-2 col-span-2 md:col-span-1">
          <button
            type="button"
            onClick={() => reset()}
            className="w-full rounded-md border px-3 py-2"
          >
            Limpiar
          </button>
          <button
            type="submit"
            className="w-full rounded-md bg-inmobiliaria-600 text-white px-3 py-2"
          >
            Buscar
          </button>
        </div>
      </form>
    </section>
  );
};

export default Filtros;


