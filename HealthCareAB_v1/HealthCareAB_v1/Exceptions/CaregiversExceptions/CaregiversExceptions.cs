namespace HealthCareAB_v1.Exceptions;

public class CaregiverNotFoundException : Exception
{
    public CaregiverNotFoundException() : base("Caregiver was not found.")
    {
    }

    public CaregiverNotFoundException(string message) : base(message)
    {
    }
}

public class CaregiverValidationException : Exception
{
    public CaregiverValidationException() : base("Caregiver validation failed.")
    {
    }

    public CaregiverValidationException(string message) : base(message)
    {
    }
}
