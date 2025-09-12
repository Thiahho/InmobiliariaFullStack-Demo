using AutoMapper;
using DrCell_V02.Data;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace DrCell_V02.Services
{
    public class BateriasService : IBateriasService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BateriasService(ApplicationDbContext applicationDbContext, IMapper mapper)
        {
            _context = applicationDbContext;
            _mapper = mapper;
        }

        public async Task<List<object>> ObtenerBateriasAsync()
        {
            return await _context.Baterias
                .AsNoTracking()
                .Select(p => new {p.marca, p.modelo})
                .Distinct()
                .Cast<object>()
                .ToListAsync();
        }

        public async Task<List<object>> ObtenerBateriasByMarcaAsync(string marca)
        {
            return await _context.Baterias
                .Where(b => b.marca == marca)
                .Select(b => new
                {
                    b.marca,
                    b.modelo,
                    b.arreglo,
                }).Cast<object>()
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<string>> ObtenerBateriasByModeloAsync()
        {
            return await _context.Baterias
                .AsNoTracking()
                .Select(p => p.modelo)
                .Where(m => !string.IsNullOrEmpty(m))
                .Cast<string>()
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<object>> ObtenerBateriasByModeloYMarcaAsync(string marca, string modelo)
        {
            return await _context.Baterias
                .Where(p => p.marca != null && p.modelo != null && 
                           EF.Functions.ILike(p.marca, marca) && EF.Functions.ILike(p.modelo, modelo))
                .Select(m => new
                {
                    marca = m.marca ?? "",
                    modelo = m.modelo ?? "",
                    arreglo = m.arreglo,
                    costo = m.costo,
                    id = m.id
                }).Cast<object>()
                .ToListAsync();
        }

        public async Task<Baterias?> GetBateriaByIdAsync(int id)
        {
            return await _context.Baterias.FirstOrDefaultAsync(b => b.id == id);
        }

        public async Task UpdateAsync(Baterias bateria)
        {
            var entidad = await _context.Baterias.FirstOrDefaultAsync(b => b.id == bateria.id);
            if (entidad != null)
            {
                entidad.marca = bateria.marca ?? "";
                entidad.modelo = bateria.modelo ?? "";
                entidad.arreglo = bateria.arreglo;
                entidad.costo = bateria.costo;
                entidad.tipo = bateria.tipo ?? "";
                
                _context.Baterias.Update(entidad);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entidad = await _context.Baterias.FirstOrDefaultAsync(b => b.id == id);
            if (entidad != null)
            {
                _context.Baterias.Remove(entidad);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsBateriaAsync(string marca, string modelo)
        {
            if (string.IsNullOrEmpty(marca) || string.IsNullOrEmpty(modelo))
                return false;
                
            return await _context.Baterias
                .AnyAsync(b => b.marca == marca && b.modelo == modelo);
        }
        public async Task<Baterias> AddAsync(Baterias bateria)
        {
            bool existe = await ExistsBateriaAsync(bateria.marca, bateria.modelo); 
            if(existe){
                throw new InvalidOperationException("La bateria ya existe");
            }
            _context.Baterias.Add(bateria);
            await _context.SaveChangesAsync();
            return bateria;
        }
    }
}
