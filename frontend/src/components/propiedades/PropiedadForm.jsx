import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { usePropiedadesStore } from '../../store/propiedadesStore';
import { toast } from 'react-hot-toast';
import MediaUploader from './MediaUploader';
import ExternalUrlManager from './ExternalUrlManager';

const PropiedadForm = ({ propiedadId = null, onSuccess = null }) => {
  const {
    createPropiedad,
    updatePropiedad,
    fetchPropiedadById,
    propiedadActual,
    loading
  } = usePropiedadesStore();

  const [activeTab, setActiveTab] = useState('datos');
  const [amenitiesSeleccionados, setAmenitiesSeleccionados] = useState({});

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    setValue,
    watch
  } = useForm({
    defaultValues: {
      codigo: '',
      tipo: '',
      operacion: 'Venta',
      barrio: '',
      comuna: '',
      direccion: '',
      latitud: '',
      longitud: '',
      moneda: 'USD',
      precio: '',
      expensas: '',
      ambientes: '',
      dormitorios: '',
      banos: '',
      cochera: false,
      metrosCubiertos: '',
      metrosTotales: '',
      antiguedad: '',
      piso: '',
      aptoCredito: false,
      estado: 'Activo',
      destacado: false,
      titulo: '',
      descripcion: ''
    }
  });

  const operacion = watch('operacion');
  const tipoPropiedad = watch('tipo');

  // Lista de amenities disponibles
  const amenitiesDisponibles = [
    'Piscina', 'Gimnasio', 'Seguridad 24hs', 'Ascensor', 'Terraza',
    'Balcón', 'Parrilla', 'Jardín', 'Cochera', 'Baulera',
    'Aire acondicionado', 'Calefacción', 'Portal', 'Portero',
    'SUM', 'Solarium', 'Vigilancia', 'Cámaras de seguridad'
  ];

  // Tipos de propiedades
  const tiposPropiedad = [
    'Departamento', 'Casa', 'PH', 'Local', 'Oficina', 'Galpon',
    'Terreno', 'Quinta', 'Chacra', 'Campo', 'Cochera'
  ];

  // Cargar datos si es edición
  useEffect(() => {
    if (propiedadId) {
      fetchPropiedadById(propiedadId).then((propiedad) => {
        if (propiedad) {
          Object.keys(propiedad).forEach(key => {
            if (key !== 'amenities') {
              setValue(key, propiedad[key] || '');
            }
          });
          
          // Manejar amenities
          if (propiedad.amenities) {
            setAmenitiesSeleccionados(propiedad.amenities);
          }
        }
      });
    }
  }, [propiedadId, fetchPropiedadById, setValue]);

  const handleAmenityChange = (amenity) => {
    setAmenitiesSeleccionados(prev => ({
      ...prev,
      [amenity]: !prev[amenity]
    }));
  };

  const onSubmit = async (data) => {
    try {
      // Tipos de propiedades que no requieren ciertos campos
      const tiposQueNoRequierenAmbientes = ['Terreno', 'Cochera', 'Galpon'];

      const propiedadData = {
        ...data,
        amenities: amenitiesSeleccionados,
        precio: parseFloat(data.precio) || 0,
        expensas: data.expensas ? parseFloat(data.expensas) : null,
        ambientes: tiposQueNoRequierenAmbientes.includes(data.tipo)
          ? null
          : (data.ambientes ? parseInt(data.ambientes) : null),
        dormitorios: data.dormitorios ? parseInt(data.dormitorios) : null,
        banos: data.banos ? parseInt(data.banos) : null,
        metrosCubiertos: data.metrosCubiertos ? parseInt(data.metrosCubiertos) : null,
        metrosTotales: data.metrosTotales ? parseInt(data.metrosTotales) : null,
        antiguedad: data.antiguedad ? parseInt(data.antiguedad) : null,
        piso: data.piso ? parseInt(data.piso) : null,
        latitud: data.latitud ? parseFloat(data.latitud) : null,
        longitud: data.longitud ? parseFloat(data.longitud) : null
      };

      if (propiedadId) {
        await updatePropiedad(propiedadId, propiedadData);
        toast.success('Propiedad actualizada exitosamente');
      } else {
        await createPropiedad(propiedadData);
        toast.success('Propiedad creada exitosamente');
        reset();
        setAmenitiesSeleccionados({});
      }

      if (onSuccess) {
        onSuccess();
      }
    } catch (error) {
      console.error('Error al guardar propiedad:', error);
    }
  };

  return (
    <div className="max-w-4xl mx-auto bg-white rounded-lg shadow-lg">
      <div className="border-b border-gray-200">
        <nav className="flex space-x-8 px-6">
          {[
            { id: 'datos', label: 'Datos Básicos' },
            { id: 'ubicacion', label: 'Ubicación' },
            { id: 'caracteristicas', label: 'Características' },
            { id: 'amenities', label: 'Amenities' },
            { id: 'media', label: 'Imágenes y Videos' }
          ].map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`py-4 px-1 border-b-2 font-medium text-sm ${
                activeTab === tab.id
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </nav>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-6">
        {/* Tab: Datos Básicos */}
        {activeTab === 'datos' && (
          <div className="space-y-6">
            <h3 className="text-lg font-medium text-gray-900">Información Básica</h3>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-medium text-gray-700">Código *</label>
                <input
                  type="text"
                  {...register('codigo', { required: 'El código es requerido' })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
                {errors.codigo && <p className="mt-1 text-sm text-red-600">{errors.codigo.message}</p>}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">Tipo de Propiedad *</label>
                <select
                  {...register('tipo', { required: 'El tipo es requerido' })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                >
                  <option value="">Seleccionar tipo</option>
                  {tiposPropiedad.map(tipo => (
                    <option key={tipo} value={tipo}>{tipo}</option>
                  ))}
                </select>
                {errors.tipo && <p className="mt-1 text-sm text-red-600">{errors.tipo.message}</p>}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">Operación *</label>
                <select
                  {...register('operacion', { required: 'La operación es requerida' })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                >
                  <option value="Venta">Venta</option>
                  <option value="Alquiler">Alquiler</option>
                </select>
                {errors.operacion && <p className="mt-1 text-sm text-red-600">{errors.operacion.message}</p>}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">Estado</label>
                <select
                  {...register('estado')}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                >
                  <option value="Activo">Activo</option>
                  <option value="Reservado">Reservado</option>
                  <option value="Vendido">Vendido</option>
                  <option value="Pausado">Pausado</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">Moneda</label>
                <select
                  {...register('moneda')}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                >
                  <option value="USD">USD</option>
                  <option value="ARS">ARS</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">Precio *</label>
                <input
                  type="number"
                  step="0.01"
                  {...register('precio', { required: 'El precio es requerido', min: 0 })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
                {errors.precio && <p className="mt-1 text-sm text-red-600">{errors.precio.message}</p>}
              </div>

              {operacion === 'Alquiler' && (
                <div>
                  <label className="block text-sm font-medium text-gray-700">Expensas</label>
                  <input
                    type="number"
                    step="0.01"
                    {...register('expensas', { min: 0 })}
                    className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                  />
                </div>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Título</label>
              <input
                type="text"
                {...register('titulo')}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                placeholder="Título descriptivo de la propiedad"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Descripción</label>
              <textarea
                rows={4}
                {...register('descripcion')}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                placeholder="Descripción detallada de la propiedad"
              />
            </div>

            <div className="flex items-center space-x-6">
              <label className="flex items-center">
                <input
                  type="checkbox"
                  {...register('destacado')}
                  className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span className="ml-2 text-sm text-gray-700">Propiedad destacada</span>
              </label>

              <label className="flex items-center">
                <input
                  type="checkbox"
                  {...register('aptoCredito')}
                  className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span className="ml-2 text-sm text-gray-700">Apto crédito</span>
              </label>
            </div>
          </div>
        )}

        {/* Tab: Ubicación */}
        {activeTab === 'ubicacion' && (
          <div className="space-y-6">
            <h3 className="text-lg font-medium text-gray-900">Ubicación</h3>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-medium text-gray-700">Barrio *</label>
                <input
                  type="text"
                  {...register('barrio', { required: 'El barrio es requerido' })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
                {errors.barrio && <p className="mt-1 text-sm text-red-600">{errors.barrio.message}</p>}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">Comuna *</label>
                <input
                  type="text"
                  {...register('comuna', { required: 'La comuna es requerida' })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
                {errors.comuna && <p className="mt-1 text-sm text-red-600">{errors.comuna.message}</p>}
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Dirección *</label>
              <input
                type="text"
                {...register('direccion', { required: 'La dirección es requerida' })}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              />
              {errors.direccion && <p className="mt-1 text-sm text-red-600">{errors.direccion.message}</p>}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-medium text-gray-700">Latitud</label>
                <input
                  type="number"
                  step="any"
                  {...register('latitud')}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                  placeholder="-34.6118"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">Longitud</label>
                <input
                  type="number"
                  step="any"
                  {...register('longitud')}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                  placeholder="-58.3960"
                />
              </div>
            </div>
          </div>
        )}

        {/* Tab: Características */}
        {activeTab === 'caracteristicas' && (
          <div className="space-y-6">
            <h3 className="text-lg font-medium text-gray-900">Características</h3>
            
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Ambientes {!['Terreno', 'Cochera', 'Galpon'].includes(tipoPropiedad) && '*'}
                </label>
                <input
                  type="number"
                  {...register('ambientes', {
                    required: !['Terreno', 'Cochera', 'Galpon'].includes(tipoPropiedad) ? 'Los ambientes son requeridos' : false,
                    min: 0
                  })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                  disabled={['Terreno', 'Cochera', 'Galpon'].includes(tipoPropiedad)}
                />
                {errors.ambientes && <p className="mt-1 text-sm text-red-600">{errors.ambientes.message}</p>}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">Dormitorios</label>
                <input
                  type="number"
                  {...register('dormitorios', { min: 0 })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">Baños</label>
                <input
                  type="number"
                  {...register('banos', { min: 0 })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">Metros Cubiertos</label>
                <input
                  type="number"
                  {...register('metrosCubiertos', { min: 0 })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">Metros Totales</label>
                <input
                  type="number"
                  {...register('metrosTotales', { min: 0 })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">Antigüedad (años)</label>
                <input
                  type="number"
                  {...register('antiguedad', { min: 0 })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">Piso</label>
                <input
                  type="number"
                  {...register('piso')}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
              </div>
            </div>

            <div>
              <label className="flex items-center">
                <input
                  type="checkbox"
                  {...register('cochera')}
                  className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span className="ml-2 text-sm text-gray-700">Tiene cochera</span>
              </label>
            </div>
          </div>
        )}

        {/* Tab: Amenities */}
        {activeTab === 'amenities' && (
          <div className="space-y-6">
            <h3 className="text-lg font-medium text-gray-900">Amenities</h3>
            
            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
              {amenitiesDisponibles.map(amenity => (
                <label key={amenity} className="flex items-center">
                  <input
                    type="checkbox"
                    checked={amenitiesSeleccionados[amenity] || false}
                    onChange={() => handleAmenityChange(amenity)}
                    className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                  />
                  <span className="ml-2 text-sm text-gray-700">{amenity}</span>
                </label>
              ))}
            </div>
          </div>
        )}

        {/* Tab: Media */}
        {activeTab === 'media' && (
          <div className="space-y-6">
            <h3 className="text-lg font-medium text-gray-900">Imágenes y Videos</h3>
            
            {propiedadId ? (
              <div className="space-y-8">
                <MediaUploader propiedadId={propiedadId} />
                {/* <ExternalUrlManager propiedadId={propiedadId} /> */}
              </div>
            ) : (
              <div className="text-center py-8 bg-gray-50 rounded-lg border-2 border-dashed border-gray-300">
                <div className="space-y-2">
                  <h4 className="text-lg font-medium text-gray-600">
                    Media disponible después de crear la propiedad
                  </h4>
                  <p className="text-sm text-gray-500">
                    Primero guarda la propiedad para poder agregar imágenes y videos
                  </p>
                </div>
              </div>
            )}
          </div>
        )}

        {/* Botones de acción */}
        <div className="flex justify-end space-x-4 pt-6 border-t border-gray-200">
          <button
            type="button"
            onClick={() => {
              reset();
              setAmenitiesSeleccionados({});
            }}
            className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          >
            Limpiar
          </button>
          
          <button
            type="submit"
            disabled={loading}
            className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
          >
            {loading ? 'Guardando...' : (propiedadId ? 'Actualizar' : 'Crear')} Propiedad
          </button>
        </div>
      </form>
    </div>
  );
};

export default PropiedadForm;
