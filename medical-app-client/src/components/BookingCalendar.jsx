import { useState, useEffect } from "react";
import styled from "styled-components";
import axios from "axios";
import { Group, Text, Button, Select, Loader } from "@mantine/core";
import BookingCalendarSection from "./BookingCalendarSection";
import BookingCalendarColumn from "./BookingCalendarColumn";

// TODO: Replace with your real API URL
const BOOKINGS_URL = "http://localhost:5256/api/Bookings/";

const BookingCalendarContainer = styled(Group)`
  align-items: stretch;
  min-height: calc(100vh - 60px);
  background-color: #8693a7;
  justify-content: space-around;
  border-top: 1px solid grey;
`;

// ----- Helper functions -----
function getMonday(d) {
  const date = new Date(d);
  const day = date.getDay(); // Sunday = 0
  const diff = day === 0 ? -6 : 1 - day;
  date.setDate(date.getDate() + diff);
  return new Date(date.getFullYear(), date.getMonth(), date.getDate());
}

function getWeekDates(monday) {
  return Array.from({ length: 5 }, (_, i) => {
    const d = new Date(monday);
    d.setDate(d.getDate() + i);
    return new Date(d.getFullYear(), d.getMonth(), d.getDate());
  });
}

function formatDate(d) {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${y}-${m}-${day}`;
}

function getWeekRangeString(weekDates) {
  const options = { month: "short", day: "numeric", year: "numeric" };
  const start = weekDates[0].toLocaleDateString("en-US", options);
  const end = weekDates[4].toLocaleDateString("en-US", options);
  return `${start} - ${end}`;
}

// ----- Main component -----
export default function BookingCalendar() {
  const today = new Date();

  // --- State ---
  const [currentWeekStart, setCurrentWeekStart] = useState(getMonday(today));
  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(true);

  const maxBookingDate = new Date();
  maxBookingDate.setDate(today.getDate() + 90);

  // --- Fetch bookings ---
  useEffect(() => {
    const fetchBookings = async () => {
      setLoading(true);
      try {
        const response = await axios.get(BOOKINGS_URL, { withCredentials: true });
        // TODO: Implement dynamic data.
        // setBookings(response.data);
      } catch (error) {
        console.error("Failed to fetch bookings:", error);
        // TODO: Remove temp static data.
        setBookings([
          {
            caregiverId: 1,
            caregiverName: "Anna Andersson",
            bookings: [
              {
                date: "2026-01-20",
                appointments: [
                  { startTime: "09:00", endTime: "09:30" },
                  { startTime: "10:00", endTime: "10:30" },
                ],
              },
              {
                date: "2026-01-29",
                appointments: [{ startTime: "14:00", endTime: "14:30" }],
              },
            ],
          },
        ]);
      } finally {
        setLoading(false);
      }
    };

    fetchBookings();
  }, []);

  // --- Derived data ---
  const allAppointments = bookings.flatMap((caregiver) =>
    caregiver.bookings.flatMap((day) =>
      day.appointments.map((appt, index) => ({
        id: `${caregiver.caregiverId}-${day.date}-${index}`,
        caregiverName: caregiver.caregiverName,
        caregiverId: caregiver.caregiverId,
        date: day.date,
        startTime: appt.startTime,
        endTime: appt.endTime,
      }))
    )
  );

  const weekDates = getWeekDates(currentWeekStart);
  const weekDateStrings = weekDates.map(formatDate);
  const weekAppointments = allAppointments.filter((appt) => weekDateStrings.includes(appt.date));

  // --- Week navigation ---
  const prevWeek = () => {
    const newMonday = new Date(currentWeekStart);
    newMonday.setDate(newMonday.getDate() - 7);

    const lowerBound = getMonday(today);
    if (newMonday >= lowerBound) setCurrentWeekStart(newMonday);
  };

  const nextWeek = () => {
    const newMonday = new Date(currentWeekStart);
    newMonday.setDate(newMonday.getDate() + 7);
    if (newMonday <= maxBookingDate) setCurrentWeekStart(newMonday);
  };

  // --- Dropdown: all Mondays ---
  const allMondays = [];
  let monday = getMonday(today);
  while (monday <= maxBookingDate) {
    allMondays.push(new Date(monday.getTime()));
    monday.setDate(monday.getDate() + 7);
  }

  // --- Render ---
  if (loading) {
    return (
      <Group position="center" style={{ minHeight: "80vh" }}>
        <Loader size="lg" variant="dots" />
      </Group>
    );
  }

  return (
    <>
      {/* Header */}
      <Group position="apart" style={{ justifyContent: "center", margin: "10px 0", marginTop: "28px", flexWrap: "wrap", gap: 10 }}>
        <Button onClick={prevWeek}>Previous Week</Button>
        <Text weight={500}>{getWeekRangeString(weekDates)}</Text>
        <Button onClick={nextWeek}>Next Week</Button>

        <Select
          style={{ minWidth: 200 }}
          placeholder="Jump to week"
          value={formatDate(currentWeekStart)}
          onChange={(val) => {
            const selected = allMondays.find((d) => formatDate(d) === val);
            selected && setCurrentWeekStart(selected);
          }}
          data={allMondays.map((d) => ({
            value: formatDate(d),
            label: getWeekRangeString(getWeekDates(d)),
          }))}
        />
      </Group>

      {/* Calendar */}
      <BookingCalendarContainer>
        {weekDates.map((date, colIndex) => {
          const weekday = date.toLocaleDateString("en-US", { weekday: "short" });
          const dateString = formatDate(date);
          const appointmentsForColumn = weekAppointments.filter((appt) => appt.date === dateString);

          return (
            <BookingCalendarColumn key={colIndex}>
              <Text weight={500}>{`${weekday} - ${dateString}`}</Text>
              {appointmentsForColumn.length > 0 ? (
                appointmentsForColumn.map((appt) => (
                  <BookingCalendarSection
                    key={appt.id}
                    booking={appt}
                    role="caregiver"
                    onBook={(b) => console.log("Book", b)}
                    onEdit={(b) => console.log("Edit", b)}
                    onDelete={(b) => console.log("Delete", b)}
                  />
                ))
              ) : (
                <Text size="xs" color="dimmed">
                  No bookings
                </Text>
              )}
            </BookingCalendarColumn>
          );
        })}
      </BookingCalendarContainer>
    </>
  );
}
