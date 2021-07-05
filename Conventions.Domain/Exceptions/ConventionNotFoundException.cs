namespace Conventions.Domain.Exceptions
{
    using System;

    public class ConventionNotFoundException : Exception
    {
        public ConventionNotFoundException(string message) : base(message)
        {
        }
    }
}
