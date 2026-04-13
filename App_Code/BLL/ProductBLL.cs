using System.Collections.Generic;
using serena.Entities;
using serena.DAL;

namespace serena.BLL
{
    public class ProductBLL
    {
        private ProductDAL _dal = new ProductDAL();

        public List<Product> GetNewArrivals(int limit = 8)
        {
            return _dal.GetNewArrivals(limit);
        }

        public List<Product> GetTopPicks(int limit = 8)
        {
            return _dal.GetTopPicks(limit);
        }

        public List<Product> GetProductsByIds(List<int> ids)
        {
            return _dal.GetByIds(ids);
        }

        public Product GetProductBySlug(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return null;
            return _dal.GetBySlug(slug);
        }

        public Product GetProduct(int id)
        {
            if (id <= 0) return null;
            return _dal.GetById(id);
        }

        public bool IsInStock(int productId, int quantityRequested)
        {
            var p = GetProduct(productId);
            return p != null && p.Stock >= quantityRequested;
        }
    }
}
