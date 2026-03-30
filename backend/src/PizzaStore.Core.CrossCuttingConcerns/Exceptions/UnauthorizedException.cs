namespace PizzaStore.Core.CrossCuttingConcerns.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
