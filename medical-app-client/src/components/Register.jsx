import styled from "styled-components";
import { useState } from "react";
import axios from "axios";
import { useAuth } from "../hooks/useAuth";
import { useNavigate } from "react-router-dom";
import { IconX, IconCheck } from "@tabler/icons-react";
import { PasswordInput, Progress, Text, Popover, Box, Stack, TextInput } from "@mantine/core";
import PropTypes from "prop-types";

const REGISTER_URL = "http://localhost:5256/api/Auth/register";

const RegisterContainer = styled(Stack)`
  height: 100vh;
  align-items: center;
`;

const RegisterButton = styled.button`
  cursor: pointer;
  padding: 10px 30px;
  background-color: #057d7a;
  border-radius: 10px;
  font-size: 18px;
  font-weight: 600;
  color: #fff;
  margin-top: 40px;
  transition: background-color 0.3s ease, transform 0.2s ease, box-shadow 0.2s ease;
  text-align: center;
  border: none;

  &:hover {
    background-color: #2fadaa;
    transform: translateY(-3px);
    box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.15);
  }
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

function PasswordRequirement({ meets, label }) {
  return (
    <Text component="div" c={meets ? "teal" : "red"} style={{ display: "flex", alignItems: "center", paddingBottom: "15px" }} mt={7} size="sm">
      {meets ? <IconCheck size={14} /> : <IconX size={14} />}
      <Box ml={10}>{label}</Box>
    </Text>
  );
}
PasswordRequirement.propTypes = {
  meets: PropTypes.bool.isRequired,
  label: PropTypes.string.isRequired,
};

const requirements = [
  { re: /[0-9]/, label: "Includes number" },
  { re: /[a-z]/, label: "Includes lowercase letter" },
  { re: /[A-Z]/, label: "Includes uppercase letter" },
  { re: /[$&+,:;=?@#|'<>.^*()%!-]/, label: "Includes special symbol" },
];

function getStrength(password) {
  let multiplier = password.length > 5 ? 0 : 1;

  requirements.forEach((requirement) => {
    if (!requirement.re.test(password)) {
      multiplier += 1;
    }
  });

  return Math.max(100 - (100 / (requirements.length + 1)) * multiplier, 10);
}

function Register() {
  const { setAuthState } = useAuth();
  const navigate = useNavigate();
  const [popoverOpened, setPopoverOpened] = useState(false);
  const [error, setError] = useState("");

  const [credentials, setCredentials] = useState({
    personalIdentityNumber: "",
    password: "",
    firstName: "",
    lastName: "",
    email: "",
    address: "",
  });

  const checks = requirements.map((requirement, index) => <PasswordRequirement key={index} label={requirement.label} meets={requirement.re.test(credentials.password)} />);

  const strength = getStrength(credentials.password);
  const color = strength === 100 ? "teal" : strength > 50 ? "yellow" : "red";

  const handleInputChange = (e) => {
    setCredentials((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleRegistration = async (e) => {
    e.preventDefault();

    try {
      const response = await axios.post(REGISTER_URL, credentials, {
        withCredentials: true,
      });

      console.log("Registration successful:", JSON.stringify(response.data));

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
      } else if (roles.includes("Patient")) {
        navigate("/patient/dashboard", { replace: true });
      }
       else if (roles.includes("Caregiver")) {
        navigate("/caregiver/dashboard", { replace: true });
      }
    } catch (error) {
      console.error("Login failed:", error.response || error);
      setError("Invalid login details. Please try again.");
    }
  };

  return (
    <RegisterContainer>
      <Title>Register</Title>
      {error && <p style={{ color: "red" }}>{error}</p>}
      <FormWrapper onSubmit={handleRegistration} aria-label="Login form">
        <TextInput label="Personal Identity Number" name="personalIdentityNumber" placeholder="YYMMDD-xxxx" required value={credentials.personalIdentityNumber} onChange={handleInputChange} />

        <Popover opened={popoverOpened} position="bottom" width="target" transitionProps={{ transition: "pop" }}>
          <Popover.Target>
            <div onFocusCapture={() => setPopoverOpened(true)} onBlurCapture={() => setPopoverOpened(false)}>
              <PasswordInput withAsterisk label="Password" name="password" placeholder="Password..." value={credentials.password} onChange={handleInputChange} />
            </div>
          </Popover.Target>
          <Popover.Dropdown>
            <Progress color={color} value={strength} size={5} mb="xs" />
            <PasswordRequirement label="Includes at least 8 characters" meets={credentials.password.length > 7} />
            {checks}
          </Popover.Dropdown>
        </Popover>
        <TextInput label="First Name" name="firstName" placeholder="Name..." required value={credentials.firstName} onChange={handleInputChange} />
        <TextInput label="Last Name" name="lastName" placeholder="Name..." required value={credentials.lastName} onChange={handleInputChange} />
        <TextInput label="Email" name="email" placeholder="example@gmail.com" required value={credentials.email} onChange={handleInputChange} />
        <TextInput label="Address" name="address" placeholder="street, city, zip" required value={credentials.address} onChange={handleInputChange} />

        <RegisterButton type="submit">Register</RegisterButton>
      </FormWrapper>
    </RegisterContainer>
  );
}

export default Register;
