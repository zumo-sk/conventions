namespace Conventions.Domain.Exceptions
{
    using System;

    public class VenueNotFoundException : Exception
    {
        public VenueNotFoundException(string message) : base(message)
        {
        }
    }
}
