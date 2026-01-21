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
import { Stack } from "@mantine/core";
import BookingView from "./pages/BookingView";
import ChooseCaregiver from "./pages/ChooseCaregiver";
import PatientProfile from "./pages/PatientProfile";
import CaregiverProfile from "./pages/CaregiverProifle";

// AuthProvider must wrap Router to ensure auth state is available to all routes
function App() {
  return (
    <AuthProvider>
      <GlobalStyle />
      <Navbar />
      <Stack className="content">
        <Routes>
          {/* Public routes - accessible without authentication */}
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/unauthorized" element={<Unauthorized />} />
          <Route path="/choose-caregiver"
            element={<ChooseCaregiver />} />


          {/* TEMP PUBLIC MOVE TO PROTECTED LATER */}
          <Route path="/booking" element={<BookingView />} />
          <Route path="/booking/:caregiverId" element={<BookingView />} />
          <Route path="/profile-patient"
            element={<PatientProfile />} />
          <Route path="/profile-caregiver"
            element={<CaregiverProfile />} />

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


          {/* Fallback route - redirects unknown paths to home */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Stack>
    </AuthProvider>
  );
}

export default App;
