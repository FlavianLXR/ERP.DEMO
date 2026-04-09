using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ERP.DEMO.Toolkit.Extensions
{
    public class MD5Extension
    {
        public string HashMd5(string stringToHash)
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(stringToHash);
            byte[] hashBytes;
            using (MD5 md5 = MD5.Create())
            {
                hashBytes = md5.ComputeHash(inputBytes);
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
                sb.Append(hashBytes[i].ToString("X2"));

            return sb.ToString();
        }
    }
}
