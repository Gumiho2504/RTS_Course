namespace Gumiho_Rts.TechTree
{
    using System;
    class InvalidPathSpecifiedException : Exception
    {
        public InvalidPathSpecifiedException(string attributeName) : base($"{attributeName} does not exitst at the provided path!") { }
    }
}