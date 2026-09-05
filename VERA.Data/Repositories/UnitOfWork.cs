using VERA.Data.Context;

namespace VERA.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly VeraDbContext _context;

        private IBusinessRepository? _businesses;
        private IOpportunityRepository? _opportunities;
        private IFundingOfferRepository? _fundingOffers;
        private IFulfilmentRecordRepository? _fulfilmentRecords;

        public UnitOfWork(VeraDbContext context)
        {
            _context = context;
        }

        public IBusinessRepository Businesses => _businesses ??= new BusinessRepository(_context);

        public IOpportunityRepository Opportunities => _opportunities ??= new OpportunityRepository(_context);

        public IFundingOfferRepository FundingOffers => _fundingOffers ??= new FundingOfferRepository(_context);

        public IFulfilmentRecordRepository FulfilmentRecords => _fulfilmentRecords ??= new FulfilmentRecordRepository(_context);

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return _context.SaveChangesAsync(ct);
        }
    }
}
