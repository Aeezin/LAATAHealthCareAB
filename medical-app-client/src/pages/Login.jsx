import styled from "styled-components";
import { useState } from "react";
import axios from "axios";
import { useAuth } from "../hooks/useAuth";
import { useNavigate } from "react-router-dom";
import { PasswordInput, TextInput, Group, Stack } from "@mantine/core";
import { useSearchParams } from "react-router-dom";
import PrimaryButton from "../components/PrimaryButton";
import AuthForm from "../components/AuthForm";

// API endpoint for login
const LOGIN_URL_PATIENT = "http://localhost:5256/api/Auth/login-patient";
const LOGIN_URL_CAREGIVER = "http://localhost:5256/api/Auth/login-caregiver";

// Styled components for login page layout
const LoginContainer = styled(Stack)`
  align-items: center;
`;

const Title = styled.h2`
  font-size: 22px;
`;

const UserType = {
  PATIENT: "patient",
  CAREGIVER: "caregiver",
  ADMIN: "admin",
};

function Login() {
  const [searchParams] = useSearchParams();
  const userType = searchParams.get("type");
  const { setAuthState } = useAuth();
  const navigate = useNavigate();
  const [credentials, setCredentials] = useState({
    identifier: "",
    password: "",
  });
  const [error, setError] = useState("");

  const handleInputChange = (e) => {
    setCredentials((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleLogin = async (e) => {
    e.preventDefault();

    try {
      var LOGIN_URL = "";
      if (userType === UserType.ADMIN) {
        // LOGIN_URL = "";
        throw new Error("Admin login not implemented yet");
      }
      if (userType === UserType.PATIENT) {
        LOGIN_URL = LOGIN_URL_PATIENT;
      } else if (userType === UserType.CAREGIVER) {
        LOGIN_URL = LOGIN_URL_CAREGIVER;
      }

      const response = await axios.post(LOGIN_URL, credentials, {
        // withCredentials: true is required for the server to set HTTP-only cookies
        // This is essential for cookie-based authentication
        withCredentials: true,
      });

      console.log("Login successful:", JSON.stringify(response.data));

      const { loggedInUser, roles, entityId } = response.data;

      // Update global auth state with user information
      setAuthState({
        isAuthenticated: true,
        user: loggedInUser,
        roles: roles,
        entityId: entityId,
      });

      // Redirect based on user role
      if (roles.includes("Admin")) {
        // navigate("/admin/dashboard", { replace: true });
        throw new Error("Admin login not implemented yet");
      } else if (roles.includes("Patient")) {
        navigate("/", { replace: true });
      } else if (roles.includes("Caregiver")) {
        navigate("/", { replace: true });
      }
    } catch (error) {
      console.error("Login failed:", error.response || error);
      setError("Invalid username or password");
    }
  };

  return userType === UserType.PATIENT ? (
    <LoginContainer>
      <Title>Patient Login</Title>
      {error && <p style={{ color: "red" }}>{error}</p>}
      <AuthForm onSubmit={handleLogin} aria-label="Login form">
        <TextInput label="Personal Identity Number" name="identifier" onChange={handleInputChange}></TextInput>
        <PasswordInput label="Password" name="password" onChange={handleInputChange}></PasswordInput>
        <Group justify="space-between">
          <PrimaryButton type="button" $variant="secondary" onClick={() => navigate("/register?type=patient")}>
            Register
          </PrimaryButton>
          <PrimaryButton type="submit" $variant="submit">
            Login
          </PrimaryButton>
        </Group>
      </AuthForm>
    </LoginContainer>
  ) : userType === UserType.CAREGIVER ? (
    <LoginContainer>
      <Title>Caregiver Login</Title>
      {error && <p style={{ color: "red" }}>{error}</p>}
      <AuthForm onSubmit={handleLogin} aria-label="Login form">
        <TextInput label="Username" name="identifier" onChange={handleInputChange}></TextInput>
        <PasswordInput label="Password" name="password" onChange={handleInputChange}></PasswordInput>
        <PrimaryButton type="submit" $variant="submit">
          Login
        </PrimaryButton>
      </AuthForm>
    </LoginContainer>
  ) : userType === UserType.ADMIN ? (
    <LoginContainer>
      <Title>Login</Title>
      <AuthForm aria-label="Login form">
        <p style={{ color: "red", alignSelf: "center" }}>Admin login not implemented</p>
      </AuthForm>
    </LoginContainer>
  ) : (
    <LoginContainer>
      <Title>Login</Title>
      <AuthForm aria-label="Login form">
        <p style={{ color: "red", alignSelf: "center" }}>User type not detected.</p>
      </AuthForm>
    </LoginContainer>
  );
}

export default Login;