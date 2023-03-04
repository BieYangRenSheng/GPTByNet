using System.Security.Cryptography;
using System.Text;

namespace ChatGptByNet.Helper
{
    public static class EncryptAndDecryptHelper
    {
        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string EncryptString(string? plainText, string? password)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(password))
            {
                return "";
            }
            // 创建一个加密类
            using (var aes = new AesCryptoServiceProvider())
            {
                // 用密码生成一个密钥和一个初始化向量
                var pdb = new Rfc2898DeriveBytes(password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64 });
                aes.Key = pdb.GetBytes(32);
                aes.IV = pdb.GetBytes(16);

                // 创建一个加密器对象
                using (var encryptor = aes.CreateEncryptor())
                {
                    // 将明文转换为字节数组
                    var plainBytes = Encoding.Unicode.GetBytes(plainText);

                    // 对字节数组进行加密，并返回加密后的字符串
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(plainBytes, 0, plainBytes.Length);
                            cs.Close();
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string DecryptString(string cipherText, string password)
        {
            if (string.IsNullOrEmpty(cipherText) || string.IsNullOrEmpty(password))
            {
                return "";
            }
            // 创建一个加密类
            using (var aes = new AesCryptoServiceProvider())
            {
                // 用密码生成一个密钥和一个初始化向量
                var pdb = new Rfc2898DeriveBytes(password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64 });
                aes.Key = pdb.GetBytes(32);
                aes.IV = pdb.GetBytes(16);

                // 创建一个解密器对象
                using (var decryptor = aes.CreateDecryptor())
                {
                    // 将密文转换为字节数组
                    var cipherBytes = Convert.FromBase64String(cipherText);

                    // 对字节数组进行解密，并返回解密后的字符串
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        return Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
            }
        }
    }
}
