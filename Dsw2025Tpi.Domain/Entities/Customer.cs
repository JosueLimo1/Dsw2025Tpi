using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Customer : EntityBase
    {
        public string? eMail { get; set; }
        public string? name { get; set; }  
        public string? phoneNumber { get; set; }

        public Customer(string? email, string? name, string? phoneNumber)
        {
            this.eMail = email;
            this.name = name;  
            this.phoneNumber = phoneNumber;
        }
        public ICollection<Order>? Order { get; set; } = new List<Order>();
    }
}
