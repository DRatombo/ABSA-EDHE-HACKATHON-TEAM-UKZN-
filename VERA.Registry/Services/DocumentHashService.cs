using System.Security.Cryptography;

namespace VERA.Registry.Services
{
    public class DocumentHashService
    {
        public async Task<string> ComputeSha256Async(
            Stream stream)
        {
            using var sha256 = SHA256.Create();

            byte[] hashBytes =
                await sha256.ComputeHashAsync(stream);

            return Convert.ToHexString(hashBytes);
        }
    }
}