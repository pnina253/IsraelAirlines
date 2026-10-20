namespace IsraelAirlines.core.Exceptions;

// Thrown when a save fails because another request changed the same row first.
// It is a ConflictException, so the API answers 409.
public class ConcurrencyConflictException : ConflictException
{
    public ConcurrencyConflictException(string message)
        : base(message)
    {
    }
}
