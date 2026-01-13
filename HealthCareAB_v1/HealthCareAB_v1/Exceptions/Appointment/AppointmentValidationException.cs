namespace HealthCareAB_v1.Exceptions;

public class AppointmentValidationException : Exception
{
    public AppointmentValidationException(string message) : base(message) { }
}