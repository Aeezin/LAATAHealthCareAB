import { useEffect, useState } from "react";
import { useAuth } from "../hooks/useAuth";
import { Navigate } from "react-router-dom";
import axios from "axios";

// API endpoint for authentication check
const AUTH_CHECK_URL = "http://localhost:5256/api/Auth/check";

// Protected route component that verifies authentication and role-based access
// Wrap any route that requires authentication with this component
// Usage: <RequireAuth allowedRoles={["Admin"]}><YourComponent /></RequireAuth>
function RequireAuth({ children, allowedRoles }) {
  const { authState, setAuthState } = useAuth();
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const checkAuth = async () => {
      try {
        const response = await axios.get(AUTH_CHECK_URL, {
          // withCredentials: true is required for sending cookies with cross-origin requests
          // This ensures the HTTP-only auth cookie is included in the request
          withCredentials: true,
        });
        setAuthState({
          isAuthenticated: true,
          user: response.data.username,
          roles: response.data.roles,
        });
      } catch (error) {
        setAuthState({
          isAuthenticated: false,
          user: null,
          roles: [],
        });
      } finally {
        setLoading(false);
      }
    };

    if (!authState.isAuthenticated) {
      checkAuth();
    } else {
      setLoading(false);
    }
  }, [authState.isAuthenticated, setAuthState]);

  if (loading) {
    return <div>Loading...</div>;
  }

  if (!authState.isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Check if user has at least one of the allowed roles
  if (
    allowedRoles &&
    !allowedRoles.some((role) => authState.roles.includes(role))
  ) {
    return <Navigate to="/unauthorized" replace />;
  }

  return children;
}

export default RequireAuth;
