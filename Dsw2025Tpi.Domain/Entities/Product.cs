using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Product : EntityBase
    {
        public string sku { get; set; }
        public string internalCode { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        private decimal _currentUnitPrice;

        public decimal currentUnitPrice
        {
            get => _currentUnitPrice;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("El precio debe ser mayor a 0.");
                _currentUnitPrice = value;
            }
        }
        private int _stockQuantity;
        public int stockQuantity
        {
            get => _stockQuantity;
            set
            {
                if (value < 0)
                    throw new ArgumentException("La cantidad de stock no puede ser negativa.");
                _stockQuantity = value;
            }
        }

        public Product(string sku, string internalCode, string name, string description, decimal currentUnitPrice, int stockQuantity, bool isActive)
        {
            this.sku = sku;
            this.internalCode = internalCode;
            this.name = name;
            this.description = description;
            this.currentUnitPrice = currentUnitPrice;
            this.stockQuantity = stockQuantity;
            this.isActive = isActive;
        }


        public bool isActive { get; set; }
        public ICollection<OrderItem>? items { get; set; }

        public void Toggle()
        {
            isActive = !isActive;
        }


    }
}
