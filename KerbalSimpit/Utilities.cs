using System;
using System.Runtime.InteropServices;

namespace KerbalSimpit.Utilities
{
    public class KerbalSimpitUtils
    {
        // https://stackoverflow.com/questions/2871/reading-a-c-c-data-structure-in-c-sharp-from-a-byte-array/41836532#41836532
        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                                                   typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
