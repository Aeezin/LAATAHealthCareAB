import { Stack, SimpleGrid, Card, Image, Text, UnstyledButton } from "@mantine/core";
import styled from "styled-components";
import axios from "axios";
import { useAuth } from "../hooks/useAuth";
import { useState, useEffect } from 'react';
import { useSearchParams } from "react-router-dom";

const GET_URL_CAREGIVER = "http://localhost:5256/api/Caregivers";
const ChooseCaregiverContainer = styled(Stack)` align-items: center;`;

const CaregiverCard = styled(UnstyledButton)` transition: transform 0.2s, box-shadow 0.2s; border-radius: 8px;    &:hover {
        transform: translateY(-4px);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    }
`;



function ChooseCaregiver() {
    const [caregivers, setCaregivers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // const {
    //     authState: { user },
    // } = useAuth();

    const [searchParams] = useSearchParams();

    useEffect(() => {
        const fetchCaregivers = async () => {
            try {
                const response = await axios.get(GET_URL_CAREGIVER, {
                    withCredentials: true,
                });
                setCaregivers(response.data);
            } catch (err) {
                setError(err.response?.data?.message || err.message);
            } finally {
                setLoading(false);
            }
        };
        fetchCaregivers();
    }, []);

    const handleCaregiverSelect = (caregiverId) => {
        navigate(`/BookingView/${caregiverId}`);
    };


    if (loading) return <p>Loading...</p>;
    if (error) return <p>Error: {error}</p>;

    return (
        <ChooseCaregiverContainer>
            <h2>Choose a Caregiver</h2>
            <SimpleGrid cols={{ base: 1, sm: 2, md: 3 }} spacing="lg">
                {caregivers.map((caregiver) => (
                    <CaregiverCard
                        key={caregiver.id}
                        onClick={() => handleCaregiverSelect(caregiver.id)}
                    >
                        <Card shadow="sm" padding="lg" radius="md" withBorder>
                            <Card.Section>
                                <Image
                                    src={caregiver.imageUrl || "/placeholder-avatar.png"}
                                    height={160}
                                    alt={caregiver.name}
                                    fallbackSrc="https://placehold.co/200x160?text=No+Image"
                                />
                            </Card.Section>
                            <Text fw={500} size="lg" mt="md" ta="center">
                                {caregiver.name}
                            </Text>
                            {caregiver.specialty && (
                                <Text size="sm" c="dimmed" ta="center">
                                    {caregiver.specialty}
                                </Text>
                            )}
                        </Card>
                    </CaregiverCard>
                ))}
            </SimpleGrid>
        </ChooseCaregiverContainer>
    );
}
export default ChooseCaregiver;
