import React from "react";
import { toast } from "react-toastify";
import { motion } from "framer-motion";

const Contact = () => {
  const [result, setResult] = React.useState("");

  const onSubmit = async (event) => {
    event.preventDefault();
    setResult("Sending....");
    const formData = new FormData(event.target);

    formData.append("access_key", "c108c89a-91eb-47ba-832f-0ab8f0b4fb84");

    try {
      const response = await fetch("https://api.web3forms.com/submit", {
        method: "POST",
        body: formData,
      });

      const data = await response.json();

      if (data.success) {
        setResult("");
        toast.success("Mensaje Enviado Exitosamente!");
        event.target.reset();
      } else {
        console.log("Error", data);
        toast.error(data.message || "Error al enviar el mensaje");
        setResult("");
      }
    } catch (error) {
      console.error("Network error:", error);
      toast.error("Error de conexión. Intenta de nuevo.");
      setResult("");
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 100 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 1.5 }}
      whileInView={{ opacity: 1, y: 0 }}
      viewport={{ once: true }}
      className="text-center p-6 py-20 lg:px-32 w-full overflow-hidden"
      id="Contact"
    >
      <h1 className="text-2xl sm:text-4xl font-bold mb-2 text-center">
        Contacta{" "}
        <span className="underline underline-offset-4 decoration-1 under font-light">
          con Nosotros
        </span>
      </h1>
      <p className="text-center text-gray-500 mb-12 max-w-80 mx-auto">
        ¿Listo para dar el salto? Construyamos juntos tu futuro.
      </p>

      <form
        onSubmit={onSubmit}
        className="max-w-2xl mx-auto text-gray-600 pt-8"
      >
        <div className="flex flex-wrap">
          <div className="w-full md:w-1/2 text-left">
            Tú Nombre
            <input
              className="w-full border border-gray-300 rounded py-3 px-4 mt-2"
              type="text"
              placeholder="Tú Nombre"
              required
              name="Nombre"
              id=""
            />
          </div>
          <div className="w-full md:w-1/2 text-left md:pl-4">
            Email
            <input
              className="w-full border border-gray-300 rounded py-3 px-4 mt-2"
              type="email"
              placeholder="Tú Email"
              required
              name="Email"
              id=""
            />
          </div>
        </div>
        <div className="my-6 text-left">
          Mensaje
          <textarea
            name="Mensaje"
            placeholder="Mensaje"
            required
            className="w-full border border-gray-300 rounded py-3 px-4 mt-2 h-48 resize-none"
          ></textarea>
        </div>
        <button className="bg-blue-600 text-white rounded py-2 px-12 mb-10">
          {result ? result : "Enviar Mensaje"}
        </button>
      </form>
    </motion.div>
  );
};

export default Contact;
