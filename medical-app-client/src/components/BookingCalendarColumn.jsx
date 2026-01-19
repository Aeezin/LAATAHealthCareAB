import { Stack } from "@mantine/core";
import styled from "styled-components";

const BookingCalendarColumn = styled(Stack)`
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start;
  padding: 20px;
  background-color: #f9f9f9;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  flex: 1;
  @media (max-width: 768px) {
    flex: none;
    width: 100%;
    height: auto;
    margin-bottom: 20px;
  }
`;

export default BookingCalendarColumn;
