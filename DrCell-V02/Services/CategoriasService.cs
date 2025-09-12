using AutoMapper;
using DrCell_V02.Data;
using DrCell_V02.Data.Dtos;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace DrCell_V02.Services
{
    public class CategoriasService : ICategoriaService
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<CategoriasService> _logger;

        public CategoriasService(IMapper mapper, ApplicationDbContext applicationDbContext, ILogger<CategoriasService> logger)
        {
            _mapper = mapper;
            _dbContext = applicationDbContext;
            _logger = logger;
        }

        public async Task<CategoriaDto> AddCategoriaAsync(CategoriaDto categoriaDto)
        {
           try
            {
                var categoria = _mapper.Map<Categorias>(categoriaDto);
                _dbContext.Categorias.Add(categoria);
                await _dbContext.SaveChangesAsync();
                
                return _mapper.Map<CategoriaDto>(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar categoría");
                throw;
            }
        }

        public async Task DeleteCategoriaAsync(int id)
        {
           try
            {
                var categoria = await _dbContext.Categorias.FindAsync(id);
                if (categoria == null)
                    throw new KeyNotFoundException("Categoría no encontrada");

                // Soft delete
                categoria.Activa = false;
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría");
                throw;
            }
        }

        public async Task<IEnumerable<CategoriaDto>> GetAllCategoriasAsync()
        {
           try
            {
                var categorias = await _dbContext.Categorias
                    .Where(c => c.Activa)
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.Map<IEnumerable<CategoriaDto>>(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las categorías");
                throw;
            }
        }

        public async Task<CategoriaDto> GetCategoriaByIdAsync(int id)
        {
           try
            {
                var categoria = await _dbContext.Categorias
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                return categoria != null ? _mapper.Map<CategoriaDto>(categoria) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categoría por ID: {Id}", id);
                throw;
            }
        }


        public async Task UpdateCategoriaAsync(CategoriaDto categoriaDto)
        {
             try
            {
                var categoria = await _dbContext.Categorias.FindAsync(categoriaDto.Id);
                if (categoria == null)
                    throw new KeyNotFoundException("Categoría no encontrada");

                _mapper.Map(categoriaDto, categoria);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar categoría");
                throw;
            }

        }
    }
}
