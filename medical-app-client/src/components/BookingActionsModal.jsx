import { Modal, Stack, Button, Text } from "@mantine/core";
import PropTypes from "prop-types";

function BookingActionsModal({ opened, onClose, role, booking, variant, onBook, onCancel }) {
  const time = `${booking.startTime} - ${booking.endTime}`;

  return (
    <Modal opened={opened} onClose={onClose} title={variant === "bookings" ? "Available Slot" : "Appointment Details"} centered>
      <Stack gap="md">
        <Text weight={600}>
          {variant === "bookings" ? `Caregiver: ${booking.caregiverName}` : `Patient: ${booking.patientName} | Caregiver: ${booking.caregiverName}`}
        </Text>

        <Text>Date: {booking.date}</Text>
        <Text>Time: {time}</Text>

        {variant === "appointments" && booking.room && <Text>Room: {booking.room}</Text>}
        {variant === "appointments" && booking.patientNotes && <Text>Patient Notes: {booking.patientNotes}</Text>}
        {variant === "appointments" && booking.caregiverNotes && <Text>Caregiver Notes: {booking.caregiverNotes}</Text>}
        {variant === "appointments" && booking.status && <Text>Status: {booking.status}</Text>}

        {variant === "bookings" && (
          <Button color="green" onClick={() => onBook?.(booking)}>
            Book
          </Button>
        )}

        {variant === "appointments" && (role === "patient" || role === "caregiver") && (
          <Button color="red" onClick={() => onCancel?.(booking)}>
            Cancel Appointment
          </Button>
        )}
      </Stack>
    </Modal>
  );
}

BookingActionsModal.propTypes = {
  opened: PropTypes.bool.isRequired,
  onClose: PropTypes.func.isRequired,
  role: PropTypes.oneOf(["patient", "caregiver"]).isRequired,
  booking: PropTypes.object.isRequired,
  variant: PropTypes.oneOf(["bookings", "appointments"]).isRequired,
  onBook: PropTypes.func,
  onCancel: PropTypes.func,
};

export default BookingActionsModal;
