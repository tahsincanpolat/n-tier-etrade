using ETICARET.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETICARET.Business.Abstract
{
    public interface IProductService
    {
        Product GetById(int id);
        Product GetProductsByCategory(string category,int page,int pageSize);
        List<Product> GetAll();
        Product GetProductDetails(int id);
        void Create(Product entity);
        void Delete(Product entity);
        void Update(Product entity);
        int GetCountByCategory(string category);
    }
}
