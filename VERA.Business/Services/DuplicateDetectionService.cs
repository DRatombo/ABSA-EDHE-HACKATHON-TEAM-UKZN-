using VERA.Models.Entities;

namespace VERA.Business.Services
{
    /// <summary>
    /// Detects whether an opportunity may already have been submitted
    /// to VERA using its generated fingerprint.
    ///
    /// This helps reduce the risk of the same purchase order being
    /// submitted multiple times for funding within the VERA platform.
    /// </summary>
    public class DuplicateDetectionService
    {
        /// <summary>
        /// Checks whether the fingerprint of the current opportunity
        /// already exists among previously submitted opportunities.
        /// </summary>
        /// <param name="opportunity">
        /// The new opportunity currently being assessed.
        /// </param>
        /// <param name="existingOpportunities">
        /// Opportunities that already exist in the VERA system.
        /// </param>
        /// <returns>
        /// True if a matching fingerprint is found; otherwise false.
        /// </returns>
        public bool IsDuplicate(
            Opportunity opportunity,
            IEnumerable<Opportunity> existingOpportunities)
        {
            // A fingerprint must first be generated before VERA
            // can perform duplicate detection.
            if (string.IsNullOrWhiteSpace(opportunity.Fingerprint))
            {
                return false;
            }

            // Search the existing opportunities for another record
            // containing exactly the same fingerprint.
            //
            // OpportunityId is checked so that an opportunity does not
            // accidentally identify itself as a duplicate when edited.
            bool duplicateExists = existingOpportunities.Any(existing =>
                existing.OpportunityId != opportunity.OpportunityId &&
                !string.IsNullOrWhiteSpace(existing.Fingerprint) &&
                existing.Fingerprint.Equals(
                    opportunity.Fingerprint,
                    StringComparison.OrdinalIgnoreCase));

            // Return the result to the Trust Engine.
            return duplicateExists;
        }
    }
}