import React from "react";
// import Navbar from "./components/Navbar";
import Header from "./components/Header";
import About from "./components/About";
import Projects from "./components/Projects";
import Testimonials from "./components/Testimonials";
import Contact from "./components/Contact";
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import Footer from "./components/Footer";
import Filtros from "./components/propiedades/Filtros";
import PropiedadesGrid from "./components/propiedades/PropiedadesGrid";
import Login from "./components/auth/Login.tsx";
import AdminPanel from "./components/admin/AdminPanel";
import AgentePanel from "./components/admin/AgentePanel";
import CargadorPanel from "./components/admin/CargadorPanel";
import RoleSwitcher from "./components/auth/RoleSwitcher";
import Protected from "./components/auth/Protected";
import { Roles } from "./store/authStore";
import UsersAdmin from "./components/users/UsersAdmin";
import UserProfile from "./components/users/UserProfile";
const App = () => {
  return (
    <div className="w-full overflow-hidden">
      <ToastContainer />
      <Header />
      <RoleSwitcher />
      <Login />
      <Filtros />
      <PropiedadesGrid />
      <About />
      <Projects />
      <Testimonials />
      <Contact />
      <Protected roles={[Roles.Admin]}>
        <AdminPanel />
        <UsersAdmin />
      </Protected>
      <Protected roles={[Roles.Agente]}>
        <AgentePanel />
      </Protected>
      <Protected roles={[Roles.Cargador]}>
        <CargadorPanel />
      </Protected>
      <Protected roles={[Roles.Admin, Roles.Agente, Roles.Cargador]}>
        <UserProfile />
      </Protected>
      <Footer />
    </div>
  );
};

export default App;
