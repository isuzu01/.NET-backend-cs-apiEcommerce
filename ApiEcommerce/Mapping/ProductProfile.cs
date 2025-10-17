using System;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using AutoMapper;
using Microsoft.Data.SqlClient;

namespace ApiEcommerce.Mapping;

public class ProductProfile:Profile   //heredar profile de autoMapper
{
  public ProductProfile()
  {
    CreateMap<Product, ProductDto>()
    .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(SqlRowsCopiedEventArgs => SqlRowsCopiedEventArgs.Category.Name))
    .ReverseMap();
    CreateMap<Product, CreateProductDto>().ReverseMap();
    CreateMap<Product, UpdateProductDto>().ReverseMap();
  }
}
