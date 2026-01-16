import { Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";
import Login from "./pages/Login";
import Register from "./pages/Register";
import UserDashboard from "./pages/UserDashboard";
import AdminDashboard from "./pages/AdminDashboard";
import Unauthorized from "./components/Unauthorized";
import Home from "./pages/Home";
import RequireAuth from "./components/RequireAuth";
import GlobalStyle from "./styles/GlobalStyle";
import "@mantine/core/styles.css";
import Navbar from "./components/Navbar";

// AuthProvider must wrap Router to ensure auth state is available to all routes
function App() {
  return (
    <AuthProvider>
      <GlobalStyle />
      <Navbar />
      <div className="content">
        <Routes>
          {/* Public routes - accessible without authentication */}
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/unauthorized" element={<Unauthorized />} />

          {/* Protected routes - require authentication and specific roles */}
          <Route
            path="/user/dashboard"
            element={
              <RequireAuth allowedRoles={["User"]}>
                <UserDashboard />
              </RequireAuth>
            }
          />
          <Route
            path="/admin/dashboard"
            element={
              <RequireAuth allowedRoles={["Admin"]}>
                <AdminDashboard />
              </RequireAuth>
            }
          />
          <Route path="/choose-caregiver"
           element={<RequireAuth allowedRoles={["User"]}> 
           <ChooseCaregiver/> 
           </RequireAuth>} />

          {/* Fallback route - redirects unknown paths to home */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </div>
    </AuthProvider>
  );
}

export default App;
