import React from "react";
import { assets } from "../assets/assets/assets";

const Footer = () => {
  return (
    <div
      className="pt-10 px-4 md:px-20 lg:px-32 bg-gray-900 w-full overflow-hidden"
      id="Footer"
    >
      <div className="container mx-auto flex flex-col md:flex-row justify-between items-start">
        <div className="w-full md:w-1/3 mb-8 md:mb-0">
          <img src={assets.logo_dark} alt="" />
          <p className="text-gray-400 mt-4">
            Lorem, ipsum dolor sit amet consectetur adipisicing elit. Esse
            veritatis tempore iste consequatur voluptas voluptate sapiente.
            Autem facere voluptas vero ipsum quo eveniet cupiditate porro?
            Temporibus quia officiis officia nisi?
          </p>
        </div>
        <div className="w-full md:w-1/5 mb-8 md:mb-0">
          <h3 className="text-white text-lg font-bold">Compañia</h3>
          <ul className="flex flex-col gap-3 text-gray-400">
            <a href="#Header" className="hover:text-white">
              Inicio
            </a>
            <a href="#About" className="hover:text-white">
              Nosotros
            </a>
            <a href="#Contact" className="hover:text-white">
              Contactanos
            </a>
            <a href="#" className="hover:text-white">
              Privacy policy
            </a>
          </ul>
        </div>
        <div className="w-full md:w-1/3">
          <h3 className="text-white text-lg font-bold mb-4">
            Subscribite para recibir novedades
          </h3>
          <p className="text-gray-400 mb-4 max-w-80">
            Nuestras ultimas noticias, articulos, y novedades, te lo enviamos a
            tu inbox semanalmente.
          </p>
          <div className="flex gap-2">
            <input
              type="email"
              name="email"
              placeholder="Ingrese su correo"
              className="p-2 rounded bg-gray-800 text-gray-400 border border-gray-700 focus:outline-none w-full md:w-auto"
              id=""
            />
            <button className="py-2 px-4 rounded bg-blue-500 text-white">
              Subscribite
            </button>
          </div>
        </div>
      </div>
      <div className="border border-gray-700 py-4 mt-10 text-center text-gray-500">
        Copyright 2025 TEXE. All Right Reserved.
      </div>
    </div>
  );
};

export default Footer;
