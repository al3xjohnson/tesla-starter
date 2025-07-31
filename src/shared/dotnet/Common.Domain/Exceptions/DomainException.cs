namespace Common.Domain.Exceptions;

/// <summary>
/// Base class for all domain-specific exceptions.
/// Represents violations of business rules and domain invariants.
/// </summary>
/// <remarks>
/// Use DomainException when:
/// - A business rule is violated
/// - Domain invariants cannot be maintained
/// - An operation is not allowed in the current state
/// - Required preconditions are not met
/// 
/// Domain exceptions should:
/// - Have meaningful names that describe the business problem
/// - Include relevant context about what went wrong
/// - Be caught and handled at the application layer
/// - Be translated to appropriate HTTP responses in the API layer
/// 
/// Examples of domain exceptions:
/// - OrderCannotBeShippedException (business rule violation)
/// - InsufficientInventoryException (domain constraint)
/// - LocationAlreadyClosedException (invalid state transition)
/// 
/// This is an abstract class - create specific exception types for different
/// business rule violations rather than using this class directly.
/// </remarks>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the domain exception.
    /// </summary>
    protected DomainException() { }

    /// <summary>
    /// Initializes a new instance of the domain exception with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the business rule violation.</param>
    protected DomainException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the domain exception with a specified error message
    /// and a reference to the inner exception that caused this exception.
    /// </summary>
    /// <param name="message">The message that describes the business rule violation.</param>
    /// <param name="innerException">The exception that caused this domain exception.</param>
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}
