import { createContext, useState } from "react";

// Initial authentication state - exported for use in logout functionality
export const initialAuthState = {
  isAuthenticated: false,
  user: null,
  roles: [],
};

// Authentication context for global auth state management
export const AuthContext = createContext();

// Provider component that wraps the app and provides auth state to all children
export const AuthProvider = ({ children }) => {
  const [authState, setAuthState] = useState(initialAuthState);

  return (
    <AuthContext.Provider value={{ authState, setAuthState }}>
      {children}
    </AuthContext.Provider>
  );
};
