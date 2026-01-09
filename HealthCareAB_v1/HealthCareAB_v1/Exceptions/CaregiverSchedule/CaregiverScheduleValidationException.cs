namespace HealthCareAB_v1.Exceptions;

public class CaregiverScheduleValidationException : Exception
{
    public CaregiverScheduleValidationException(string message) : base(message) { }
}