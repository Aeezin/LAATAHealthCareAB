import { createContext, useState, useContext } from "react";
import PropTypes from "prop-types";
import Logout from "../pages/Logout";

// Initial authentication state - exported for use in logout functionality
export const initialAuthState = {
  isAuthenticated: false,
  user: null,
  roles: [],
};

// Authentication context for global auth state management
export const AuthContext = createContext(null);

// Provider component that wraps the app and provides auth state to all children
export const AuthProvider = ({ children }) => {
  const [authState, setAuthState] = useState(initialAuthState);
  const logout = () => {
    setAuthState(initialAuthState);
  };


  return (
    <AuthContext.Provider value={{ authState, setAuthState, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

// Custom hook to use the auth context
export const useAuth = () => {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }

  return context;
};

AuthProvider.propTypes = {
  children: PropTypes.node.isRequired,
};
