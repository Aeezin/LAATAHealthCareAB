namespace HealthCareAB_v1.Exceptions;

public class AppointmentNotFoundException : Exception
{
    public AppointmentNotFoundException(string message) : base(message) { }
}