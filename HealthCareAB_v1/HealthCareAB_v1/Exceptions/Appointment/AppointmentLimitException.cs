namespace HealthCareAB_v1.Exceptions;

public class AppointmentLimitException : Exception
{
    public AppointmentLimitException(string message) : base(message) { }
}