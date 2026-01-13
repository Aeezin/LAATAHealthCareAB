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
    height: 100%;
  }

  .content {
    margin: 0;
  }
    /* Desktop */
  .content {
    padding-top: 60px;
    height: calc(100vh - 60px);
    padding-bottom: 0;
  }

  /* Mobile */
  @media (max-width: 768px) {
    .content {
      padding-top: 0;
      padding-bottom: 150px;
      // height: calc(100vh - 150px);
    }
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
