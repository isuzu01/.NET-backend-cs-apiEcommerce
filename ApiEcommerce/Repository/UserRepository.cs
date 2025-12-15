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

  public ApplicationUser? GetUser(string id)
  {
    return _db.ApplicationUsers.FirstOrDefault( u => u.Id == id );
  }

  public ICollection<ApplicationUser> GetUsers()
  {
    return _db.ApplicationUsers.OrderBy(u => u.UserName).ToList();
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

  public async Task<UserDataDto> Register(CreateUserDto createUser)
  {
    if(string.IsNullOrEmpty(createUser.Username) )
    {
      throw new ArgumentException("El Username es requerido");
    }

    if(createUser.Password == null)
    {
      throw new ArgumentException("El Passwowrd es requerido");
    }

    var user = new ApplicationUser()
    {
      UserName = createUser.Username,
      NormalizedEmail = createUser.Username.ToUpper(),
      Email = createUser.Username,
      Name = createUser.Name,
    };

    var result = await _userManager.CreateAsync(user, createUser.Password);
    if(!result.Succeeded)
    {
      var userRole = createUser.Role ?? "User";
      var roleExists = await _roleManager.RoleExistsAsync(userRole);
      if(!roleExists)
      {
        var identityRole = new IdentityRole(userRole);
        await _roleManager.CreateAsync(identityRole);
      }
      await _userManager.AddToRoleAsync(user, userRole);
      var createdUser = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == createUser.Username);
      return _mapper.Map<UserDataDto>(createdUser);
    }
    throw new ApplicationException("Error al registrar el usuario" );
  }
}
