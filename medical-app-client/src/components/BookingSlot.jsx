import { Group, Text } from "@mantine/core";
import styled from "styled-components";
import PropTypes from "prop-types";

const Slot = styled(Group)`
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  background-color: #7cfc8d;
  border-radius: 10px;
  width: 100%;
  padding: 6px;
  cursor: pointer;
`;

function BookingSlot({ title, startTime, endTime, onClick }) {
  return (
    <Slot onClick={onClick}>
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
};

export default BookingSlot;
