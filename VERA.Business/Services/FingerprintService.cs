using System.Security.Cryptography;
using System.Text;
using VERA.Models.Entities;

namespace VERA.Business.Services
{
    /// <summary>
    /// Generates a deterministic fingerprint for an opportunity.
    ///
    /// The fingerprint allows VERA to identify potentially duplicated
    /// purchase-order funding requests without storing or comparing the
    /// original values as one long plain-text identifier.
    /// </summary>
    public class FingerprintService
    {
        /// <summary>
        /// Generates a SHA-256 fingerprint from important opportunity fields.
        /// </summary>
        /// <param name="opportunity">
        /// The opportunity for which the fingerprint must be generated.
        /// </param>
        /// <returns>
        /// A hexadecimal SHA-256 fingerprint representing the opportunity.
        /// </returns>
        public string Generate(Opportunity opportunity)
        {
            // Normalise the buyer name so differences in capitalisation
            // or accidental spaces do not produce different fingerprints.
            string buyerName =
                opportunity.BuyerName.Trim().ToUpperInvariant();

            // Normalise the PO number for the same reason.
            string poNumber =
                opportunity.PONumber.Trim().ToUpperInvariant();

            // Combine selected identifying attributes of the opportunity.
            //
            // BusinessId identifies the SME submitting the opportunity.
            // BuyerName identifies the organisation that issued the PO.
            // PONumber identifies the buyer's purchase order.
            // POValue provides an additional comparison attribute.
            string rawValue =
                $"{buyerName}|" +
                $"{opportunity.BusinessId}|" +
                $"{poNumber}|" +
                $"{opportunity.POValue:F2}";

            // Convert the combined string into bytes because the SHA-256
            // algorithm operates on byte data.
            byte[] inputBytes =
                Encoding.UTF8.GetBytes(rawValue);

            // Create the SHA-256 hashing algorithm.
            using SHA256 sha256 = SHA256.Create();

            // Generate the fixed-length cryptographic hash.
            byte[] hashBytes =
                sha256.ComputeHash(inputBytes);

            // Convert the hash into a hexadecimal string that can easily
            // be stored and compared in the database.
            return Convert.ToHexString(hashBytes);
        }
    }
}