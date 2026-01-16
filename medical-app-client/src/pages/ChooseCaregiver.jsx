import { Stack } from "@mantine/core";
import styled from "styled-components";
import axios from "axios";
import { useAuth } from "../hooks/useAuth";
import { useState, useEffect } from 'react';
import { useSearchParams } from "react-router-dom";
const GET_URL_CAREGIVER = "http://localhost:5256/api/Caregivers";
const ChooseCaregiverContainer = styled(Stack)` align-items: center;`;

function ChooseCaregiver() {
    const [caregivers, setCaregivers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const {
        authState: { user },
    } = useAuth();
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
            {caregivers.map((caregiver) => (
                <div key={caregiver.id}>
                    {caregiver.name}
                </div>
            ))}
        </ChooseCaregiverContainer>
    );
}
export default ChooseCaregiver;
