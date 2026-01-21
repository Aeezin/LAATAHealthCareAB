import { useState } from "react";
import PropTypes from "prop-types";
import BookingSlot from "./BookingSlot";
import BookingActionsModal from "./BookingActionsModal";

export default function BookingCalendarSection({ booking, role, onBook, onCancel, variant }) {
  const [open, setOpen] = useState(false);

  return variant === "bookings" ? (
    <>
      <BookingSlot
        title={`Caregiver: ${booking.caregiverName}`}
        startTime={booking.startTime}
        endTime={booking.endTime}
        onClick={() => setOpen(true)}
        variant={variant}
      />

      <BookingActionsModal opened={open} onClose={() => setOpen(false)} role={role} booking={booking} variant={variant} onBook={onBook} />
    </>
  ) : (
    <>
      <BookingSlot
        title={role === "patient" ? `Caregiver: ${booking.caregiverName}` : `Patient: ${booking.patientName}`}
        startTime={booking.startTime}
        endTime={booking.endTime}
        onClick={() => setOpen(true)}
        cancelled={booking.status === 2}
        variant={variant}
      />

      <BookingActionsModal opened={open} onClose={() => setOpen(false)} role={role} booking={booking} variant={variant} onCancel={onCancel} />
    </>
  );
}

BookingCalendarSection.propTypes = {
  variant: PropTypes.oneOf(["bookings", "appointments"]).isRequired,
  booking: PropTypes.object.isRequired,
  role: PropTypes.oneOf(["patient", "caregiver"]).isRequired,
  onBook: PropTypes.func,
  onCancel: PropTypes.func,
};
