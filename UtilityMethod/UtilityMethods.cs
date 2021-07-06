using System;
using System.Security.Cryptography;
using System.Text;

namespace WU.Utilities {
    public class UtilityMethods {
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
        
        public static string GetHashString(string inputString) {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();

            byte[] GetHash(string inString) {
                using (HashAlgorithm algorithm = SHA256.Create())
                    return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inString));
            }
        }
    }
}
