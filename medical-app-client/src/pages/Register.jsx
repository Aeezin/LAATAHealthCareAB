import styled from "styled-components";
import { useState } from "react";
import axios from "axios";
import { useAuth } from "../hooks/useAuth";
import { useNavigate } from "react-router-dom";
import { IconX, IconCheck } from "@tabler/icons-react";
import { PasswordInput, Progress, Text, Popover, Box, Stack, TextInput } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import PropTypes from "prop-types";
import AuthForm from "../components/AuthForm";
import PrimaryButton from "../components/PrimaryButton";

const REGISTER_URL = "http://localhost:5256/api/Auth/register";

const RegisterContainer = styled(Stack)`
  align-items: center;
`;

const Title = styled.h2`
  font-size: 22px;
`;

const requirements = [
  { re: /[0-9]/, label: "Includes number" },
  { re: /[a-z]/, label: "Includes lowercase letter" },
  { re: /[A-Z]/, label: "Includes uppercase letter" },
  { re: /[$&+,:;=?@#|'<>.^*()%!-]/, label: "Includes special symbol" },
];

function FieldRequirement({ meets, label }) {
  return (
    <Text component="div" c={meets ? "teal" : "red"} style={{ display: "flex", alignItems: "center", paddingBottom: "15px" }} mt={7} size="sm">
      {meets ? <IconCheck size={14} /> : <IconX size={14} />}
      <Box ml={10}>{label}</Box>
    </Text>
  );
}

FieldRequirement.propTypes = {
  meets: PropTypes.bool.isRequired,
  label: PropTypes.string.isRequired,
};

function Register() {
  const { setAuthState } = useAuth();
  const navigate = useNavigate();
  const [passwordPopoverOpened, setPasswordPopoverOpened] = useState(false);
  const [confirmPasswordPopoverOpened, setConfirmPasswordPopoverOpened] = useState(false);
  const [emailPopoverOpened, setEmailPopoverOpened] = useState(false);
  const [firstNamePopoverOpened, setFirstNamePopoverOpened] = useState(false);
  const [lastNamePopoverOpened, setLastNamePopoverOpened] = useState(false);
  const [personalIdPopoverOpened, setPersonalIdPopoverOpened] = useState(false);
  const [phonePopoverOpened, setPhonePopoverOpened] = useState(false);

  const [confirmPassword, setConfirmPassword] = useState("");

  const [visible, { toggle }] = useDisclosure(false);
  const [error, setError] = useState("");
  const [validationErrors, setValidationErrors] = useState({});

  const [credentials, setCredentials] = useState({
    personalIdentityNumber: "",
    password: "",
    firstName: "",
    lastName: "",
    email: "",
    phone: "",
  });

  const checks = requirements.map((requirement, index) => (
    <FieldRequirement key={index} label={requirement.label} meets={requirement.re.test(credentials.password)} />
  ));

  const strength = getStrength(credentials.password);
  const match = getMatch(confirmPassword, credentials.password);
  const color = strength === 100 ? "teal" : strength > 50 ? "yellow" : "red";

  const allFieldsFilled =
    credentials.personalIdentityNumber.trim() !== "" &&
    credentials.password.trim() !== "" &&
    confirmPassword.trim() !== "" &&
    credentials.firstName.trim() !== "" &&
    credentials.lastName.trim() !== "" &&
    credentials.email.trim() !== "";

  const handleInputChange = (e) => {
    setCredentials((prev) => ({ ...prev, [e.target.name]: e.target.value }));
    // Clear validation error for this field when user starts typing
    if (validationErrors[e.target.name]) {
      setValidationErrors((prev) => ({ ...prev, [e.target.name]: "" }));
    }
  };

  const handleConfirmPasswordChange = (e) => {
    setConfirmPassword(e.target.value);
    // Clear validation error when user starts typing
    if (validationErrors.confirmPassword) {
      setValidationErrors((prev) => ({ ...prev, confirmPassword: "" }));
    }
  };

  const validateForm = () => {
    const errors = {};

    if (!validatePersonalIdentityNumber(credentials.personalIdentityNumber)) {
      errors.personalIdentityNumber = "Invalid format. Use YYYYMMDD-XXXX";
    }

    if (!validateEmail(credentials.email)) {
      errors.email = "Invalid email address";
    }

    if (credentials.phone.length > 0 && !isValidSwedishPhoneNumber(credentials.phone)) {
      errors.phone = "Invalid phone number";
    }

    if (credentials.firstName.trim().length < 2) {
      errors.firstName = "First name must be at least 2 characters";
    }

    if (credentials.lastName.trim().length < 2) {
      errors.lastName = "Last name must be at least 2 characters";
    }

    if (strength !== 100) {
      errors.password = "Password does not meet all requirements";
    }

    if (!match) {
      errors.confirmPassword = "Passwords do not match";
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const isFormValid =
    allFieldsFilled && strength === 100 && match && (credentials.phone.length === 0 || isValidSwedishPhoneNumber(credentials.phone)) && validateForm;
    
  const handleRegistration = async (e) => {
    e.preventDefault();

    // Clear previous errors
    setError("");

    // Validate form
    if (!validateForm()) {
      setError("Please fix the errors below");
      return;
    }

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
        throw new Error("Admin login not implemented yet");
        // navigate("/admin/dashboard", { replace: true });
      } else if (roles.includes("Patient")) {
        navigate("/patient/dashboard", { replace: true });
      } else if (roles.includes("Caregiver")) {
        navigate("/caregiver/dashboard", { replace: true });
      }
    } catch (error) {
      console.error("Registration failed:", error.response || error);
      setError(error.response?.data?.message || "Registration failed. Please try again.");
    }
  };

  return (
    <RegisterContainer>
      <Title>Patient Registration</Title>
      {error && <p style={{ color: "red" }}>{error}</p>}
      <AuthForm onSubmit={handleRegistration} aria-label="Register form">
        <Popover opened={personalIdPopoverOpened} position="bottom" width="target" transitionProps={{ transition: "pop" }}>
          <Popover.Target>
            <div onFocusCapture={() => setPersonalIdPopoverOpened(true)} onBlurCapture={() => setPersonalIdPopoverOpened(false)}>
              <TextInput
                label="Personal Identity Number"
                name="personalIdentityNumber"
                placeholder="YYYYMMDD-XXXX"
                required
                value={credentials.personalIdentityNumber}
                onChange={handleInputChange}
                error={validationErrors.personalIdentityNumber}
              />{" "}
            </div>
          </Popover.Target>
          <Popover.Dropdown>
            <FieldRequirement
              label="Valid personal identity number format: YYYYMMDD-XXXX"
              meets={validatePersonalIdentityNumber(credentials.personalIdentityNumber) === true}
            />
          </Popover.Dropdown>
        </Popover>

        <Popover opened={passwordPopoverOpened} position="bottom" width="target" transitionProps={{ transition: "pop" }}>
          <Popover.Target>
            <div onFocusCapture={() => setPasswordPopoverOpened(true)} onBlurCapture={() => setPasswordPopoverOpened(false)}>
              <PasswordInput
                withAsterisk
                label="Password"
                name="password"
                placeholder="Password..."
                value={credentials.password}
                onChange={handleInputChange}
                visible={visible}
                onVisibilityChange={toggle}
                error={validationErrors.password}
              />
            </div>
          </Popover.Target>
          <Popover.Dropdown>
            <Progress color={color} value={strength} size={5} mb="xs" />
            <FieldRequirement label="Includes at least 8 characters" meets={credentials.password.length > 7} />
            {checks}
          </Popover.Dropdown>
        </Popover>

        <Popover opened={confirmPasswordPopoverOpened} position="bottom" width="target" transitionProps={{ transition: "pop" }}>
          <Popover.Target>
            <div onFocusCapture={() => setConfirmPasswordPopoverOpened(true)} onBlurCapture={() => setConfirmPasswordPopoverOpened(false)}>
              <PasswordInput
                withAsterisk
                label="Confirm Password"
                placeholder="Password..."
                value={confirmPassword}
                onChange={handleConfirmPasswordChange}
                visible={visible}
                onVisibilityChange={toggle}
                error={validationErrors.confirmPassword}
              />
            </div>
          </Popover.Target>
          <Popover.Dropdown>
            <FieldRequirement label="Passwords match" meets={match} />
          </Popover.Dropdown>
        </Popover>

        <Popover opened={firstNamePopoverOpened} position="bottom" width="target" transitionProps={{ transition: "pop" }}>
          <Popover.Target>
            <div onFocusCapture={() => setFirstNamePopoverOpened(true)} onBlurCapture={() => setFirstNamePopoverOpened(false)}>
              <TextInput
                label="First Name"
                name="firstName"
                placeholder="Name..."
                required
                value={credentials.firstName}
                onChange={handleInputChange}
                error={validationErrors.firstName}
              />
            </div>
          </Popover.Target>
          <Popover.Dropdown>
            <FieldRequirement label="First name is not empty" meets={validateName(credentials.firstName) === true} />
          </Popover.Dropdown>
        </Popover>

        <Popover opened={lastNamePopoverOpened} position="bottom" width="target" transitionProps={{ transition: "pop" }}>
          <Popover.Target>
            <div onFocusCapture={() => setLastNamePopoverOpened(true)} onBlurCapture={() => setLastNamePopoverOpened(false)}>
              <TextInput
                label="Last Name"
                name="lastName"
                placeholder="Name..."
                required
                value={credentials.lastName}
                onChange={handleInputChange}
                error={validationErrors.lastName}
              />
            </div>
          </Popover.Target>
          <Popover.Dropdown>
            <FieldRequirement label="Last name is not empty" meets={validateName(credentials.lastName) === true} />
          </Popover.Dropdown>
        </Popover>

        <Popover opened={emailPopoverOpened} position="bottom" width="target" transitionProps={{ transition: "pop" }}>
          <Popover.Target>
            <div onFocusCapture={() => setEmailPopoverOpened(true)} onBlurCapture={() => setEmailPopoverOpened(false)}>
              <TextInput
                label="Email"
                name="email"
                type="email"
                placeholder="example@domain.com"
                required
                value={credentials.email}
                onChange={handleInputChange}
                error={validationErrors.email}
              />
            </div>
          </Popover.Target>
          <Popover.Dropdown>
            <FieldRequirement label="Valid email address format" meets={validateEmail(credentials.email) === true} />
          </Popover.Dropdown>
        </Popover>

        <Popover type="tel" opened={phonePopoverOpened} position="bottom" width="target" transitionProps={{ transition: "pop" }}>
          <Popover.Target>
            <div onFocusCapture={() => setPhonePopoverOpened(true)} onBlurCapture={() => setPhonePopoverOpened(false)}>
              <TextInput
                label="Phone"
                name="phone"
                type="phone"
                placeholder="+46 70 123 45 67"
                value={credentials.phone}
                onChange={handleInputChange}
                error={validationErrors.phone}
              />
            </div>
          </Popover.Target>
          <Popover.Dropdown hidden={credentials.phone.length === 0}>
            <FieldRequirement label="Valid phone number format" meets={isValidSwedishPhoneNumber(credentials.phone) === true} />
          </Popover.Dropdown>
        </Popover>

        <PrimaryButton disabled={!isFormValid} type="submit" $variant="submit">
          Submit
        </PrimaryButton>
      </AuthForm>
    </RegisterContainer>
  );
}

// Validation helper functions

const validatePersonalIdentityNumber = (pin) => {
  // Swedish format: YYYYMMDD-XXXX (10 digits total, with dash)
  const regex = /^\d{8}-\d{4}$/;
  return regex.test(pin);
};

function getStrength(password) {
  let multiplier = password.length > 5 ? 0 : 1;

  requirements.forEach((requirement) => {
    if (!requirement.re.test(password)) {
      multiplier += 1;
    }
  });

  return Math.max(100 - (100 / (requirements.length + 1)) * multiplier, 10);
}

function getMatch(confirmPassword, password) {
  return confirmPassword === password;
}

const validateName = (name) => {
  return name.trim().length >= 1;
};

const validateEmail = (email) => {
  // Standard email validation
  const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return regex.test(email);
};

function cleanNumber(number) {
  // Ta bort allt som inte är siffror
  return number.replace(/\D/g, "");
}

function isValidSwedishPhoneNumber(number) {
  let digits = cleanNumber(number);

  // Hantera landsnummer +46
  if (digits.startsWith("46")) digits = "0" + digits.slice(2);

  // Mobilnummer 07xx
  if (digits.startsWith("07") && digits.length === 10) return true;

  // Fast telefoni (8–10 siffror)
  if (digits.startsWith("0") && digits.length >= 8 && digits.length <= 10) return true;

  return false;
}

export default Register;
