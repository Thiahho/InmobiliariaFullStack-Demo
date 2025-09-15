import React, { ReactNode } from "react";
import { useAuthStore } from "../../store/authStore";

type ProtectedProps = {
  roles?: string[];
  children: ReactNode;
  fallback?: ReactNode;
};

type AuthShape = {
  isAuthenticated: boolean;
  role: string;
};


const Protected: React.FC<ProtectedProps> = ({ roles, children, fallback = null }) => {
  const { isAuthenticated, role } = useAuthStore() as unknown as AuthShape;
  const canView = isAuthenticated && (!roles || roles.includes(role));
  if (!canView) return <>{fallback}</>;
  return <>{children}</>;
};

export default Protected;


