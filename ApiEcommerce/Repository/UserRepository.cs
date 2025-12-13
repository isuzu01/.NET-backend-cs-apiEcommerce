using System;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;

namespace ApiEcommerce.Repository;

public class UserRepository : IUserRepository
{

  public readonly ApplicationDbContext _db;

  public UserRepository( ApplicationDbContext db )
  {
    _db = db;
  }

  public User? GetUser(int id)
  {
    return _db.Users.FirstOrDefault( u => u.Id == id );
  }

  public ICollection<User> GetUsers()
  {
    return _db.Users.OrderBy(u => u.Username).ToList();
  }

  public bool IsUniqueUser(string username)
  {
    return !_db.Users.Any( u => u.Username.ToLower().Trim() == username.ToLower().Trim() );
  }

  public Task<UserLoginResponseDto> Login(UserLoginDto userLogin)
  {
    throw new NotImplementedException();
  }

  public async Task<User> Register(CreateUserDto createUser)
  {
    var encriptedPassword = BCrypt.Net.BCrypt.HashPassword(createUser.Password);
    var user = new User
    {
      Username = createUser.Username?? "No User Name",
      Name = createUser.Name,
      Role = createUser.Role,
      password = encriptedPassword
    };
    _db.Users.Add(user);
    await _db.SaveChangesAsync();
    return user;
  }
}
