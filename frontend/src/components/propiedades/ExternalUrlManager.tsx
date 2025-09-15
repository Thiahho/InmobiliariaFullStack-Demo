import React, { useState } from "react";
import { usePropiedadesStore } from "../../store/propiedadesStore";
import { toast } from "react-hot-toast";
import {
  LinkIcon,
  VideoCameraIcon,
  PhotoIcon,
  GlobeAltIcon,
  CheckCircleIcon,
  XCircleIcon,
  PlusIcon,
} from "@heroicons/react/24/outline";

// ==== Tipos ====
type ID = string | number;
type TipoMedia = "image" | "video" | "tour";

type ValidationResult = {
  valid: boolean;
  message: string;
  tipo?: TipoMedia;
  processedUrl?: string;
};

type UrlData = {
  url: string;
  titulo: string;
  tipo: TipoMedia;
  orden: number;
};

type StoreShape = {
  addExternalUrl: (propiedadId: ID, data: UrlData) => Promise<void>;
  validateUrl: (url: string) => Promise<ValidationResult>;
  loading: boolean;
};

type Props = {
  propiedadId: ID | null | undefined;
};

const ExternalUrlManager: React.FC<Props> = ({ propiedadId }) => {
  const { addExternalUrl, validateUrl, loading } = usePropiedadesStore() as unknown as StoreShape;

  const [newUrl, setNewUrl] = useState<string>("");
  const [newTitulo, setNewTitulo] = useState<string>("");
  const [selectedTipo, setSelectedTipo] = useState<TipoMedia>("image");
  const [validationResult, setValidationResult] = useState<ValidationResult | null>(null);
  const [isValidating, setIsValidating] = useState<boolean>(false);

  const tiposDisponibles: Array<{ value: TipoMedia; label: string; icon: React.ComponentType<any> }> =
    [
      { value: "image", label: "Imagen", icon: PhotoIcon },
      { value: "video", label: "Video", icon: VideoCameraIcon },
      { value: "tour", label: "Tour Virtual", icon: GlobeAltIcon },
    ];

  const urlExamples: Record<TipoMedia, string[]> = {
    image: [
      "https://drive.google.com/file/d/ID_DEL_ARCHIVO/view",
      "https://imgur.com/imagen.jpg",
      "https://example.com/imagen.png",
    ],
    video: [
      "https://www.youtube.com/watch?v=VIDEO_ID",
      "https://youtu.be/VIDEO_ID",
      "https://vimeo.com/VIDEO_ID",
      "https://drive.google.com/file/d/ID_DEL_VIDEO/view",
    ],
    tour: ["https://matterport.com/tour/ID_DEL_TOUR", "https://example.com/tour-360"],
  };

  const handleValidateUrl = async () => {
    if (!newUrl) {
      toast.error("Ingrese una URL");
      return;
    }
    setIsValidating(true);
    try {
      const result = await validateUrl(newUrl);
      setValidationResult(result);

      if (result.valid) {
        if (result.tipo && result.tipo !== selectedTipo) setSelectedTipo(result.tipo);
        if (!newTitulo && (result.processedUrl || newUrl)) {
          const src = result.processedUrl ?? newUrl;
          setNewTitulo(generateAutoTitle(src, result.tipo ?? selectedTipo));
        }
      }
    } catch (err) {
      // eslint-disable-next-line no-console
      console.error("Error al validar URL:", err);
      setValidationResult({ valid: false, message: "Error al validar URL" });
    } finally {
      setIsValidating(false);
    }
  };

  const generateAutoTitle = (url: string, tipo: TipoMedia): string => {
    try {
      if (url.includes("youtube.com") || url.includes("youtu.be")) return "Video de YouTube";
      if (url.includes("vimeo.com")) return "Video de Vimeo";
      if (url.includes("drive.google.com")) return `Archivo de Google Drive (${tipo})`;
      if (url.includes("matterport.com")) return "Tour Virtual 3D";

      const urlObj = new URL(url);
      const parts = urlObj.pathname.split("/");
      const fileName = parts[parts.length - 1];
      if (fileName && fileName.includes(".")) return fileName.split(".")[0];

      return `${tipo.charAt(0).toUpperCase() + tipo.slice(1)} externa`;
    } catch {
      return `${tipo.charAt(0).toUpperCase() + tipo.slice(1)} externa`;
    }
  };

  const handleAddUrl = async () => {
    if (!newUrl) {
      toast.error("Ingrese una URL");
      return;
    }
    if (!propiedadId) {
      toast.error("Primero debe guardar la propiedad");
      return;
    }

    try {
      const urlData: UrlData = {
        url: newUrl,
        titulo: newTitulo || generateAutoTitle(newUrl, selectedTipo),
        tipo: selectedTipo,
        orden: 0,
      };
      await addExternalUrl(propiedadId, urlData);

      setNewUrl("");
      setNewTitulo("");
      setSelectedTipo("image");
      setValidationResult(null);

      toast.success("URL externa agregada exitosamente");
    } catch (err) {
      // eslint-disable-next-line no-console
      console.error("Error al agregar URL:", err);
    }
  };

  const handleUrlChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setNewUrl(e.target.value);
    setValidationResult(null);
  };

  const isValidUrl = (url: string): boolean => {
    try {
      new URL(url);
      return true;
    } catch {
      return false;
    }
  };

  return (
    <div className="space-y-6">
      <div className="bg-gray-50 rounded-lg p-6">
        <h4 className="text-lg font-medium text-gray-900 mb-4">Agregar URL Externa</h4>

        <div className="space-y-4">
          {/* Tipo */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Tipo de contenido</label>
            <div className="grid grid-cols-3 gap-3">
              {tiposDisponibles.map((tipo) => {
                const Icon = tipo.icon;
                return (
                  <button
                    key={tipo.value}
                    type="button"
                    onClick={() => setSelectedTipo(tipo.value)}
                    className={`flex items-center justify-center p-3 rounded-lg border-2 transition-colors ${
                      selectedTipo === tipo.value
                        ? "border-blue-500 bg-blue-50 text-blue-700"
                        : "border-gray-200 hover:border-gray-300 text-gray-600"
                    }`}
                  >
                    <Icon className="h-5 w-5 mr-2" />
                    <span className="text-sm font-medium">{tipo.label}</span>
                  </button>
                );
              })}
            </div>
          </div>

          {/* URL */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">URL del archivo</label>
            <div className="flex space-x-2">
              <div className="flex-1 relative">
                <input
                  type="url"
                  value={newUrl}
                  onChange={handleUrlChange}
                  placeholder={`Pegue aquí la URL de ${tiposDisponibles
                    .find((t) => t.value === selectedTipo)
                    ?.label.toLowerCase()}`}
                  className="w-full pl-9 px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
                />
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <LinkIcon className="h-5 w-5 text-gray-400" />
                </div>
              </div>
              <button
                type="button"
                onClick={handleValidateUrl}
                disabled={!newUrl || isValidating}
                className="px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isValidating ? "Validando..." : "Validar"}
              </button>
            </div>
          </div>

          {/* Resultado validación */}
          {validationResult && (
            <div
              className={`flex items-center p-3 rounded-md ${
                validationResult.valid
                  ? "bg-green-50 text-green-800 border border-green-200"
                  : "bg-red-50 text-red-800 border border-red-200"
              }`}
            >
              {validationResult.valid ? (
                <CheckCircleIcon className="h-5 w-5 mr-2" />
              ) : (
                <XCircleIcon className="h-5 w-5 mr-2" />
              )}
              <span className="text-sm">{validationResult.message}</span>
            </div>
          )}

          {/* Título */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Título (opcional)</label>
            <input
              type="text"
              value={newTitulo}
              onChange={(e) => setNewTitulo(e.target.value)}
              placeholder="Título descriptivo del contenido"
              className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          {/* Agregar */}
          <button
            type="button"
            onClick={handleAddUrl}
            disabled={!newUrl || !isValidUrl(newUrl) || loading}
            className="w-full flex items-center justify-center px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <PlusIcon className="h-5 w-5 mr-2" />
            {loading ? "Agregando..." : "Agregar URL Externa"}
          </button>
        </div>
      </div>

      {/* Ejemplos */}
      <div className="bg-blue-50 rounded-lg p-4">
        <h5 className="font-medium text-blue-900 mb-2">
          Ejemplos de URLs para {tiposDisponibles.find((t) => t.value === selectedTipo)?.label}:
        </h5>
        <ul className="space-y-1">
          {urlExamples[selectedTipo]?.map((example, index) => (
            <li key={index} className="text-sm text-blue-700 font-mono">
              {example}
            </li>
          ))}
        </ul>
      </div>

      {/* Notas */}
      <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
        <h5 className="font-medium text-yellow-800 mb-2">Notas importantes:</h5>
        <ul className="text-sm text-yellow-700 space-y-1">
          <li>• Google Drive: el archivo debe ser público o con permiso de visualización.</li>
          <li>• YouTube: URLs normales o acortadas (youtu.be) funcionan.</li>
          <li>• Las URLs se procesan automáticamente para optimizar la visualización.</li>
          <li>• El sistema detectará el tipo cuando sea posible.</li>
        </ul>
      </div>
    </div>
  );
};

export default ExternalUrlManager;
