using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
  {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        // obtener lista de productos
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            var products = _productRepository.GetProducts();
            var productsDto = _mapper.Map<List<ProductDto>>(products);

            return Ok(productsDto);
        }

        //product por ID
        [HttpGet("{productId:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProduct(int productId)
        {
            var product = _productRepository.GetProduct(productId);
            if (product == null)
            {
                return NotFound($"El Producto con el id {productId} no existe");
            }
            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }
        
        //Crear producto
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (createProductDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_productRepository.ProductExists(createProductDto.Name))
            {
                ModelState.AddModelError("CustomError", "El Producto ya existe");
                return BadRequest(ModelState);
            }
            if (!_categoryRepository.CategoryExists(createProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", $"La categoria con el {createProductDto.CategoryId} no existe");
                return BadRequest(ModelState);
            }
            var product = _mapper.Map<Product>(createProductDto);  //convertir la entidad dto en una entidad de dominio, que seria Product
            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("CustomeError", $"Algo salió mal al guardar el registro {product.Name}");
                return StatusCode(500, ModelState);
            }
            var createdProduct = _productRepository.GetProduct(product.ProductId);
            var productoDto = _mapper.Map<ProductDto>(createdProduct);
            return CreatedAtRoute("GetProduct", new { productId = product.ProductId }, product);
        }

  }
}
