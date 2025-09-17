'use client';

import Propiedades from '../../components/Propiedades';
import { Toaster } from 'react-hot-toast';

export default function PropiedadesPage() {
  return (
    <>
      <Propiedades />
      <Toaster position="top-right" />
    </>
  );
}