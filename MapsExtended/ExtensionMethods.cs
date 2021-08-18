using System;
using ExceptionDispatchInfo = System.Runtime.ExceptionServices.ExceptionDispatchInfo;

namespace MapsExt
{
    public static class ExtensionMethods
    {
        public static void Rethrow(this Exception ex)
        {
            ExceptionDispatchInfo.Capture(ex).Throw();
        }
    }
}
