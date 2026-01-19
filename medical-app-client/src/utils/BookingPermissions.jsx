export function getBookingPermissions({ role }) {
  return {
    canBook: role === "patient",
    canDelete: role === "caregiver" || role === "patient"
  };
}