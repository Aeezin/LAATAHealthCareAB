import styled from "styled-components";
import { IconHomeFilled, IconUserFilled, IconClockFilled } from "@tabler/icons-react";
import { Link } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";

const StyledNavbar = styled.nav`
  width: 100%;
  height: 60px;
  background-color: #057d7a;
  display: flex;
  align-items: center;
  padding: 0 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  color: #ffffff;
  font-size: 20px;
  font-weight: bold;

  position: fixed;
  top: 0;
  left: 0;
  z-index: 1000;

  @media (max-width: 768px) {
    top: auto;
    bottom: 0;
  }
`;

const NavTitle = styled.div`
  text-align: center;
  font-size: 27px;
`;

const NavIcons = styled.div`
  display: flex;
  align-items: center;
  gap: 50px;
  margin-left: 60px;
  margin-right: 60px;

  @media (max-width: 768px) {
    width: 100%;
    justify-content: space-around;
    gap: 0;
  }
`;

const NavLink = styled(Link)`
  color: #ffffff;
  display: flex;
  align-items: center;
  text-decoration: none;
  transition: opacity 0.2s;

  &:hover {
    opacity: 0.8;
  }
`;

function Navbar() {
  const { authState } = useAuth();

  const isAuthenticated = Boolean(authState?.isAuthenticated);
  const roles = Array.isArray(authState?.roles) ? authState.roles : [];

  const isPatient = roles.includes("Patient");
  const isCaregiver = roles.includes("Caregiver");


  const profileRoute = isPatient
    ? "/profile-patient"
    : isCaregiver
      ? "/profile-caregiver"
      : null;

  const bookingRoute = isPatient
    ? "/choose-caregiver"
    : isCaregiver
      ? "/booking"
      : null;

  return (
    <StyledNavbar>
      <NavTitle>HealthCare AB</NavTitle>
      <NavIcons>
        <NavLink to="/">
          <IconHomeFilled size={28} />
        </NavLink>

        {isAuthenticated && (
          <>
            <NavLink to={profileRoute}>
              <IconUserFilled size={28} />
            </NavLink>

            {bookingRoute && (
              <NavLink to={bookingRoute}>
                <IconClockFilled size={28} />
              </NavLink>
            )}
          </>
        )}
      </NavIcons>
    </StyledNavbar>
  );
}

export default Navbar;
