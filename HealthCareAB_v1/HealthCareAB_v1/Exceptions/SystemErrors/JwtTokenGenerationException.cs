namespace HealthCareAB_v1.Exceptions;
public class JwtTokenGenerationException : Exception
{
    public JwtTokenGenerationException(string message)
        : base(message) { }
}