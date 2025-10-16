using System;
using ApiEcommerce.Models;

namespace ApiEcommerce.Repository.IRepository;

public interface IProductRepository
{
  ICollection<Product> GetProducts();
  ICollection<Product> GetProductsForCategory(int categoryId);
  ICollection<Product> SearchProduct(string nombre);
  Product? GetProduct(int id);
  bool BuyProduct(string nombre, int quantity);
  bool ProductExists(int id);
  bool ProductExists(string name);
  bool CreateProduct(Product product);
  bool UpdateProduct(Product product);
  bool DeleteProduct(Product product);

  bool Save();
}
