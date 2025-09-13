'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '../../store/authStore';
import { axiosClient } from '../../lib/axiosClient';
import UserProfile from '../../components/users/UserProfile';

export default function PerfilPage() {
    const router = useRouter();
    const { isAuthenticated, user, login, logout } = useAuthStore();
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
                    login(userData, userData.rol || userData.role);
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
    }, [router, login, logout]);

    if (loading) return <div className="p-6">Cargando...</div>;
    if (!isAuthenticated || !user) {
        router.push('/');
        return null;
    }

    return (
        <div className="min-h-screen bg-gray-100">
            <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
                <div className="flex items-center justify-between mb-4">
                    <h1 className="text-3xl font-bold">Mi perfil</h1>
                    <a href="/admin" className="inline-flex items-center gap-2 rounded bg-blue-600 text-white px-4 py-2 hover:bg-blue-700">Volver al Panel</a>
                </div>
                <UserProfile />
            </div>
        </div>
    );
}


