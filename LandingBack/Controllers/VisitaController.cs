using LandingBack.Data.Dtos;
using LandingBack.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LandingBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VisitaController : ControllerBase
    {
        private readonly IVisitaService _visitaService;
        private readonly IVisitaAuditoriaService _auditoriaService;

        public VisitaController(IVisitaService visitaService, IVisitaAuditoriaService auditoriaService)
        {
            _visitaService = visitaService;
            _auditoriaService = auditoriaService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VisitaResponseDto>> GetVisita(int id)
        {
            try
            {
                var visita = await _visitaService.GetVisitaByIdAsync(id);
                return Ok(visita);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetVisitas([FromQuery] VisitaSearchDto searchDto)
        {
            var (visitas, totalCount) = await _visitaService.GetVisitasAsync(searchDto);
            
            return Ok(new
            {
                visitas,
                totalCount,
                page = searchDto.Page,
                pageSize = searchDto.PageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<VisitaResponseDto>> CreateVisita(VisitaCreateDto visitaCreateDto)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var visita = await _visitaService.CreateVisitaAsync(visitaCreateDto, usuarioId);
                return CreatedAtAction(nameof(GetVisita), new { id = visita.Id }, visita);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<VisitaResponseDto>> UpdateVisita(int id, VisitaUpdateDto visitaUpdateDto)
        {
            try
            {
                if (id != visitaUpdateDto.Id)
                    return BadRequest("El ID de la visita no coincide");

                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var visita = await _visitaService.UpdateVisitaAsync(id, visitaUpdateDto, usuarioId);
                return Ok(visita);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVisita(int id)
        {
            try
            {
                await _visitaService.DeleteVisitaAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("agente/{agenteId}")]
        public async Task<ActionResult<IEnumerable<VisitaResponseDto>>> GetVisitasByAgente(
            int agenteId, 
            [FromQuery] DateTime? fechaDesde = null, 
            [FromQuery] DateTime? fechaHasta = null)
        {
            var visitas = await _visitaService.GetVisitasByAgenteAsync(agenteId, fechaDesde, fechaHasta);
            return Ok(visitas);
        }

        [HttpGet("propiedad/{propiedadId}")]
        public async Task<ActionResult<IEnumerable<VisitaResponseDto>>> GetVisitasByPropiedad(int propiedadId)
        {
            var visitas = await _visitaService.GetVisitasByPropiedadAsync(propiedadId);
            return Ok(visitas);
        }

        [HttpGet("calendar")]
        public async Task<ActionResult<IEnumerable<VisitaCalendarDto>>> GetVisitasCalendar(
            [FromQuery] int? agenteId = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            var visitas = await _visitaService.GetVisitasCalendarAsync(agenteId, fechaDesde, fechaHasta);
            return Ok(visitas);
        }

        [HttpGet("disponibilidad/{agenteId}")]
        public async Task<ActionResult<DisponibilidadDto>> GetDisponibilidadAgente(
            int agenteId, 
            [FromQuery] DateTime fecha)
        {
            try
            {
                var disponibilidad = await _visitaService.GetDisponibilidadAgenteAsync(agenteId, fecha);
                return Ok(disponibilidad);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("check-conflict")]
        public async Task<ActionResult<bool>> CheckConflict(ConflictCheckDto conflictDto)
        {
            var hasConflict = await _visitaService.CheckConflictAsync(conflictDto);
            return Ok(new { hasConflict });
        }

        [HttpGet("available-slots/{agenteId}")]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetAvailableSlots(
            int agenteId,
            [FromQuery] DateTime fecha,
            [FromQuery] int duracionMinutos = 60)
        {
            var slots = await _visitaService.GetAvailableSlotsAsync(agenteId, fecha, duracionMinutos);
            return Ok(slots);
        }

        [HttpPatch("{id}/confirmar")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<VisitaResponseDto>> ConfirmarVisita(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var visita = await _visitaService.ConfirmarVisitaAsync(id, usuarioId);
                return Ok(visita);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/cancelar")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<VisitaResponseDto>> CancelarVisita(int id, [FromBody] string motivo)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var visita = await _visitaService.CancelarVisitaAsync(id, motivo, usuarioId);
                return Ok(visita);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/reagendar")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<VisitaResponseDto>> ReagendarVisita(int id, [FromBody] DateTime nuevaFecha)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var visita = await _visitaService.ReagendarVisitaAsync(id, nuevaFecha, usuarioId);
                return Ok(visita);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/realizada")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<VisitaResponseDto>> MarcarComoRealizada(int id, [FromBody] string? notas = null)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var visita = await _visitaService.MarcarComoRealizadaAsync(id, notas, usuarioId);
                return Ok(visita);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("bulk-action")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<bool>> BulkAction(BulkVisitaActionDto bulkActionDto)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _visitaService.BulkActionAsync(bulkActionDto, usuarioId);
                return Ok(new { success = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<VisitaStatsDto>> GetVisitaStats(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int? agenteId = null)
        {
            var stats = await _visitaService.GetVisitaStatsAsync(fechaDesde, fechaHasta, agenteId);
            return Ok(stats);
        }

        [HttpPost("{id}/notification")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<IActionResult> SendNotification(int id, [FromQuery] string tipo = "Email")
        {
            try
            {
                await _visitaService.SendVisitaNotificationAsync(id, tipo);
                return Ok(new { message = "Notificación enviada correctamente" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{id}/ics")]
        public async Task<IActionResult> GenerateICSFile(int id)
        {
            try
            {
                var icsContent = await _visitaService.GenerateICSFileAsync(id);
                var bytes = System.Text.Encoding.UTF8.GetBytes(icsContent);
                
                return File(bytes, "text/calendar", $"visita-{id}.ics");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("validate-timeslot")]
        public async Task<ActionResult<bool>> ValidateTimeSlot([FromBody] ConflictCheckDto timeSlotDto)
        {
            var isValid = await _visitaService.IsValidTimeSlotAsync(
                timeSlotDto.AgenteId, 
                timeSlotDto.FechaHora, 
                timeSlotDto.DuracionMinutos);
            
            return Ok(new { isValid });
        }

        [HttpGet("business-hours")]
        public async Task<ActionResult<bool>> IsBusinessHour([FromQuery] DateTime fechaHora)
        {
            var isBusinessHour = await _visitaService.IsBusinessHourAsync(fechaHora);
            return Ok(new { isBusinessHour });
        }

        [HttpGet("{id}/historial")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<IEnumerable<VisitaAuditLogDto>>> GetHistorialVisita(int id)
        {
            var historial = await _auditoriaService.GetHistorialVisitaAsync(id);
            return Ok(historial);
        }

        [HttpGet("historial/usuario/{usuarioId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<VisitaAuditLogDto>>> GetHistorialUsuario(
            int usuarioId,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            var historial = await _auditoriaService.GetHistorialUsuarioAsync(usuarioId, fechaDesde, fechaHasta);
            return Ok(historial);
        }
    }
}