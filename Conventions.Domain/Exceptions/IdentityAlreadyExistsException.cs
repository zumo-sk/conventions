namespace Conventions.Domain.Exceptions
{
    using System;

    public class IdentityAlreadyExistsException : Exception
    {
        public IdentityAlreadyExistsException(string message) : base(message)
        {
        }
    }
}
