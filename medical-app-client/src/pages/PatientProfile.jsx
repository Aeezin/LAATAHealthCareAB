import { Stack, SimpleGrid, Card, Image, Text, UnstyledButton, Button } from "@mantine/core";
import styled from "styled-components";
import axios from "axios";
import { useAuth } from "../hooks/useAuth";
import { useState, useEffect } from 'react';
import { Link } from "react-router-dom";
import { useNavigate } from "react-router-dom";


const GET_URL_PATIENTS = "http://localhost:5256/api/Patients";
const ChoosePatientContainer = styled(Stack)` align-items: center; padding 600px `;


const PatientCard = styled(UnstyledButton)` transition: transform 0.2s, box-shadow 0.2s; border-radius: 8px;    &:hover {
        transform: translateY(-4px);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    }
`;

const PatientProfileContainer = styled(Stack)`
    align-items: center;
    padding: 2rem;
`;



function PatientProfile() {
    const [patient, setPatient] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const navigate = useNavigate();
    const { logout } = useAuth();

    const { authState: { user, entityId, roles } } = useAuth();
    useEffect(() => {
        const fetchPatientProfile = async () => {
            if (!entityId) {
                setError("No patient ID found");
                setLoading(false);
                return;
            }
            try {
                const response = await axios.get(`${GET_URL_PATIENTS}/${entityId}`, {
                    withCredentials: true,
                });
                setPatient(response.data);
            } catch (err) {
                setError(err.response?.data?.message || err.message);
            } finally {
                setLoading(false);
            }
        };
        fetchPatientProfile();
    }, [entityId]);
    if (loading) return <p>Loading...</p>;
    if (error) return <p>Error: {error}</p>;
    if (!patient) return <p>No patient data found</p>;

    const handleLogout = async () => {
        try {
            await axios.post(
                "http://localhost:5256/api/auth/logout",
                {},
                { withCredentials: true }
            );

            logout();
            navigate("/");
        } catch (err) {
            console.error("Logout failed:", err);
        }
    };

    return (
        <PatientProfileContainer>
            <Card shadow="sm" padding="xl" radius="md" withBorder style={{ minWidth: '300px' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1.5rem' }}>
                    <Image
                        h={80}
                        w={80}
                        fit="cover"
                        radius="50%"
                        src={patient.profileImageUrl}
                        alt={`${patient.firstName} ${patient.lastName}`}
                        fallbackSrc="https://placehold.co/80x80?text=No+Image"
                    />
                    <div>
                        <Text fw={600} size="lg">
                            {patient.firstName} {patient.lastName}
                        </Text>
                        <Text size="sm" c="dimmed">
                            {patient.email}
                        </Text>
                    </div>
                </div>



                <Text size="sm" mb="sm">
                    <strong>Phone:</strong> {patient.phoneNumber || "Not available"}
                </Text>
                <Text size="sm" mb="sm">
                    <strong>Email:</strong> {patient.email || "Not available"}
                </Text>
                <Text size="sm" mb="xl">
                    <strong>Date of birth:</strong> {patient.dateOfBirth || "Not available"}
                </Text>
                <Link key={patient.id} to={"/booking/"} >
                    <Button fullWidth mb="sm">
                        Your Appointments
                    </Button>
                </Link>

                <Button fullWidth mb="sm">
                    Edit Profile
                </Button>
                <Button
                    fullWidth
                    color="red"
                    mb="sm"
                    variant="outline"
                    onClick={handleLogout}
                >
                    Logout
                </Button>

                <Button fullWidth color="red" mb="sm" variant="outline">
                    Remove Account
                </Button>
            </Card>
        </PatientProfileContainer >
    );
}
export default PatientProfile;