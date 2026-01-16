export function getBookingPermissions({ booked, role, isOwnBooking }) {
  return {
    canBook: !booked && role === "patient",
    canEdit: booked && (role === "caregiver" || isOwnBooking),
    canDelete: booked && (role === "caregiver" || isOwnBooking),
    isReadOnly: booked && role === "patient" && !isOwnBooking,
  };
}
