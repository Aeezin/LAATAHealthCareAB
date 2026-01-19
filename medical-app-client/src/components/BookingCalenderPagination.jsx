import { Pagination, Text, Group } from "@mantine/core";
import styled from "styled-components";

const DefaultBookingCalendarPagination = styled(Pagination)`
`;
const BookingCalendarPaginationContainer = styled(Group)`
  align-items: center;
  justify-content: center;
  padding: 3px;
`;


function BookingCalendarPagination() {
  return (
    <BookingCalendarPaginationContainer>
      <Text m={0}>Select Week:</Text>
      <DefaultBookingCalendarPagination total={18} />
    </BookingCalendarPaginationContainer>
  );
}

export default BookingCalendarPagination;
