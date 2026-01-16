import { useState } from "react";
import PropTypes from "prop-types";
import BookingSlot from "./BookingSlot";
import BookingActionsModal from "./BookingActionsModal";

export default function BookingCalendarSection({ booking, role, onBook, onEdit, onDelete }) {
  const [open, setOpen] = useState(false);

  return (
    <>
      <BookingSlot
        title={`Caregiver: ${booking.caregiverName}`}
        startTime={booking.startTime}
        endTime={booking.endTime}
        onClick={() => setOpen(true)}
      />

      <BookingActionsModal
        opened={open}
        onClose={() => setOpen(false)}
        role={role}
        booking={booking}
        onBook={onBook}
        onEdit={onEdit}
        onDelete={onDelete}
      />
    </>
  );
}

BookingCalendarSection.propTypes = {
  booking: PropTypes.shape({
    bookingId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    startTime: PropTypes.string.isRequired,
    endTime: PropTypes.string.isRequired,
    caregiverName: PropTypes.string.isRequired,
    date: PropTypes.string.isRequired,
  }).isRequired,
  role: PropTypes.oneOf(["patient", "caregiver"]).isRequired,
  onBook: PropTypes.func,
  onEdit: PropTypes.func,
  onDelete: PropTypes.func,
};
