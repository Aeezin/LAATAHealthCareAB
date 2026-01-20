import { createGlobalStyle } from "styled-components";
// global styles that affects the whole app
// you can add more if needed
const GlobalStyle = createGlobalStyle`
  
  /* Desktop */
  .content {
    padding-top: 60px;
  }
  
  body {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
    font-family: "Roboto", sans-serif;
  }

  .link {
    text-decoration: none;
    color: inherit;
    all: unset;
  }

  *, *::before, *::after {
    box-sizing: inherit;
  }

  /* Mobile */

  @media (max-width: 768px) {
    .content {
      padding-top: 0;
      padding-bottom: 80px;
    }
  }

`;

export default GlobalStyle;
