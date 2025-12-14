using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
  {
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UsersController( IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    // obtener lista de usuarios
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetUsers()
    {
        var users = _userRepository.GetUsers();
        var usersDto = _mapper.Map<List<UserDto>>(users);

        return Ok(usersDto);
    }

    //usuario por ID
    [HttpGet("{id:int}", Name = "GetUser")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetUser(int id)
    {
        var user = _userRepository.GetUser(id);
        if (user == null)
        {
            return NotFound($"El Usuario con el id {id} no existe");
        }
        var userDto = _mapper.Map<UserDto>(user);
        return Ok(userDto);
    }

    //Registrar usuario
    [HttpPost(Name = "RegisterUser")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
    {
        if ( createUserDto == null || !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }
        if ( string.IsNullOrWhiteSpace( createUserDto.Username ) )
        {
            return BadRequest("Username es requerido");
        }
        if ( ! _userRepository.IsUniqueUser( createUserDto.Username ) )
        {
            return BadRequest("El usuario ya existe");
        }
        var result = await _userRepository.Register( createUserDto );
        if ( result == null )
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "El Usuario no se pudo registrar");
        }
    
        return CreatedAtRoute("GetUser", new { id = result.Id }, result);
    }

    //Login usuario
    [HttpPost("login", Name = "LoginUser")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginUser([FromBody] UserLoginDto userLoginDto)
    {
        if ( userLoginDto == null || !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }
        var user = await _userRepository.Login( userLoginDto );
        if ( user == null )
        {
            return Unauthorized();
        }
    
        return Ok(user);
    }


  }
}
