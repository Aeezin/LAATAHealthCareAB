import { Modal, Stack, Button, Text, Textarea } from "@mantine/core";
import { useEffect, useState } from "react";
import PropTypes from "prop-types";

function BookingActionsModal({
  opened,
  onClose,
  role,
  booking,
  variant,
  onBook,
  onCancel,
}) {
  const time = `${booking.startTime} - ${booking.endTime}`;

  const bookingStatus =
    booking.status === 0
      ? "Scheduled"
      : booking.status === 1
      ? "Completed"
      : "Cancelled";

  // Local editable copy of booking (never mutate props)
  const [editableBooking, setEditableBooking] = useState(booking);

  // Keep state in sync when booking changes
  useEffect(() => {
    setEditableBooking(booking);
  }, [booking]);

  return (
    <Modal
      opened={opened}
      onClose={onClose}
      title={variant === "bookings" ? "Available Slot" : "Appointment Details"}
      centered
    >
      <Stack gap="md">
        <Text fw={600}>
          {variant === "bookings"
            ? `Caregiver: ${editableBooking.caregiverName}`
            : `Patient: ${editableBooking.patientName} | Caregiver: ${editableBooking.caregiverName}`}
        </Text>

        <Text>Date: {editableBooking.date}</Text>
        <Text>Time: {time}</Text>

        {/* Patient notes (editable when booking) */}
        {variant === "bookings" && (
          <Textarea
            label="Additional Notes"
            placeholder="Add your additional notes here..."
            value={editableBooking.patientNotes ?? ""}
            onChange={(event) => {
              const value = event.target.value;

              setEditableBooking((prev) => ({
                ...prev,
                patientNotes: value,
              }));
            }}
          />
        )}

        {/* Read-only appointment info */}
        {variant === "appointments" && editableBooking.room && (
          <Text>Room: {editableBooking.room}</Text>
        )}

        {variant === "appointments" && editableBooking.patientNotes && (
          <Text>Patient Notes: {editableBooking.patientNotes}</Text>
        )}

        {variant === "appointments" && editableBooking.caregiverNotes && (
          <Text>Caregiver Notes: {editableBooking.caregiverNotes}</Text>
        )}

        {variant === "appointments" &&
          editableBooking.status != null && (
            <Text>Status: {bookingStatus}</Text>
          )}

        {/* Actions */}
        {variant === "bookings" && (
          <Button
            color="green"
            onClick={() => onBook?.(editableBooking)}
          >
            Book
          </Button>
        )}

        {variant === "appointments" &&
          (role === "patient" || role === "caregiver") && (
            <Button
              color="red"
              onClick={() => onCancel?.(editableBooking)}
            >
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
