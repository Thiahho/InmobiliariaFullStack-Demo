using DrCell_V02.Data;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace DrCell_V02.Services
{
    public class ModulosService : IModulosService
    {
        private readonly ApplicationDbContext _context;

        public ModulosService(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }
        

        public async Task<List<object>> ObtenerModulosAsync()
        {
            return await _context.Modulos
                .AsNoTracking()
                .Select(m => new {m.marca, m.modelo})
                .Distinct()
                .Cast<object>()
                .ToListAsync();
        }

        public async Task<List<object>> ObtenerModulosByMarcaAsync(string marca)
        {
            return await _context.Modulos
                .Where(m => m.marca == marca)
                .Select(m => new
                {
                    m.marca,
                    m.modelo,
                    m.version,
                    m.marco,
                    m.color,
                    m.arreglo,
                }).Cast<object>()
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<object>> ObtenerModulosByModeloAsync(string modelo)
        {
            return await _context.Modulos
                .Where(c => c.modelo == modelo)
                .Select(m => new
                {
                    m.modelo,
                    m.marca,
                    m.arreglo,
                    m.tipo,
                    m.version,
                    m.marco,
                    m.color,
                }).Cast<object>()
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<object>> ObtenerModulosByModeloYMarcaAsync(string marca, string modelo)
        {
            return await _context.Modulos
                .Where(m => m.marca != null && m.modelo != null && 
                           EF.Functions.ILike(m.marca, marca) && EF.Functions.ILike(m.modelo, modelo))
                .Select(c => new
                {
                    c.marca,
                    c.modelo,
                    c.version,
                    c.marco,
                    c.color,
                    c.arreglo,
                    c.id
                }).Cast<object>()
                .ToListAsync();
        }

        public async Task<Modulos?> GetModuloByIdAsync(int id)
        {
            return await _context.Modulos.FirstOrDefaultAsync(m => m.id == id);
        }

        public async Task UpdateAsync(Modulos modulo)
        {
            var entidad = await _context.Modulos.FirstOrDefaultAsync(m => m.id == modulo.id);
            if (entidad != null)
            {
                entidad.marca = modulo.marca ?? "";
                entidad.modelo = modulo.modelo ?? "";
                entidad.version = modulo.version ?? "";
                entidad.marco = modulo.marco;
                entidad.color = modulo.color ?? "";
                entidad.arreglo = modulo.arreglo;
                entidad.tipo = modulo.tipo ?? "";

                _context.Modulos.Update(entidad);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entidad = await _context.Modulos.FirstOrDefaultAsync(m => m.id == id);
            if (entidad != null)
            {
                _context.Modulos.Remove(entidad);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsModuloAsync(string marca, string modelo)
        {
            if (string.IsNullOrEmpty(marca) || string.IsNullOrEmpty(modelo))
                return false;
                
            return await _context.Modulos
                .AnyAsync(m => m.marca == marca && m.modelo == modelo);
        }

        public async Task<Modulos> AddAsync(Modulos modulo)
        {
            bool existe = await ExistsModuloAsync(modulo.marca, modulo.modelo);
            if(existe){
                throw new InvalidOperationException("El modulo ya existe");
            }
            _context.Modulos.Add(modulo);
            await _context.SaveChangesAsync();
            return modulo;
        }
    }
}
