using System;

namespace JobPortal.BLL.Infrastructure
{
    public class ValidationException : Exception
    {
        public string Property { get; private set; }
        public ValidationException(string message, string property) : base(message)
        {
            Property = property;
        }
    }
}