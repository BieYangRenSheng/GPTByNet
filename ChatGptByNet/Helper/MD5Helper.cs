using System.Security.Cryptography;
using System.Text;

namespace ChatGptByNet.Helper
{
    public static class MD5Helper
    {
        public static string CreateMD5(string? input)
        {
            if (input == null || string.IsNullOrEmpty(input))
                return "";
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                //return Convert.ToHexString(hashBytes); // .NET 5 and above

                StringBuilder? sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
