using System;
using System.Runtime.ConstrainedExecution;
using System.Security;

namespace ReactiveXComponent.Common
{
    /// <summary>
    /// Generate platform independant hashcode
    /// </summary>
    public static class HashcodeHelper
    {

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [SecuritySafeCritical]
        public static unsafe int GetXcHashCode(string s)
        {
            fixed (void* str = s)
            {
                int* numPtr = (int*)str;
                int num = 0x15051505;
                int num2 = num;
                for (int i = s.Length; i > 0; i -= 4)
                {
                    num = (((num << 5) + num) + (num >> 0x1b)) ^ numPtr[0];
                    if (i <= 2)
                    {
                        break;
                    }
                    num2 = (((num2 << 5) + num2) + (num2 >> 0x1b)) ^ numPtr[1];
                    numPtr += 2;
                }
                return (num + (num2 * 0x5d588b65));
            }
        }

        [SecuritySafeCritical]
        public static unsafe int GetXcHashCode(double num1)
        {
            if (num1 == 0.0)
                return 0;
            long num2 = *(long*)&num1;
            return (int)num2 ^ (int)(num2 >> 32);
        }

        public static int GetXcHashCode(int s)
        {
            return s;
        }

        public static int GetXcHashCode(Int64 s)
        {
            return (int)s ^ (int)(s >> 32);
        }

        public static int GetXcHashCode(bool s)
        {
            return !s ? 0 : 1;
        }

        public static int GetXcHashCode(DateTime s)
        {
            long internalTicks = s.Ticks;
            return (int)internalTicks ^ (int)(internalTicks >> 32);
        }

        public static int GetXcHashCode(object s)
        {
            if (s is int)
            {
                return GetXcHashCode((int)s);
            }

            var valStr = s as string;
            if (valStr != null)
            {
                return GetXcHashCode(valStr);
            }

            if (s is Int64)
            {
                return GetXcHashCode((Int64)s);
            }

            if (s is bool)
            {
                return GetXcHashCode((bool)s);
            }

            if (s is double)
            {
                return GetXcHashCode((double)s);
            }

            if (s is DateTime)
            {
                return GetXcHashCode((DateTime)s);
            }

            return 17;
        }

    }
}
