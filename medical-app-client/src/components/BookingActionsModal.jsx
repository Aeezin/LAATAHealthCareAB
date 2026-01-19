import { Modal, Stack, Button, Group, Text } from "@mantine/core";
import PropTypes from "prop-types";
import { getBookingPermissions } from "../utils/bookingPermissions";

function BookingActionsModal({ opened, onClose, role, booking, onBook, onEdit, onDelete }) {
  const permissions = getBookingPermissions({ booked: booking.booked, role });

  // Compute time string from start/end
  const time = `${booking.startTime} - ${booking.endTime}`;

  // Title changes depending on role
  const title =
    booking.booked && role === "caregiver"
      ? `Patient: ${booking.patientName || "Unknown"}`
      : `Caregiver: ${booking.caregiverName}`;

  return (
    <Modal opened={opened} onClose={onClose} title="Booking Details" centered>
      <Stack gap="md">
        <Text>{title}</Text>
        <Text>Date: {booking.date}</Text>
        <Text>Time: {time}</Text>
        {permissions.canBook && (
          <Button color="green" onClick={() => onBook?.(booking)}>
            Book
          </Button>
        )}
        {(permissions.canEdit || permissions.canDelete) && (
          <Group grow>
            {permissions.canEdit && <Button onClick={() => onEdit?.(booking)}>Edit</Button>}
            {permissions.canDelete && (
              <Button
                color="red"
                onClick={() => {
                  if (window.confirm("Delete booking?")) {
                    onDelete?.(booking);
                  }
                }}
              >
                Delete
              </Button>
            )}
          </Group>
        )}
      </Stack>
    </Modal>
  );
}

BookingActionsModal.propTypes = {
  opened: PropTypes.bool.isRequired,
  onClose: PropTypes.func.isRequired,
  role: PropTypes.oneOf(["patient", "caregiver"]).isRequired,
  booking: PropTypes.shape({
    bookingId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    startTime: PropTypes.string.isRequired,
    endTime: PropTypes.string.isRequired,
    caregiverName: PropTypes.string.isRequired,
    patientName: PropTypes.string,
    booked: PropTypes.bool,
    date: PropTypes.string.isRequired,
  }).isRequired,
  onBook: PropTypes.func,
  onEdit: PropTypes.func,
  onDelete: PropTypes.func,
};

export default BookingActionsModal;
