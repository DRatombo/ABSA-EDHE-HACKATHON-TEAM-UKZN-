using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using VERA.Registry.Models;

namespace VERA.Registry.Services
{
    public class FingerprintService
    {
        public string GeneratePurchaseOrderFingerprint(
            RegistryIssuer issuer,
            RegisteredPurchaseOrder po)
        {
            string canonical =
                $"{Normalize(issuer.CIPCRegistrationNumber)}|" +
                $"{Normalize(po.PONumber)}|" +
                $"{Normalize(po.SupplierCIPCRegistrationNumber ?? po.SupplierName)}|" +
                $"{po.POValue.ToString("F2", CultureInfo.InvariantCulture)}|" +
                $"{po.IssueDate:yyyy-MM-dd}";

            using SHA256 sha256 = SHA256.Create();

            byte[] inputBytes =
                Encoding.UTF8.GetBytes(canonical);

            byte[] hashBytes =
                sha256.ComputeHash(inputBytes);

            return Convert.ToHexString(hashBytes);
        }

        private static string Normalize(string value)
        {
            return value
                .Trim()
                .ToUpperInvariant()
                .Replace(" ", "");
        }
    }
}