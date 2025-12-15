using System;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;

namespace ApiEcommerce.Repository.IRepository;

public interface IUserRepository
{
  //ICollection<User> GetUsers();
  ICollection<ApplicationUser> GetUsers();
  //User? GetUser(int id);
  ApplicationUser? GetUser(string id);
  bool IsUniqueUser(string username);
  Task<UserLoginResponseDto> Login( UserLoginDto userLogin );
  //Task<User> Register( CreateUserDto createUser );
  Task<UserDataDto> Register( CreateUserDto createUserDto );
}
