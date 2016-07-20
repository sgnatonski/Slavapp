using System;
using System.Linq;
using System.Text;
namespace SlavApp.Api.Resembler.Services
{
    public class UsageHashDecryptService : IUsageHashDecryptService
    {
        public UsageHash DecryptHash(string hash)
        {
            if (string.IsNullOrEmpty(hash))
            {
                return null;
            }

            var b64 = hash.Reverse() + "==";
            byte[] data = Convert.FromBase64String(b64);
            string decodedString = Encoding.UTF8.GetString(data);
            var parts = decodedString.Split('|');

            return new UsageHash()
            {
                Version = new Version(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2])).ToString(),
                Identity = parts[3],
                PHashCount = int.Parse(parts[4]),
                DHashCount = int.Parse(parts[5])
            };
        }
    }
}