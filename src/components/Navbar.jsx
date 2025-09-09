import React, { useEffect } from "react";
import { assets } from "../assets/assets/assets";

const Navbar = () => {
  const [showMobileMenu, setMobileMenu] = React.useState(false);
  useEffect(() => {
    if (showMobileMenu) {
      document.body.style.overflow = "hidden";
    } else {
      document.body.style.overflow = "auto";
    }
    return () => {
      document.body.style.overflow = "auto";
    };
  }, [showMobileMenu]);
  return (
    <div className="absolute top-0 left-0 w-full z-10">
      <div className="flex items-center justify-between px-6 py-4">
        <img src={assets.logo} alt="" className="w-36" />
        <ul className="hidden md:flex items-center gap-7 text-white">
          <li>
            <a href="#Header" className="cursor-pointer hover:text-gray-400">
              Inicio
            </a>
          </li>
          <li>
            <a href="#About" className="cursor-pointer hover:text-gray-400">
              Nosotros
            </a>
          </li>
          <li>
            <a href="#Projects" className="cursor-pointer hover:text-gray-400">
              Proyectos
            </a>
          </li>
          <li>
            <a
              href="#Testimonials"
              className="cursor-pointer hover:text-gray-400"
            >
              Testimonios
            </a>
          </li>
        </ul>
        <button className="hidden md:block bg-white px-8 py-2 rounded-full text-black">
          Iniciar Sesion
        </button>
        <img
          onClick={() => setMobileMenu(true)}
          src={assets.menu_icon}
          className="md:hidden w-7 cursor-pointer"
          alt=""
        />
      </div>
      {/* ------- mobile menu -------- */}
      <div
        className={`md:hidden ${
          showMobileMenu ? "fixed w-full h-full" : "h-0 w-0"
        } right-0 top-0 z-20 overflow-hidden bg-white transition-all duration-300`}
      >
        <ul className="flex flex-col items-center gap-2 mt-5 px-5 text-lg font-medium">
          <div className="flex justify-end p-6 w-full cursor-pointer">
            <img
              onClick={() => setMobileMenu(false)}
              src={assets.cross_icon}
              className="w-6"
              alt=""
            />
          </div>
          <a
            href="#Header"
            onClick={() => setMobileMenu(false)}
            className="px-4 py-2 rounded-full inline-block text-black hover:bg-gray-100"
          >
            Inicio
          </a>

          <a
            href="#About"
            onClick={() => setMobileMenu(false)}
            className="px-4 py-2 rounded-full inline-block text-black hover:bg-gray-100"
          >
            Nosotros
          </a>

          <a
            href="#Projects"
            onClick={() => setMobileMenu(false)}
            className="px-4 py-2 rounded-full inline-block text-black hover:bg-gray-100"
          >
            Proyectos
          </a>
          <a
            href="#Testimonials"
            onClick={() => setMobileMenu(false)}
            className="px-4 py-2 rounded-full inline-block text-black hover:bg-gray-100"
          >
            Testimonios
          </a>
        </ul>
      </div>
    </div>
  );
};

export default Navbar;
