namespace Conventions.Domain.Exceptions
{
    using System;

    public class NotPartOfConventionException : Exception
    {
        public NotPartOfConventionException(string message) : base(message)
        {
        }
    }
}
