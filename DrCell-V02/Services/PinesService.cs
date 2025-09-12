using DrCell_V02.Data;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace DrCell_V02.Services
{
    public class PinesService : IPinesService
    {
        private readonly ApplicationDbContext _context;

        public PinesService(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public async Task<List<object>> ObtenerPinesAsync()
        {
            return await _context.Pines
                .AsNoTracking()
                .Select(p => new {p.marca, p.modelo})
                .Distinct()
                .Cast<object>()
                .ToListAsync();
        }

        public async Task<List<object>> ObtenerPinesByMarcaAsync(string marca)
        {
            return await _context.Pines
                .Where(p => p.marca == marca)
                .Select(p => new
                {
                   p.marca,
                   p.modelo,
                   p.arreglo,
                   p.placa
                }).Cast<object>()
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<string>> ObtenerPinesByModeloAsync()
        {
            return await _context.Pines
                .AsNoTracking()
                .Select(p => p.modelo)
                .Where(m => !string.IsNullOrEmpty(m))
                .Cast<string>()
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<object>> ObtenerPinesByModeloYMarcaAsync(string marca, string modelo)
        {
            return await _context.Pines
                .Where(p => p.marca != null && p.modelo != null && 
                           EF.Functions.ILike(p.marca, marca) && EF.Functions.ILike(p.modelo, modelo))
                .Select(m => new
                {
                    m.marca,
                    m.modelo,
                    m.arreglo,
                    m.costo,
                    m.tipo,
                    m.placa,
                    m.id
                }).Cast<object>()
                .ToListAsync();
        }

        public async Task<Pines?> GetPinByIdAsync(int id)
        {
            return await _context.Pines.FirstOrDefaultAsync(p => p.id == id);
        }

        public async Task UpdateAsync(Pines pin)
        {
            var entidad = await _context.Pines.FirstOrDefaultAsync(p => p.id == pin.id);
            if (entidad != null)
            {
                entidad.marca = pin.marca ?? "";
                entidad.modelo = pin.modelo ?? "";
                entidad.arreglo = pin.arreglo;
                entidad.costo = pin.costo;
                entidad.tipo = pin.tipo ?? "";
                entidad.placa = pin.placa;

                _context.Pines.Update(entidad);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entidad = await _context.Pines.FirstOrDefaultAsync(p => p.id == id);
            if (entidad != null)
            {
                _context.Pines.Remove(entidad);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsPinAsync(string marca, string modelo)
        {
            if (string.IsNullOrEmpty(marca) || string.IsNullOrEmpty(modelo))
                return false;
                
            return await _context.Pines
                .AnyAsync(p => p.marca == marca && p.modelo == modelo);
        }

        public async Task<Pines> AddAsync(Pines pin)
        {
            bool existe = await ExistsPinAsync(pin.marca, pin.modelo);
            if(existe){
                throw new InvalidOperationException("El pin ya existe");
            }
            _context.Pines.Add(pin);
            await _context.SaveChangesAsync();
            return pin;
        }
    }
}
