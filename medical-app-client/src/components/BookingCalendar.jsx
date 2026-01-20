import { useState, useEffect } from "react";
import { useAuth } from "../context/AuthContext";
import { useParams } from "react-router-dom";
import axios from "axios";
import styled from "styled-components";
import { Group, Text, Button, Select, Loader, SegmentedControl } from "@mantine/core";
import BookingCalendarSection from "./BookingCalendarSection";
import BookingCalendarColumn from "./BookingCalendarColumn";

// ----- Container -----
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
  const day = date.getDay();
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
  const { authState } = useAuth();
  const role = authState.role || "patient";

  const { caregiverId } = useParams(); // optional caregiver ID

  const today = new Date();
  const maxBookingDate = new Date();
  maxBookingDate.setDate(today.getDate() + 90);

  const [currentWeekStart, setCurrentWeekStart] = useState(getMonday(today));
  const [view, setView] = useState("appointments");
  const [bookings, setBookings] = useState([]);
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);

  // ----- Fetch data using Axios -----
  useEffect(() => {
    setLoading(true);

    const fetchData = async () => {
      try {
        if (caregiverId) {
          // Fetch bookings and appointments for a specific caregiver
          const [bookingsRes, appointmentsRes] = await Promise.all([
            axios.get(`/api/bookings/${caregiverId}`, { withCredentials: true }), // cookies if used
            axios.get(`/api/appointments/${caregiverId}`, { withCredentials: true }),
          ]);

          setBookings(bookingsRes.data.bookings || []);
          setAppointments(appointmentsRes.data.appointments || []);
        } else {
          // Fetch only appointments (no caregiver selected)
          const res = await axios.get("/api/appointments", { withCredentials: true });
          setAppointments(res.data.appointments || []);
        }
      } catch (error) {
        console.error("Error fetching data:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [caregiverId]);

  // ----- Week dates -----
  const weekDates = getWeekDates(currentWeekStart);
  const weekDateStrings = weekDates.map(formatDate);

  const allBookingSlots = bookings.flatMap((caregiver) =>
    caregiver.bookings.flatMap((day) =>
      day.appointments.map((slot, index) => ({
        id: `${caregiver.caregiverId}-${day.date}-${index}`,
        caregiverId: caregiver.caregiverId,
        caregiverName: caregiver.caregiverName,
        date: day.date,
        startTime: slot.startTime,
        endTime: slot.endTime,
      }))
    )
  );

  const dataForWeek =
    view === "bookings"
      ? allBookingSlots.filter((slot) => weekDateStrings.includes(slot.date))
      : appointments.filter((appt) => weekDateStrings.includes(appt.date));

  // ----- Week navigation -----
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

  const allMondays = [];
  let monday = getMonday(today);
  while (monday <= maxBookingDate) {
    allMondays.push(new Date(monday.getTime()));
    monday.setDate(monday.getDate() + 7);
  }

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
      <Group
        position="apart"
        style={{
          justifyContent: "center",
          margin: "10px 0",
          marginTop: "28px",
          flexWrap: "wrap",
          gap: 10,
        }}
      >
        <Button
          onClick={prevWeek}
          style={{
            minWidth: "120px",
            padding: "6px 12px",
            borderRadius: "6px",
            backgroundColor: "#057d7a",
            color: "white",
            fontWeight: 500,
            transition: "all 0.2s ease",
          }}
        >
          Previous Week
        </Button>
        <Text weight={500}>{getWeekRangeString(weekDates)}</Text>
        <Button
          onClick={nextWeek}
          style={{
            minWidth: "120px",
            padding: "6px 12px",
            borderRadius: "6px",
            backgroundColor: "#057d7a",
            color: "white",
            fontWeight: 500,
            transition: "all 0.2s ease",
          }}
        >
          Next Week
        </Button>

        <Select
          style={{ minWidth: 200 }}
          placeholder="Jump to week"
          value={formatDate(currentWeekStart)}
          onChange={(val) => {
            const selected = allMondays.find((d) => formatDate(d) === val);
            if (selected) setCurrentWeekStart(selected);
          }}
          data={allMondays.map((d) => ({
            value: formatDate(d),
            label: getWeekRangeString(getWeekDates(d)),
          }))}
        />

        {/* Toggle for patients only and only if caregiverId exists */}
        {role === "patient" && caregiverId && (
          <SegmentedControl
            value={view}
            onChange={setView}
            data={[
              { label: "Available Slots", value: "bookings" },
              { label: "Appointments", value: "appointments" },
            ]}
            size="md"
          />
        )}
      </Group>

      {/* Calendar */}
      <BookingCalendarContainer>
        {weekDates.map((date, colIndex) => {
          const dateString = formatDate(date);
          const dataForColumn = dataForWeek.filter((item) => item.date === dateString);

          return (
            <BookingCalendarColumn key={colIndex}>
              <Text weight={500}>
                {`${date.toLocaleDateString("en-US", { weekday: "short" })} - ${dateString}`}
              </Text>
              {dataForColumn.length > 0 ? (
                dataForColumn.map((item) => (
                  <BookingCalendarSection
                    key={item.id}
                    booking={item}
                    role={role}
                    variant={view}
                  />
                ))
              ) : (
                <Text size="xs" c="dimmed">
                  No {view === "bookings" ? "available slots" : "appointments"}
                </Text>
              )}
            </BookingCalendarColumn>
          );
        })}
      </BookingCalendarContainer>
    </>
  );
}
