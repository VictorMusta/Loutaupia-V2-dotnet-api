namespace Loutaupia_V2_dotnet_api.Core.Domain.Exceptions;
public class DomainException : System.Exception
{
    public DomainException(string message) : base(message)
    {
    }
    public DomainException(string message, System.Exception innerException) : base(message, innerException)
    {
    }
}
