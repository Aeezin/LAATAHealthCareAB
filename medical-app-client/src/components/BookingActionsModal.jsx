import { Modal, Button, Stack, Group, Text } from "@mantine/core";
import PropTypes from "prop-types";
import { getBookingPermissions } from "./bookingPermissions";

function BookingActionsModal({
  opened,
  onClose,
  role,
  booking,
  isOwnBooking,
  onBook,
  onEdit,
  onDelete,
}) {
  const permissions = getBookingPermissions({
    booked: booking.booked,
    role,
    isOwnBooking,
  });

  return (
    <Modal opened={opened} onClose={onClose} title="Booking details" centered>
      <Stack gap="md">
        {booking.patientName && <Text>Patient: {booking.patientName}</Text>}
        {booking.caregiverName && (
          <Text>Caregiver: {booking.caregiverName}</Text>
        )}
        <Text>Time: {booking.time}</Text>

        {permissions.canBook && (
          <Button color="green" onClick={() => onBook?.(booking)}>
            Book
          </Button>
        )}

        {(permissions.canEdit || permissions.canDelete) && (
          <Group grow>
            {permissions.canEdit && (
              <Button onClick={() => onEdit?.(booking)}>Edit</Button>
            )}
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

        {permissions.isReadOnly && (
          <Text c="dimmed">This slot is already booked</Text>
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
    time: PropTypes.string.isRequired,
    patientName: PropTypes.string,
    caregiverName: PropTypes.string,
    booked: PropTypes.bool.isRequired,
  }).isRequired,
  isOwnBooking: PropTypes.bool,
  onBook: PropTypes.func,
  onEdit: PropTypes.func,
  onDelete: PropTypes.func,
};

export default BookingActionsModal;
