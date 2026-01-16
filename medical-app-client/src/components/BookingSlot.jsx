import { Group, Text } from "@mantine/core";
import styled from "styled-components";
import PropTypes from "prop-types";

const Slot = styled(Group)`
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  background-color: ${(p) => (p.booked ? "#ff6b6b" : "#7cfc8d")};
  border-radius: 10px;
  width: 100%;
  padding: 6px;
  cursor: pointer;
`;

function BookingSlot({ title, time, booked, onClick }) {
  return (
    <Slot booked={booked} onClick={onClick}>
      <Text size="xs">{title}</Text>
      <Text size="xs">Time: {time}</Text>
    </Slot>
  );
}

BookingSlot.propTypes = {
  title: PropTypes.string.isRequired,
  time: PropTypes.string.isRequired,
  booked: PropTypes.bool.isRequired,
  onClick: PropTypes.func.isRequired,
};

export default BookingSlot;
