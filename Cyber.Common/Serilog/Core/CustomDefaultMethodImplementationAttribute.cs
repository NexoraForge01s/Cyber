#if FEATURE_DEFAULT_INTERFACE

using System;

namespace Cyber
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
    sealed class CustomDefaultMethodImplementationAttribute : Attribute
    {
    }
}

#endif
