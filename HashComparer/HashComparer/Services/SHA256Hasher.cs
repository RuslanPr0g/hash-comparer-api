using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HashComparer.Services
{
    public class SHA256Hasher : IHasher
    {
        public string Hash(string data, char[] keyCharArray)
        {
            return CalculateHMAC(data, keyCharArray);
        }

        private static string CalculateHMAC(string data, char[] keyCharArray)
        {
            byte[] keyEncoded = Encoding.ASCII.GetBytes(keyCharArray);
            HMACSHA256 hmacsha256 = new(keyEncoded);
            byte[] dataByteArray = Encoding.ASCII.GetBytes(data);
            using MemoryStream dataByteArrayStream = new(dataByteArray);
            string resultInHMAC = hmacsha256.ComputeHash(dataByteArrayStream)
                                    .Aggregate("", (s, e) =>
                                    {
                                        return $"{s}{e:x2}";
                                    }, s => s);
            return resultInHMAC;
        }

        private static string CalculateHMACAnotherVariant(string data, char[] keyCharArray)
        {
            byte[] keyEncoded = Encoding.ASCII.GetBytes(keyCharArray);
            HMACSHA256 hmacsha256 = new(keyEncoded);
            byte[] dataByteArray = Encoding.ASCII.GetBytes(data);
            using MemoryStream dataByteArrayStream = new(dataByteArray);

            var bytes = hmacsha256.ComputeHash(dataByteArrayStream);
            var output = bytes
                .Select(x => x.ToString("x2"))
                .ToList();
            return string.Join("", output);
        }
    }
}
