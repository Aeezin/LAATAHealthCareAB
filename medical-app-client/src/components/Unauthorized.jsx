import { useNavigate } from "react-router-dom";
import styled from "styled-components";

// Styled components for unauthorized page layout
const UnauthorizedContainer = styled.div`
  display: flex;
  align-items: center;
  justify-content: center;
  flex-direction: column;
`;

const Title = styled.h2`
  font-size: 30px;
  color: #eb4d1b;
`;

const Text = styled.p`
  font-size: 18px;
`;

// Styled button component (using semantic button element for accessibility)
const Button = styled.button`
  cursor: pointer;
  padding: 10px 30px;
  background-color: #057d7a;
  border-radius: 10px;
  font-size: 18px;
  font-weight: 600;
  color: #fff;
  border: none;
  margin-top: 3rem;
  transition: background-color 0.3s ease, transform 0.2s ease,
    box-shadow 0.2s ease;

  &:hover {
    background-color: #2fadaa;
    transform: translateY(-3px);
    box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.15);
  }
`;

// Unauthorized page component
// Displayed when a user tries to access a page they don't have permission for
function Unauthorized() {
  const navigate = useNavigate();

  // Navigate back to the previous page in browser history
  const goBack = () => {
    navigate(-1);
  };

  return (
    <UnauthorizedContainer>
      <Title>Unauthorized</Title>
      <Text>You do not have permission to view this page.</Text>
      <Button onClick={goBack} aria-label="Go back to previous page">
        Go Back
      </Button>
    </UnauthorizedContainer>
  );
}

export default Unauthorized;
