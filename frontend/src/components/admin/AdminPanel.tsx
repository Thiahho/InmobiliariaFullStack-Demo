// import React, { useState } from 'react';

// export default function AdminPanel() {
//   const [isOpen, setIsOpen] = useState(false);

//   return (
//     <>
//       <button 
//         onClick={() => setIsOpen(true)}
//         className="fixed bottom-4 right-4 bg-green-500 text-white px-4 py-2 rounded-lg hover:bg-green-600 z-50"
//       >
//         Admini
//       </button>
      
//       {isOpen && (
//         <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
//           <div className="bg-white p-8 rounded-lg max-w-4xl w-full mx-4 max-h-[90vh] overflow-y-auto">
//             <div className="flex justify-between items-center mb-6">
//               <h2 className="text-2xl font-bold">Panel de Administración...</h2>
//               <button 
//                 onClick={() => setIsOpen(false)}
//                 className="text-gray-500 hover:text-gray-700"
//               >
//                 ✕
//               </button>
//             </div>
//             <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
//               <div className="bg-blue-50 p-6 rounded-lg">
//                 <h3 className="font-bold text-lg mb-2">Propiedades</h3>
//                 <p className="text-gray-600 mb-4">Gestionar propiedades</p>
//                 <button className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">
//                   Administrar
//                 </button>
//               </div>
//               <div className="bg-green-50 p-6 rounded-lg">
//                 <h3 className="font-bold text-lg mb-2">Agentes</h3>
//                 <p className="text-gray-600 mb-4">Gestionar agentes</p>
//                 <button className="bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600">
//                   Administrar
//                 </button>
//               </div>
//               <div className="bg-yellow-50 p-6 rounded-lg">
//                 <h3 className="font-bold text-lg mb-2">Leads</h3>
//                 <p className="text-gray-600 mb-4">Gestionar leads</p>
//                 <button className="bg-yellow-500 text-white px-4 py-2 rounded hover:bg-yellow-600">
//                   Administrar
//                 </button>
//               </div>
//             </div>
//           </div>
//         </div>
//       )}
//     </>
//   );
// }