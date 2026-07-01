using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace Servicios
{
    public static class Encriptador
    {
        private static readonly string ClaveMaestra = "dwam_TechnoMode_2026_Systems_UAI";
        private static readonly string IV = "1234567890123456";

        public static string Hash(string texto)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string EncriptarAES(string textoPlano)
        {
            if (string.IsNullOrEmpty(textoPlano)) return textoPlano;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(ClaveMaestra);
                aesAlg.IV = Encoding.UTF8.GetBytes(IV);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(textoPlano);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }

        public static string DesencriptarAES(string textoCifrado)
        {
            if (string.IsNullOrWhiteSpace(textoCifrado)) return string.Empty;

            // 1. Limpiamos cualquier salto de línea, tab o espacio fantasma que venga de SQL
            textoCifrado = textoCifrado.Trim();

            // 2. Control matemático estricto del padding de Base64 (Múltiplos de 4)
            // Si SQL truncó los "==" del final, se los volvemos a poner para que .NET no explote.
            int mod4 = textoCifrado.Length % 4;
            if (mod4 > 0)
            {
                textoCifrado += new string('=', 4 - mod4);
            }

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(ClaveMaestra);
                aesAlg.IV = Encoding.UTF8.GetBytes(IV);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(textoCifrado)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}