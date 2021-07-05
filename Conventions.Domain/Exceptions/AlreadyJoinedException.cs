namespace Conventions.Domain.Exceptions
{
    using System;

    public class AlreadyJoinedException : Exception
    {
        public AlreadyJoinedException(string message) : base(message)
        {
        }
    }
}
