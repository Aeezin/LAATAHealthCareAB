import styled from "styled-components";
import { Group } from "@mantine/core";
import BookingCalendarSection from "../components/BookingCalendarSection";
import BookingCalendarColumn from "../components/BookingCalendarColumn";

const BookingPatientContainer = styled(Group)`
  align-items: center;
  height: calc(100vh - 60px);
  background-color: #8693a7;
  justify-content: space-around;
`;

function BookingPatient() {
  const currentUserName = "Andreas";

  const bookings = [
    {
      bookingId: 1,
      time: "09:00-09:30",
      caregiverName: "Anna",
      booked: false,
    },
    {
      bookingId: 2,
      time: "09:30-10:00",
      caregiverName: "Anna",
      patientName: "Andreas",
      booked: true,
    },
    {
      bookingId: 3,
      time: "10:00-10:30",
      caregiverName: "Anna",
      patientName: "Someone else",
      booked: true,
    },
  ];

  return (
    <BookingPatientContainer>
      <BookingCalendarColumn>
        {bookings.map((booking) => (
          <BookingCalendarSection
            key={booking.bookingId}
            role="patient"
            booking={booking}
            isOwnBooking={booking.patientName === currentUserName}
            onBook={(b) => console.log("Book", b)}
            onEdit={(b) => console.log("Edit", b)}
            onDelete={(b) => console.log("Delete", b)}
          />
        ))}
      </BookingCalendarColumn>
      <BookingCalendarColumn>
        {bookings.map((booking) => (
          <BookingCalendarSection
            key={booking.bookingId}
            role="patient"
            booking={booking}
            isOwnBooking={booking.patientName === currentUserName}
            onBook={(b) => console.log("Book", b)}
            onEdit={(b) => console.log("Edit", b)}
            onDelete={(b) => console.log("Delete", b)}
          />
        ))}
      </BookingCalendarColumn>
      <BookingCalendarColumn>
        {bookings.map((booking) => (
          <BookingCalendarSection
            key={booking.bookingId}
            role="patient"
            booking={booking}
            isOwnBooking={booking.patientName === currentUserName}
            onBook={(b) => console.log("Book", b)}
            onEdit={(b) => console.log("Edit", b)}
            onDelete={(b) => console.log("Delete", b)}
          />
        ))}
      </BookingCalendarColumn>
      <BookingCalendarColumn>
        {bookings.map((booking) => (
          <BookingCalendarSection
            key={booking.bookingId}
            role="patient"
            booking={booking}
            isOwnBooking={booking.patientName === currentUserName}
            onBook={(b) => console.log("Book", b)}
            onEdit={(b) => console.log("Edit", b)}
            onDelete={(b) => console.log("Delete", b)}
          />
        ))}
      </BookingCalendarColumn>
      <BookingCalendarColumn>
        {bookings.map((booking) => (
          <BookingCalendarSection
            key={booking.bookingId}
            role="patient"
            booking={booking}
            isOwnBooking={booking.patientName === currentUserName}
            onBook={(b) => console.log("Book", b)}
            onEdit={(b) => console.log("Edit", b)}
            onDelete={(b) => console.log("Delete", b)}
          />
        ))}
      </BookingCalendarColumn>
    </BookingPatientContainer>
  );
}

export default BookingPatient;
