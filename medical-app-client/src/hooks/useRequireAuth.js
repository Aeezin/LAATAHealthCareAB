import { useEffect } from "react";
import { useAuth } from "./useAuth";
import { useNavigate } from "react-router-dom";

// OPTIONAL CUSTOM HOOK
// Custom hook for authentication checks (ALTERNATIVE to RequireAuth component)
// Use this hook when you want to handle auth logic inside a component
// rather than wrapping it with RequireAuth

// Redirects to /login if user is not authenticated
// Redirects to /unauthorized if user lacks required roles
export const useRequireAuth = (allowedRoles) => {
  const {
    authState: { isAuthenticated, roles },
  } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (!isAuthenticated) {
      navigate("/login", { replace: true });
    } else if (
      allowedRoles &&
      !allowedRoles.some((role) => roles.includes(role))
    ) {
      navigate("/unauthorized", { replace: true });
    }
  }, [isAuthenticated, roles, allowedRoles, navigate]);

  return { isAuthenticated, roles };
};
