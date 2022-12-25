using ETICARET.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETICARET.Business.Abstract
{
    public interface IOrderService
    {
        List<Order> GetOrders(int userId);
        void Create(Order entity);
    }
}
