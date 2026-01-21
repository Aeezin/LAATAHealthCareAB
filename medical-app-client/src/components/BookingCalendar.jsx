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

function normalizeAvailableSlotsResponse(apiData) {
  if (!apiData || !Array.isArray(apiData.availableSlots)) return [];

  return [
    {
      caregiverId: apiData.caregiverId,
      caregiverName: apiData.caregiverName,
      bookings: apiData.availableSlots.map((day) => ({
        date: day.date.split("T")[0], // ISO → YYYY-MM-DD
        appointments: day.timeSlots.map((slot) => ({
          startTime: slot.startTime,
          endTime: slot.endTime,
        })),
      })),
    },
  ];
}

function normalizeAppointmentsResponse(apiData) {
  if (!apiData) return [];

  if (Array.isArray(apiData)) return apiData;
  if (Array.isArray(apiData.appointments)) return apiData.appointments;

  console.warn("Unexpected appointments response shape:", apiData);
  return [];
}

// ----- Main component -----
export default function BookingCalendar() {
  const { authState } = useAuth();
  const role = authState.role || "patient";
  const userId = authState.user.id;
  console.log(authState.user)

  const { caregiverId } = useParams();

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
          const [bookingsRes, appointmentsRes] = await Promise.all([
            axios.get("http://localhost:5256/api/Appointments/available-slots", {
              params: {
                CaregiverId: caregiverId,
                StartDate: today.toISOString(),
                EndDate: maxBookingDate.toISOString(),
              },
              withCredentials: true,
            }),
            axios.get("http://localhost:5256/api/appointments", { withCredentials: true }),
          ]);

          const normalizedBookings = normalizeAvailableSlotsResponse(bookingsRes.data);
          const normalizedAppointments = normalizeAppointmentsResponse(appointmentsRes.data);

          setBookings(normalizedBookings);
          setAppointments(normalizedAppointments);
        } else {
          const res = await axios.get("/api/appointments", { withCredentials: true });
          setAppointments(normalizeAppointmentsResponse(res.data));
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
      })),
    ),
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

  // ----- Appointment API calls -----
  const createAppointment = async (slot) => {
    try {
      const payload = {
        patientId: userId,        // logged-in patient
        caregiverId: slot.caregiverId,
        date: slot.date,          // "YYYY-MM-DD"
        startTime: slot.startTime,
        endTime: slot.endTime,
        patientNotes: null,
      };

      const res = await axios.post(
        "http://localhost:5256/api/appointments",
        payload,
        { withCredentials: true }
      );

      // Add to local state
      setAppointments((prev) => [...prev, res.data]);
      alert("Appointment created successfully!");
    } catch (err) {
      console.error("Failed to create appointment", err);
      alert(err.response?.data?.error ?? "Could not create appointment");
    }
  };

  const cancelAppointment = async (appointmentId) => {
    try {
      const res = await axios.put(
        `http://localhost:5256/api/appointments/cancel/${appointmentId}`,
        {},
        { withCredentials: true }
      );

      // Update local state
      setAppointments((prev) =>
        prev.map((a) =>
          a.id === appointmentId
            ? { ...a, status: res.data.status }
            : a
        )
      );
      alert("Appointment cancelled successfully!");
    } catch (err) {
      console.error("Failed to cancel appointment", err);
      alert(err.response?.data?.message ?? "Could not cancel appointment");
    }
  };

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
      <Group position="apart" style={{ justifyContent: "center", margin: "28px 0 10px", flexWrap: "wrap", gap: 10 }}>
        <Button onClick={prevWeek}>Previous Week</Button>
        <Text weight={500}>{getWeekRangeString(weekDates)}</Text>
        <Button onClick={nextWeek}>Next Week</Button>

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

        {role === "patient" && caregiverId && (
          <SegmentedControl
            value={view}
            onChange={setView}
            data={[
              { label: "Available Slots", value: "bookings" },
              { label: "Appointments", value: "appointments" },
            ]}
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
              <Text weight={500}>{`${date.toLocaleDateString("en-US", { weekday: "short" })} - ${dateString}`}</Text>

              {dataForColumn.length > 0 ? (
                dataForColumn.map((item) => (
                  <BookingCalendarSection
                    key={item.id}
                    booking={item}
                    role={role}
                    variant={view}
                    onBook={() => createAppointment(item)}
                    onCancel={() => cancelAppointment(item.id)}
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
