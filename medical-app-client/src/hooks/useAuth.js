import { useContext } from "react";
import { AuthContext } from "../context/AuthContext";

// Custom hook to access authentication context
// Throws an error if used outside of AuthProvider
export const useAuth = () => {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }

  return context;
};
