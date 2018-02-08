using System;

namespace Tortuga.Drydock.Models
{
    [Flags]
    public enum TextContentFeatures
    {
        None = 0,
        EmailAddress = 1,
        DomainUserName = 2,
        FileName = 4,
        DateTime = 8,
        Integer = 16,
        Decimal = 32,
        //NonAsciiCharacters = 64
    }
}


