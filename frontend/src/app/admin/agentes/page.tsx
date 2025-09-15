'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import UsersAdmin from '../../../components/users/UsersAdmin';
import { useAuthStore, Roles } from '../../../store/authStore';
import { axiosClient } from '../../../lib/axiosClient';
import { ArrowLeftIcon } from '@heroicons/react/24/outline';

export default function AgentesPage() {
    const router = useRouter();
    const { isAuthenticated, user, role, logout } = useAuthStore();
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const checkAuth = async () => {
            try {
                const token = localStorage.getItem('access_token');
                if (!token) {
                    router.push('/');
                    return;
                }
                const response = await axiosClient.get('/auth/me');
                if (response.data) {
                    const userData = response.data;
                    useAuthStore.getState().login(userData, userData.rol || userData.role);
                }
            } catch (error) {
                localStorage.removeItem('access_token');
                localStorage.removeItem('refresh_token');
                logout();
                router.push('/');
            } finally {
                setLoading(false);
            }
        };
        checkAuth();
    }, [router, logout]);

    if (loading) return <div className="p-6">Cargando...</div>;
    if (!isAuthenticated || role !== Roles.Admin) {
        router.push('/');
        return null;
    }

    return (
        <div className="min-h-screen bg-gray-100">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
                <div className="flex items-center justify-between mb-6">
                    <h1 className="text-3xl font-bold">Gestión de Agentes</h1>
                    <button
                        onClick={() => router.push('/admin')}
                        className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 hover:text-gray-700 transition-colors"
                    >
                        <ArrowLeftIcon className="w-4 h-4" />
                        Volver al Panel
                    </button>
                </div>

                <UsersAdmin />
            </div>
        </div>
    );
}


