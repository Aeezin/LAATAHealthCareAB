import { Stack, SimpleGrid, Card, Image, Text, UnstyledButton } from "@mantine/core";
import styled from "styled-components";
import axios from "axios";
import { useAuth } from "../hooks/useAuth";
import { useState, useEffect } from 'react';
import { NavLink, useSearchParams } from "react-router-dom";
import { Navigate } from "react-router-dom";
import caregiver1 from "../assets/Adrian.png";
import caregiver2 from "../assets/Alexander.png";
import caregiver3 from "../assets/Andreas.png";
import caregiver4 from "../assets/Ludwig.png";
import caregiver5 from "../assets/Tony.png";
import { Link } from "react-router-dom";


const GET_URL_CAREGIVER = "http://localhost:5256/api/Caregivers";
const ChooseCaregiverContainer = styled(Stack)` align-items: center; padding 600px `;
const caregiverImages = {
    4: caregiver1,
    5: caregiver2,
    6: caregiver3,
    7: caregiver4,
    8: caregiver5,
};

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


    if (loading) return <p>Loading...</p>;
    if (error) return <p>Error: {error}</p>;

    return (
        <ChooseCaregiverContainer>
            <h2>Choose a Caregiver</h2>
            <SimpleGrid style={{ padding: "30px", }} cols={{ base: 1, sm: 2, md: 5 }} spacing="lg">
                {caregivers.map((caregiver) => (
                    <Link key={caregiver.id} to={`/booking/${caregiver.id}`} >
                        <CaregiverCard
                        >
                            <Card shadow="sm" padding="lg" radius="md" withBorder>
                                <Text fw={600} size="lg" ta="center" mb="sm">
                                    {caregiver.firstName} {caregiver.lastName}
                                </Text>

                                <div style={{ display: 'flex', justifyContent: 'center', padding: '1rem' }}>
                                    <Image
                                        src={caregiverImages[caregiver.id]}
                                        h={150}
                                        w={150}
                                        fit="cover"
                                        radius="50%"
                                        alt={`${caregiver.firstName} ${caregiver.lastName}`}
                                        fallbackSrc="https://placehold.co/200x200?text=No+Image"
                                    />
                                </div>

                                <Text size="sm" c="blue" ta="center" mt="md" fw={500}>
                                    {caregiver.specialisation}
                                </Text>

                                <Text size="sm" ta="center" mt="xs">
                                    Room: {caregiver.room}
                                </Text>

                                {caregiver.bio && (
                                    <Text size="xs" c="dimmed" ta="center" mt="sm" lineClamp={3}>
                                        {caregiver.bio}
                                    </Text>
                                )}
                            </Card>
                        </CaregiverCard>
                    </Link>
                ))}
            </SimpleGrid>
        </ChooseCaregiverContainer >
    );
}
export default ChooseCaregiver;