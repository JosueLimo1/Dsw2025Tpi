using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class OrderItem : EntityBase
    {
        public decimal Subtotal => quantity * unitPrice;

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        private int _quantity;
        public int quantity
        {
            get => _quantity;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("La cantidad debe ser mayor o igual a 0");
                }
                _quantity = value;
            }
        }

        private decimal _unitPrice;

        public decimal unitPrice
        {
            get => _unitPrice;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("El precio unitario debe ser mayor o igual a 0");
                }
                _unitPrice = value;
            }
        }
      
        public OrderItem(int quantity, decimal unitPrice, Guid OrderId, Guid ProductId)
        {
            this.ProductId = ProductId;
            this.OrderId = OrderId;
            this.quantity= quantity;
            this.unitPrice = unitPrice;
        }


    }
}
