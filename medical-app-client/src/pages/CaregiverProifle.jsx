import { Stack, SimpleGrid, Card, Image, Text, UnstyledButton, Button } from "@mantine/core";
import styled from "styled-components";
import axios from "axios";
import { useAuth } from "../hooks/useAuth";
import { useState, useEffect } from 'react';
import { NavLink, useSearchParams } from "react-router-dom";
import { Link } from "react-router-dom";


const GET_URL_CAREGIVERS = "http://localhost:5256/api/Caregivers";
const ChoosePatientContainer = styled(Stack)` align-items: center; padding 600px `;


const CaregiverCard = styled(UnstyledButton)` transition: transform 0.2s, box-shadow 0.2s; border-radius: 8px;    &:hover {
        transform: translateY(-4px);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    }
`;

const CaregiverProfileContainer = styled(Stack)`
    align-items: center;
    padding: 2rem;
`;



function CaregiverProfile() {
    const [caregiver, setPatient] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const { authState: { user, entityId, roles } } = useAuth();
    useEffect(() => {
        const fetchCaregiverProfile = async () => {
            if (!entityId) {
                setError("No patient ID found");
                setLoading(false);
                return;
            }
            try {
                const response = await axios.get(`${GET_URL_CAREGIVERS}/${entityId}`, {
                    withCredentials: true,
                });
                setPatient(response.data);
            } catch (err) {
                setError(err.response?.data?.message || err.message);
            } finally {
                setLoading(false);
            }
        };
        fetchCaregiverProfile();
    }, [entityId]);
    if (loading) return <p>Loading...</p>;
    if (error) return <p>Error: {error}</p>;
    if (!caregiver) return <p>No caregiver data found</p>;

    return (
        <CaregiverProfileContainer>
            <Card shadow="sm" padding="xl" radius="md" withBorder style={{ minWidth: '300px' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1.5rem' }}>
                    <Image
                        h={80}
                        w={80}
                        fit="cover"
                        radius="50%"
                        src={caregiver.profileImageUrl}
                        alt={`${caregiver.firstName} ${caregiver.lastName}`}
                        fallbackSrc="https://placehold.co/80x80?text=No+Image"
                    />
                    <div>
                        <Text fw={600} size="lg">
                            {caregiver.firstName} {caregiver.lastName}
                        </Text>
                        <Text size="sm" c="dimmed">
                            {caregiver.email}
                        </Text>
                    </div>
                </div>

                <Text size="sm" mb="sm">
                    <strong>Email:</strong> {caregiver.email || "Not available"}
                </Text>
                <Text size="sm" mb="xl">
                    <strong>Specialisation</strong> {caregiver.specialisation  || "Not available"}
                </Text>
                <Text size="sm" mb="xl">
                    <strong>Room</strong> {caregiver.room || "Not available"}
                </Text>
                <Link key={caregiver.id} to={"/booking/"} >
                    <Button fullWidth mb="sm">
                        Your Appointments
                    </Button>
                </Link>

                <Button fullWidth color="red" mb="sm" variant="outline">
                    Logout
                </Button>

            </Card>
        </CaregiverProfileContainer >
    );
}
export default CaregiverProfile;