namespace VERA.Registry.Services
{
    public class VeraIdService
    {
        public string GenerateIssuerId()
        {
            string unique = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 8)
                .ToUpper();

            return $"VRA-ZA-{DateTime.UtcNow.Year}-{unique}";
        }

        public string GeneratePOId()
        {
            string unique = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 8)
                .ToUpper();

            return $"VPO-ZA-{DateTime.UtcNow.Year}-{unique}";
        }
    }
}