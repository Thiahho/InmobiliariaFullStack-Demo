"use client";
import React, { useEffect } from "react";
import { assets } from "../assets/assets/assets";
import Login from "./auth/Login";

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
    <div className="absolute top-0 left-0 w-full z-30">
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
        <Login />
        <button
          type="button"
          aria-label="Abrir menú"
          onClick={() => setMobileMenu(true)}
          className="md:hidden inline-flex items-center justify-center w-10 h-10 rounded-md border border-white/30 bg-white/10 backdrop-blur text-white hover:bg-white/20 transition"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            fill="currentColor"
            className="w-6 h-6"
          >
            <path d="M3 6.75A.75.75 0 0 1 3.75 6h16.5a.75.75 0 0 1 0 1.5H3.75A.75.75 0 0 1 3 6.75zm0 5.25a.75.75 0 0 1 .75-.75h16.5a.75.75 0 0 1 0 1.5H3.75A.75.75 0 0 1 3 12zm.75 4.5a.75.75 0 0 0 0 1.5h16.5a.75.75 0 0 0 0-1.5H3.75z" />
          </svg>
        </button>
      </div>
      {/* ------- mobile menu -------- */}
      <div
        className={`md:hidden ${
          showMobileMenu ? "fixed w-full h-full" : "h-0 w-0"
        } right-0 top-0 z-40 overflow-hidden bg-white transition-all duration-300`}
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
