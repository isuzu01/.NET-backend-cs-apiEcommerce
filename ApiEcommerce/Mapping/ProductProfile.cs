using System;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using AutoMapper;

namespace ApiEcommerce.Mapping;

public class ProductProfile:Profile   //heredar profile de autoMapper
{
  public ProductProfile()
  {
    CreateMap<Product, ProductDto>().ReverseMap();
    CreateMap<Product, CreateProductDto>().ReverseMap();
    CreateMap<Product, UpdateProductDto>().ReverseMap();
  }
}
