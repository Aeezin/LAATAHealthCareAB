import styled, { css } from "styled-components";
import { Button } from "@mantine/core";

const variants = {
  primary: css`
    background-color: #057d7a;
    &:hover {
      background-color: #2fadaa;
    }
  `,
  secondary: css`
    background-color: #0288d1;
    &:hover {
      background-color: #03a9f4;
    }
  `,
  submit: css`
    background-color: #057d7a;
    align-self: flex-end;
    &:hover {
      background-color: #2fadaa;
    }
    &:disabled {
      background-color: #a0a0a0;
      cursor: not-allowed;
    }
  `,
  danger: css`
    background-color: #d32f2f;
    &:hover {
      background-color: #f44336;
    }
  `,
  success: css`
    background-color: #4caf50;
    &:hover {
      background-color: #66bb6a;
    }
  `,
  outline: css`
    background-color: transparent;
    border: 2px solid #057d7a;
    color: #057d7a;
    &:hover {
      background-color: #057d7a;
      color: white;
    }
  `,
};

const PrimaryButton = styled(Button)`
  margin-top: ${props => props.$marginTop || '40px'};
  padding: ${props => props.$padding || '10px 20px'};
  border-radius: ${props => props.$borderRadius || '4px'};
  font-weight: 500;
  width: ${props => props.$fullWidth ? '100%' : '40%'};
  transition: all 0.2s ease;
  
  /* Apply variant styles */
  ${props => variants[props.$variant || 'primary']}
  
  /* Custom overrides */
  ${props => props.$bgColor && css`
    background-color: ${props.$bgColor};
  `}
  
  ${props => props.$hoverBgColor && css`
    &:hover:not(:disabled) {
      background-color: ${props.$hoverBgColor};
    }
  `}
  
  &:hover:not(:disabled) {
    box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.15);
  }
  
  &:active:not(:disabled) {
    transform: scale(0.98);
  }

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
`;

export default PrimaryButton;