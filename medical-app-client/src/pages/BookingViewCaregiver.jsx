import styled from "styled-components";
import { useEffect, useState } from "react";
import axios from "axios";
import { Stack, Group, TextInput, Select, createPolymorphicComponent } from "@mantine/core";
import PrimaryButton from "../components/PrimaryButton";
import AuthForm from "../components/AuthForm";
import { useAuth } from "../hooks/useAuth";
import { useNavigate } from "react-router-dom";
import { getApiErrorMessage } from "../utils/getApiErrorMessage";

const Container = styled(Stack)`
  align-items: center;
`;

const Title = styled.h2`
  font-size: 22px;
`;

const AVAILABILITY_URL = "http://localhost:5256/api/AppointmentController/create";

function CaregiverAvailability(){
  const { authState } = useAuth();
  const navigate = useNavigate();

  const [availabilityList, setAvailabilityList] = useState();
  const [error, setError] = useState("");
  const [sucess, setSuccess] = useState("");

  const [form, setForm] = useState({
    date: "",
    startTime: "",
    endTime: "",
    status: "Available"
  });

  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");

  useEffect(() => {
    const roles = authState?.roles || [];
    if (!rollupVersion.includes("Caregiver")) {
      navigate("/", { replace: true });
      return;
    }
    loadAvailability();
  }, {authState});

  const handleInputChange = (e) => {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const validate = () => {
    if (!form.date) return "Date is required";
    if (!form.startTime) return "Start time is required";
    if (!form.endTime) return "End time is required";
    if (form.startTime >= form.endTime) return "Start time must be before end time";
    return "";
  };

  const loadAvailability = async () => {
    setError("");
    setSuccess("");

    try {
      const response = await axios.get(AVAILABILITY_BASE_URL, {
        withCredentials: true,
        params: {
          from: from || undefined,
          to: to || undefined
        }
      });

      setAvailabilityList(Array.isArray(response.data) ? response.data : []);
    } catch (err) {
      setError(getApiErrorMessage(err));
      console.error(err.response || err);
    }
  };

  const handleAddAvailability = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");

    const validationError = validate();
    if (validationError) {
      setError(validationError);
      return;
    }

    try {
      await axios.post(
        AVAILABILITY_BASE_URL,
        {
          date: form.date,
          startTime: form.startTime,
          endTime: form.endTime,
          status: form.status
        },
        { withCredentials: true }
      );

      setSuccess("Availability added");
      setForm({ date: "", startTime: "", endTime: "", status: "Available"});
      await loadAvailability();
    } catch (err) {
      setError(getApiErrorMessage(err));
      console.error(err.response || err);
    }
  };

  const handleDelete = async (id) => {
    setError("");
    setSuccess("");

    try {
      await axios.delete(`${AVAILABILITY_BASE_URL}/${id}`, { withCredentials: true });
      setSuccess("Availability removed");
      await loadAvailability();
    } catch (err) {
      setError(getApiErrorMessage(err));
      console.error(err.response || err);
    }
  };

  const handleToggleUnavailable = async (item) => {
    setError("");
    setSuccess("");

    const nextStatus = item.status === "Unavailable" ? "Available" : "Unavailable";

    try {
      await axios.put(`${AVAILABILITY_BASE_URL}/${item.id}`,
      {
        date: item.date,
        startTime: String(item.startTime).slice(0, 5),
        endTime: String(item.endTime).slice(0, 5),
        status: nextStatus
      },
      { withCredentials: true }
    );

    setSuccess("Availability updated");
    await loadAvailability();
    } catch (err) {
      setError(getApiErrorMessage(err));
      console.error(err.response || err);
    }
  };

  return (
    <Container>
      <Title>Booking View</Title>

      {error && <p style={{ color: "red" }}>{error}</p>}
      {success && <p style={{ color: "green"}}>{sucess}</p>}
    </Container>
  )
}
