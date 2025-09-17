import React from "react";
import { useAuthStore } from "../../store/authStore";

const Protected = ({ roles, children, fallback = null }: { roles?: string[], children: React.ReactNode, fallback?: React.ReactNode }) => {
  const { isAuthenticated, role } = useAuthStore();
  const canView = isAuthenticated && (!roles || roles.includes(role));
  if (!canView) return fallback;
  return <>{children}</>;
};

export default Protected;


