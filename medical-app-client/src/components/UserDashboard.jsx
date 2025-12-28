import { useAuth } from "../hooks/useAuth";
import Logo from "../assets/health_care_logo.svg";
import styled from "styled-components";
import Logout from "./Logout";

// Styled components for user dashboard layout
const UserContainer = styled.div`
  display: flex;
  align-items: center;
  justify-content: center;
  flex-direction: column;
`;

const LogoContainer = styled.img`
  height: 20rem;
`;

const Title = styled.h2`
  font-size: 22px;
`;

const Text = styled.p`
  font-size: 18px;
`;

// Only accessible to users with the "User" role
function UserDashboard() {
  const {
    authState: { user },
  } = useAuth();

  return (
    <UserContainer>
      <LogoContainer src={Logo} alt="Health Care Logo" />
      <Title>User Dashboard</Title>
      <Text>Welcome, {user}!</Text>
      <Logout />
    </UserContainer>
  );
}

export default UserDashboard;
