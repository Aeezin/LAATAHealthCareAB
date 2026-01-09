namespace HealthCareAB_v1.Exceptions;

public class CaregiverScheduleNotFoundException : Exception
{
    public CaregiverScheduleNotFoundException(string message) : base(message) { }
}