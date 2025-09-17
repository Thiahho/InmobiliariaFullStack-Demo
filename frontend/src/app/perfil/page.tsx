'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '../../store/authStore';
import { axiosClient } from '../../lib/axiosClient';
import UserProfile from '../../components/users/UserProfile';
import Link from 'next/link';
import { ChevronLeftIcon } from '@heroicons/react/24/outline';

export default function PerfilPage() {
    const router = useRouter();
    const { isAuthenticated, user, role, login, logout } = useAuthStore();
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
            {/* Header */}
            <div className="bg-white shadow">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                    <div className="flex justify-between items-center py-4">
                        <div className="flex items-center">
                            <Link
                                href="/admin"
                                className="inline-flex items-center text-sm font-medium text-gray-500 hover:text-gray-700 mr-4"
                            >
                                <ChevronLeftIcon className="h-4 w-4 mr-1" />
                                Panel de Administración
                            </Link>
                            <div className="text-sm text-gray-300">/ Mi Perfil</div>
                        </div>
                        <div className="flex items-center space-x-4">
                            <div className="text-right">
                                <p className="text-sm font-medium text-gray-900">{user?.nombre}</p>
                                <p className="text-xs text-gray-500">{role}</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Content */}
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
                <UserProfile />
            </div>
        </div>
    );
}


