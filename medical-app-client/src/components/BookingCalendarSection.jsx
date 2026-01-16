import { useState } from "react";
import PropTypes from "prop-types";
import BookingSlot from "./BookingSlot";
import BookingActionsModal from "./BookingActionsModal";

function BookingCalendarSection({
  role,
  booking,
  isOwnBooking = false,
  onBook,
  onEdit,
  onDelete,
}) {
  const [open, setOpen] = useState(false);

  const title =
    role === "caregiver" && booking.booked
      ? `Patient: ${booking.patientName}`
      : `Caregiver: ${booking.caregiverName}`;

  return (
    <>
      <BookingSlot
        booked={booking.booked}
        title={title}
        time={booking.time}
        onClick={() => setOpen(true)}
      />

      <BookingActionsModal
        opened={open}
        onClose={() => setOpen(false)}
        role={role}
        booking={booking}
        isOwnBooking={isOwnBooking}
        onBook={onBook}
        onEdit={onEdit}
        onDelete={onDelete}
      />
    </>
  );
}

BookingCalendarSection.propTypes = {
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

export default BookingCalendarSection;