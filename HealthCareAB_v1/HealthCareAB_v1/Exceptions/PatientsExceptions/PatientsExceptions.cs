namespace HealthCareAB_v1.Exceptions;

public class PatientNotFoundException : Exception
{
    public PatientNotFoundException()
        : base("Patient was not found.") { }

    public PatientNotFoundException(string message)
        : base(message) { }
}

public class PatientValidationException : Exception
{
    public PatientValidationException()
        : base("Patient validation failed.") { }

    public PatientValidationException(string message)
        : base(message) { }
}
