using System;
using GameSDK.Core.PropertyAttributes;

namespace GameSDK.Localization
{
    [Serializable]
    public struct Language
    {
        [ReadOnly] public string Code;
        [ReadOnly] public string Name;
        [ReadOnly] public string NativeName;
    }
}