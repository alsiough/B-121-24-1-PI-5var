using System;

namespace JobPortal.BLL.Infrastructure
{
    public class AccessDeniedException : Exception
    {
        public AccessDeniedException(string message) : base(message) { }
    }
}