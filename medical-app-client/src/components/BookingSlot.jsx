import { Group, Text } from "@mantine/core";
import styled from "styled-components";
import PropTypes from "prop-types";

const Slot = styled(Group)`
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  border-radius: 10px;
  width: 100%;
  padding: 6px;
  cursor: pointer;
  background-color: ${({ variant }) => (variant === "bookings" ? "#7cfc8d" : "#87cefa")};
`;

function BookingSlot({ title, startTime, endTime, onClick, variant }) {
  return (
    <Slot onClick={onClick} variant={variant}>
      <Text size="xs">{title}</Text>
      <Text size="xs">{`${startTime} - ${endTime}`}</Text>
    </Slot>
  );
}

BookingSlot.propTypes = {
  title: PropTypes.string.isRequired,
  startTime: PropTypes.string.isRequired,
  endTime: PropTypes.string.isRequired,
  onClick: PropTypes.func.isRequired,
  variant: PropTypes.oneOf(["bookings", "appointments"]).isRequired,
};

export default BookingSlot;
