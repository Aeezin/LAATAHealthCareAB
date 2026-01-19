export function getBookingPermissions({ booked, role }) {
  return {
    canBook: !booked && role === "patient",
    canEdit: booked && role === "caregiver",
    canDelete: booked && (role === "caregiver" || role === "patient"),
  };
}