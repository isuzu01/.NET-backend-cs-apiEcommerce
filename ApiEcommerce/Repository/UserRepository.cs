using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiEcommerce.Repository;

public class UserRepository : IUserRepository
{

  public readonly ApplicationDbContext _db;
  private string? secretKey;

  private readonly UserManager<ApplicationUser> _userManager;
  private readonly RoleManager<IdentityRole> _roleManager;
  private readonly IMapper _mapper;

  public UserRepository( ApplicationDbContext db, 
                          IConfiguration configuration, 
                          UserManager<ApplicationUser> userManager, 
                          RoleManager<IdentityRole> roleManager, IMapper mapper)
  {
    _db = db;
    secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
    _userManager = userManager;
    _roleManager = roleManager;
    _mapper = mapper;
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

  public async Task<UserLoginResponseDto> Login(UserLoginDto userLogin)
  {
    if(string.IsNullOrEmpty(userLogin.Username) )
    {
      return new UserLoginResponseDto()
      {
        Token = "",
        User = null,
        Message = "El Username es requerido"
      };
    }

    var user = await _db.ApplicationUsers.FirstOrDefaultAsync<ApplicationUser>(u => u.UserName != null && u.UserName.ToLower().Trim() == userLogin.Username.ToLower().Trim());
   
    if(user == null)
    {
      return new UserLoginResponseDto()
      {
        Token = "",
        User = null,
        Message = "Username no encontrado"
      };
    }
    if(userLogin.Password == null)
    {
      return new UserLoginResponseDto()
      {
        Token = "",
        User = null,
        Message = "El password es requerido"
      };
    }
    bool isValid =await _userManager.CheckPasswordAsync(user, userLogin.Password);
    if(!isValid)
    {
      return new UserLoginResponseDto()
      {
        Token = "",
        User = null,
        Message = "Credenciales son incorrectas"
      };
    }
    //Jwt
    var handlerToken = new JwtSecurityTokenHandler();
    if(string.IsNullOrWhiteSpace(secretKey))
    {
      throw new InvalidOperationException("Secretkey no esta configurada");
    }
    var roles = await _userManager.GetRolesAsync(user);
    var key = Encoding.UTF8.GetBytes(secretKey);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(new[]
      {
        new Claim("id", user.Id.ToString()),
        new Claim("username", user.UserName ?? string.Empty),
        new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? string.Empty)
      }),
      Expires = DateTime.UtcNow.AddHours(2),
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var Token = handlerToken.CreateToken(tokenDescriptor);
    return new UserLoginResponseDto()
    {
      Token = handlerToken.WriteToken(Token),
      User = _mapper.Map<UserDataDto>(user),
      Message = "Usuario Logueado correctamente"
    };
  }

  public async Task<User> Register(CreateUserDto createUser)
  {
    var encriptedPassword = BCrypt.Net.BCrypt.HashPassword(createUser.Password);
    var user = new User
    {
      Username = createUser.Username?? "No User Name",
      Name = createUser.Name,
      Role = createUser.Role,
      Password = encriptedPassword
    };
    _db.Users.Add(user);
    await _db.SaveChangesAsync();
    return user;
  }
}
