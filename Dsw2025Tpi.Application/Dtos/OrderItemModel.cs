using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    public record OrderItemModel
    {
        public record RequestOrderItemModel(int quantity, Guid ProductId);
        public record ResponseOrderItemModel(Guid Id, int quantity, decimal unitPrice, Guid OrderId, Guid ProductId);
    }
}
