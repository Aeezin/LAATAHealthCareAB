import styled from "styled-components";
import { useState } from "react";
import axios from "axios";
import { useAuth } from "../hooks/useAuth";
import { useNavigate } from "react-router-dom";
import { Button, PasswordInput, TextInput } from "@mantine/core";

// API endpoint for login
const LOGIN_URL = "http://localhost:5256/api/Auth/login";

// Styled components for login page layout
const LoginContainer = styled.div`
  display: flex;
  align-items: center;
  justify-content: center;
  flex-direction: column;
`;

const LoginButton = styled(Button)`
  margin-top: 40px;
`;

const Title = styled.h2`
  font-size: 22px;
`;

const FormWrapper = styled.form`
  padding: 40px;
  display: flex;
  flex-direction: column;
  background-color: #ffffff;
  border-radius: 15px;
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.1);
  width: 350px;
  gap: 10px;
`;

const UserType = {
  PATIENT: "patient",
  CAREGIVER: "caregiver",
  ADMIN: "admin",
}

function Login(userType) {
  const { setAuthState } = useAuth();
  const navigate = useNavigate();
  const [credentials, setCredentials] = useState({
    username: "",
    password: "",
  });
  const [error, setError] = useState("");

  const handleInputChange = (e) => {
    setCredentials((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleLogin = async (e) => {
    e.preventDefault();

    try {
      const response = await axios.post(LOGIN_URL, credentials, {
        // withCredentials: true is required for the server to set HTTP-only cookies
        // This is essential for cookie-based authentication
        withCredentials: true,
      });

      console.log("Login successful:", JSON.stringify(response.data));

      const { loggedInUser, roles } = response.data;

      // Update global auth state with user information
      setAuthState({
        isAuthenticated: true,
        user: loggedInUser,
        roles: roles,
      });

      // Redirect based on user role
      if (roles.includes("Admin")) {
        navigate("/admin/dashboard", { replace: true });
      } else {
        navigate("/user/dashboard", { replace: true });
      }
    } catch (error) {
      console.error("Login failed:", error.response || error);
      setError("Invalid username or password");
    }
  };

  return userType === UserType.PATIENT? (
    <LoginContainer>
      <Title>Login</Title>
      {error && <p style={{ color: "red" }}>{error}</p>}
      <FormWrapper onSubmit={handleLogin} aria-label="Login form">
        <TextInput label="Personal Identity Number" onChange={handleInputChange}></TextInput>
        <PasswordInput label="Password" onChange={handleInputChange}></PasswordInput>
        <LoginButton type="submit">Login</LoginButton>
      </FormWrapper>
    </LoginContainer>
  ) : userType === UserType.CAREGIVER ? (
    <LoginContainer>
      <Title>Login</Title>
      {error && <p style={{ color: "red" }}>{error}</p>}
      <FormWrapper onSubmit={handleLogin} aria-label="Login form">
        <TextInput label="Username" onChange={handleInputChange}></TextInput>
        <PasswordInput label="Password" onChange={handleInputChange}></PasswordInput>
        <LoginButton type="submit" >Login</LoginButton>
      </FormWrapper>
    </LoginContainer>
  ) : userType === UserType.ADMIN ? (
    <LoginContainer>
      <Title>Login</Title>
      {error && <p style={{ color: "red" }}>{error}</p>}
      <FormWrapper onSubmit={handleLogin} aria-label="Login form">
        <TextInput label="Username" onChange={handleInputChange}></TextInput>
        <PasswordInput label="Password" onChange={handleInputChange}></PasswordInput>
        <LoginButton type="submit">Login</LoginButton>
      </FormWrapper>
    </LoginContainer>
  ) : (
    <LoginContainer>
      <Title>Login</Title>
      <FormWrapper onSubmit={handleLogin} aria-label="Login form">
        <p style={{ color: "red", alignSelf: "center" }}>User type not detected.</p>
      </FormWrapper>
    </LoginContainer>
  );
}

export default Login;
