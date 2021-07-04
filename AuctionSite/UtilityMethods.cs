using System;

namespace WU.Utilities {
    class UtilityMethods {
        public static void CheckNullArgument(Object obj, string paramName) {
            if (obj == null)
                throw new ArgumentNullException(paramName);
        }

        public static void CheckStringLength(string str, string paramName, int minInclusive, int maxInclusive)
        {
            if(str.Length < minInclusive || str.Length > maxInclusive)
                throw new ArgumentException(paramName);
        }

        public static void CheckNumberOutOfRange(double number, string paramName, double minInclusive, double maxInclusive) {
            if (number < minInclusive || number > maxInclusive)
                throw new ArgumentOutOfRangeException(paramName);
        }


    }
}
