namespace Conventions.Domain.Exceptions
{
    using System;

    public class TalkNotFoundException : Exception
    {
        public TalkNotFoundException(string message) : base(message)
        {
        }
    }
}
