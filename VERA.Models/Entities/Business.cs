using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VERA.Models.Entities
{
    public class Business
    {
        public int BusinessId { get; set; }

        [Required]
        public string BusinessName { get; set; } = string.Empty;

        [Required]
        public string RegistrationNumber { get; set; } = string.Empty;

        public string Industry { get; set; } = string.Empty;

        public string Province { get; set; } = string.Empty;

        public int YearsOperating { get; set; }

        public string OwnerName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool BankAccountVerified { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Opportunity> Opportunities { get; set; } = new();
    }
}