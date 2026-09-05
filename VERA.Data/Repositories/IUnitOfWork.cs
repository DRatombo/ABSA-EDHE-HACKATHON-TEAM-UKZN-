namespace VERA.Data.Repositories
{
    /// <summary>
    /// Single entry point for controllers/services to reach every repository
    /// and to commit changes. Using one shared DbContext per request (via
    /// these repositories) instead of one-per-repository means a single
    /// SaveChangesAsync() call commits everything that happened in that
    /// request as one atomic unit - a Business add and an Opportunity add in
    /// the same request either both persist or neither does.
    /// </summary>
    public interface IUnitOfWork
    {
        IBusinessRepository Businesses { get; }

        IOpportunityRepository Opportunities { get; }

        IFundingOfferRepository FundingOffers { get; }

        IFulfilmentRecordRepository FulfilmentRecords { get; }

        /// <summary>
        /// Persists every tracked change to the database. Returns the number
        /// of rows written - 0 is a useful signal that nothing actually
        /// needed saving, which is handy for "did my update actually do
        /// anything" style bugs.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}

