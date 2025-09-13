using BCrypt.Net;
using LandingBack.Data;
using LandingBack.Data.Dtos;
using LandingBack.Data.Modelos;
using LandingBack.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LandingBack.Services
{
    public class UsuariosService : IUsuariosService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UsuariosService> _logger;

        public UsuariosService(AppDbContext context, ILogger<UsuariosService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<AgenteDto>> GetUsuariosAsync()
        {
            var users = await _context.Agentes
                .OrderByDescending(a => a.FechaCreacion)
                .ToListAsync();
            return users.Select(a => new AgenteDto
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Email = a.Email,
                Telefono = a.Telefono,
                Rol = a.Rol,
                Activo = a.Activo,
                UltimoLogin = a.UltimoLogin,
                FechaCreacion = a.FechaCreacion
            });
        }

        public async Task<AgenteDto> CreateAgenteAsync(CreateAgenteDto dto, int adminId)
        {
            if (dto.Rol != "Agente" && dto.Rol != "Cargador")
                throw new ArgumentException("Rol inválido");

            var exists = await _context.Agentes.AnyAsync(a => a.Email == dto.Email);
            if (exists)
                throw new InvalidOperationException("Email ya registrado");

            var hashed = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var entity = new Agente
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                Telefono = dto.Telefono,
                Password = hashed,
                Rol = dto.Rol,
                Activo = dto.Activo,
                FechaCreacion = DateTime.UtcNow,
                FechaActualizacion = DateTime.UtcNow
            };

            _context.Agentes.Add(entity);
            await _context.SaveChangesAsync();

            return new AgenteDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Email = entity.Email,
                Telefono = entity.Telefono,
                Rol = entity.Rol,
                Activo = entity.Activo,
                UltimoLogin = entity.UltimoLogin,
                FechaCreacion = entity.FechaCreacion
            };
        }

        public async Task<AgenteDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _context.Agentes.FirstOrDefaultAsync(a => a.Id == userId);
            if (user == null) throw new KeyNotFoundException("Usuario no encontrado");

            user.Nombre = dto.Nombre;
            user.Email = dto.Email;
            user.Telefono = dto.Telefono;
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }
            user.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new AgenteDto
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Email = user.Email,
                Telefono = user.Telefono,
                Rol = user.Rol,
                Activo = user.Activo,
                UltimoLogin = user.UltimoLogin,
                FechaCreacion = user.FechaCreacion
            };
        }

        public async Task<bool> ToggleActivoAsync(int id)
        {
            var user = await _context.Agentes.FirstOrDefaultAsync(a => a.Id == id);
            if (user == null) return false;
            user.Activo = !user.Activo;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}


