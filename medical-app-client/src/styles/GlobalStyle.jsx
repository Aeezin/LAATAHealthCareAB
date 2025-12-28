import { createGlobalStyle } from "styled-components";
// global styles that affects the whole app
// you can add more if needed
const GlobalStyle = createGlobalStyle`

  body {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
    font-family: "Roboto", sans-serif;
    width: 100vw;
    height: 100vh;
  }

  .content {
  margin: 1rem;
  }

  .link {
   text-decoration: none;
    color: inherit;
    all: unset;
  }

  *, *::before, *::after {
    box-sizing: inherit;
  }

`;

export default GlobalStyle;
