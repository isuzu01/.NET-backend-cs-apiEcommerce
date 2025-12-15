using System;

namespace ApiEcommerce.Models.Dtos;

public class UserDto
{
  //public int Id { get; set; }
  public string Id { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Username { get; set; }
  public string? password { get; set; }
  public string? Role { get; set; }
}
