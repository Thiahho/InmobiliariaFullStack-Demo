// import React from "react";
// import { useAuthStore, Roles } from "../../store/authStore";

// const RoleSwitcher = () => {
//   const { isAuthenticated, role, login, logout, setRole } = useAuthStore();

//   const handleLoginAs = (r) => {
//     login({ name: "Demo" }, r);
//   };

//   return (
//     <div className="container mx-auto px-4 py-4 flex items-center gap-2 text-sm">
//       {!isAuthenticated ? (
//         <>
//           <span>Ingresar como:</span>
//           <button
//             className="rounded border px-3 py-1"
//             onClick={() => handleLoginAs(Roles.Admin)}
//           >
//             Admin
//           </button>
//           <button
//             className="rounded border px-3 py-1"
//             onClick={() => handleLoginAs(Roles.Agente)}
//           >
//             Agente
//           </button>
//           <button
//             className="rounded border px-3 py-1"
//             onClick={() => handleLoginAs(Roles.Cargador)}
//           >
//             Cargador
//           </button>
//         </>
//       ) : (
//         <>
//           <span className="text-gray-600">Rol actual: {role}</span>
//           <select
//             value={role || ""}
//             onChange={(e) => setRole(e.target.value)}
//             className="rounded border px-2 py-1"
//           >
//             <option value={Roles.Admin}>Admin</option>
//             <option value={Roles.Agente}>Agente</option>
//             <option value={Roles.Cargador}>Cargador</option>
//           </select>
//           <button className="rounded border px-3 py-1" onClick={logout}>
//             Salir
//           </button>
//         </>
//       )}
//     </div>
//   );
// };

// export default RoleSwitcher;


