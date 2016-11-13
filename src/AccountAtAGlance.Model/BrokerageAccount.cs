using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountAtAGlance.Model
{
    public class BrokerageAccount
    {
        public BrokerageAccount()
        {
            Positions = new List<Position>();
            Orders = new List<Order>();
        }
    
        // Primitive properties
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        [RegularExpression("[A-Z][0-9]*")]
        public string AccountNumber { get; set; }

        [StringLength(100)]
        public string AccountTitle { get; set; }

        public decimal Total { get; set; }
        public decimal MarginBalance { get; set; }
        public bool IsRetirement { get; set; }
        public decimal CashTotal { get; set; }
        public decimal PositionsTotal { get; set; }
        public int CustomerId { get; set; }

        // Navigation properties
        public Customer Customer { get; set; }
        public List<Position> Positions { get; set; }
        public List<Order> Orders { get; set; }
    }
}
