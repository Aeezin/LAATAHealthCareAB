import styled from "styled-components";
import Logo from "../assets/health_care_logo.svg";
import { Link } from "react-router-dom";
import { Stack } from "@mantine/core";

// Styled components for home page layout
const HomeContainer = styled(Stack)`
  align-items: center;
`;

// Styled Link component that looks like a button
// Using Link instead of div improves accessibility and semantic HTML
const LoginButton = styled(Link)`
  cursor: pointer;
  padding: 10px 30px;
  background-color: #057d7a;
  border-radius: 10px;
  font-size: 18px;
  font-weight: 600;
  color: #fff;
  margin-top: 3rem;
  text-decoration: none;
  transition: background-color 0.3s ease, transform 0.2s ease, box-shadow 0.2s ease;

  &:hover {
    background-color: #2fadaa;
    transform: translateY(-3px);
    box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.15);
  }
`;

const LogoContainer = styled.img`
  height: 20rem;
`;

const Home = () => (
  <>
    <HomeContainer>
      <LogoContainer src={Logo} alt="Health Care Logo" />
      <LoginButton to="/login?type=patient">Patient Login</LoginButton>
      <LoginButton to="/login?type=caregiver">Caregiver Login</LoginButton>
    </HomeContainer>
  </>
);

export default Home;
