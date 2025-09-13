'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import UsersAdmin from '../../../components/users/UsersAdmin';
import { useAuthStore, Roles } from '../../../store/authStore';
import { axiosClient } from '../../../lib/axiosClient';

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
                <h1 className="text-3xl font-bold mb-4">Gestión de Agentes</h1>
                <UsersAdmin />
            </div>
        </div>
    );
}


