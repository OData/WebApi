using System;

namespace Nuwa.WebStack
{
    [Flags]
    public enum BrowserTypes
    {
        Firefox = 1,
        IE = 2,
        Chrome = 4,
        Safari = 8
    }
}