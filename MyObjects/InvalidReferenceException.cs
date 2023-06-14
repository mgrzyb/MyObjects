using System;

namespace MyObjects
{
    public class InvalidReferenceException : Exception
    {
        public InvalidReferenceException(IReference reference)
        {
            this.Reference = reference;
        }

        public IReference Reference { get; }
    }

}