using LandingBack.Data.Dtos;
using LandingBack.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LandingBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeadController : ControllerBase
    {
        private readonly ILeadService _leadService;
        private readonly ILogger<LeadController> _logger;

        public LeadController(ILeadService leadService, ILogger<LeadController> logger)
        {
            _leadService = leadService;
            _logger = logger;
        }

        // GET: api/lead
        [HttpGet]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<object>> GetLeads([FromQuery] LeadSearchDto searchDto)
        {
            try
            {
                var (leads, totalCount) = await _leadService.GetLeadsAsync(searchDto);
                
                return Ok(new
                {
                    Data = leads,
                    TotalCount = totalCount,
                    Pagina = searchDto.Page,
                    TamanoPagina = searchDto.PageSize,
                    TotalPaginas = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener leads");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/lead/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<LeadResponseDto>> GetLead(int id)
        {
            try
            {
                var lead = await _leadService.GetLeadByIdAsync(id);
                return Ok(lead);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lead {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/lead
        [HttpPost]
        public async Task<ActionResult<LeadResponseDto>> CreateLead([FromBody] LeadCreateDto leadCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Capturar información del request
                leadCreateDto.IpAddress = GetClientIpAddress();
                leadCreateDto.UserAgent = Request.Headers.UserAgent.ToString();

                var lead = await _leadService.CreateLeadAsync(leadCreateDto);
                return CreatedAtAction(nameof(GetLead), new { id = lead.Id }, lead);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear lead");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/lead/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<LeadResponseDto>> UpdateLead(int id, [FromBody] LeadUpdateDto leadUpdateDto)
        {
            try
            {
                if (id != leadUpdateDto.Id)
                    return BadRequest("El ID no coincide");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var lead = await _leadService.UpdateLeadAsync(id, leadUpdateDto);
                return Ok(lead);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar lead {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/lead/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLead(int id)
        {
            try
            {
                await _leadService.DeleteLeadAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar lead {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/lead/assign
        [HttpPost("assign")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<LeadResponseDto>> AssignLead([FromBody] LeadAssignDto assignDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var usuarioId = int.TryParse(usuarioIdStr, out var id) ? id : 0;

                var lead = await _leadService.AssignLeadAsync(assignDto, usuarioId);
                return Ok(lead);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar lead");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/lead/status
        [HttpPut("status")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<LeadResponseDto>> UpdateLeadStatus([FromBody] LeadStatusUpdateDto statusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var usuarioId = int.TryParse(usuarioIdStr, out var id) ? id : 0;

                var lead = await _leadService.UpdateLeadStatusAsync(statusDto, usuarioId);
                return Ok(lead);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado del lead");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/lead/agente/{agenteId}
        [HttpGet("agente/{agenteId}")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<IEnumerable<LeadResponseDto>>> GetLeadsByAgente(int agenteId)
        {
            try
            {
                var leads = await _leadService.GetLeadsByAgenteAsync(agenteId);
                return Ok(leads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener leads del agente {AgenteId}", agenteId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/lead/unassigned
        [HttpGet("unassigned")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<IEnumerable<LeadResponseDto>>> GetUnassignedLeads()
        {
            try
            {
                var leads = await _leadService.GetUnassignedLeadsAsync();
                return Ok(leads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener leads sin asignar");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/lead/propiedad/{propiedadId}
        [HttpGet("propiedad/{propiedadId}")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<IEnumerable<LeadResponseDto>>> GetLeadsByPropiedad(int propiedadId)
        {
            try
            {
                var leads = await _leadService.GetLeadsByPropiedadAsync(propiedadId);
                return Ok(leads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener leads de propiedad {PropiedadId}", propiedadId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/lead/stats
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<LeadStatsDto>> GetLeadStats(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                var stats = await _leadService.GetLeadStatsAsync(fechaDesde, fechaHasta);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de leads");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/lead/bulk-action
        [HttpPost("bulk-action")]
        [Authorize(Roles = "Admin,Agente")]
        public async Task<ActionResult<bool>> BulkAction([FromBody] BulkLeadActionDto bulkActionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var usuarioId = int.TryParse(usuarioIdStr, out var id) ? id : 0;

                var result = await _leadService.BulkActionAsync(bulkActionDto, usuarioId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en acción masiva de leads");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/lead/form-contact
        [HttpPost("form-contact")]
        public async Task<ActionResult<object>> FormContact([FromBody] LeadCreateDto leadCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Capturar información del request para formularios web
                leadCreateDto.Canal = "Web";
                leadCreateDto.Origen = Request.Headers.Referer.ToString().Contains("utm_source") 
                    ? ExtractUtmSource(Request.Headers.Referer.ToString()) 
                    : "direct";
                leadCreateDto.IpAddress = GetClientIpAddress();
                leadCreateDto.UserAgent = Request.Headers.UserAgent.ToString();

                var lead = await _leadService.CreateLeadAsync(leadCreateDto);
                
                // Respuesta simplificada para formularios públicos
                return Ok(new
                {
                    Success = true,
                    Message = "Consulta enviada exitosamente. Nos contactaremos contigo pronto.",
                    LeadId = lead.Id,
                    WhatsAppUrl = GenerateWhatsAppUrl(lead)
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar formulario de contacto");
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        // GET: api/lead/export
        [HttpGet("export")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ExportLeads([FromQuery] LeadSearchDto searchDto)
        {
            try
            {
                var (leads, _) = await _leadService.GetLeadsAsync(searchDto);
                
                // Aquí puedes implementar la lógica de exportación (CSV, Excel, etc.)
                // Por ahora retornamos JSON
                return Ok(leads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar leads");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        #region Métodos privados

        private string GetClientIpAddress()
        {
            string? ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(ipAddress))
                ipAddress = Request.Headers["X-Real-IP"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(ipAddress))
                ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();

            return ipAddress ?? "unknown";
        }

        private string ExtractUtmSource(string referer)
        {
            try
            {
                var uri = new Uri(referer);
                var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                return queryParams["utm_source"] ?? "direct";
            }
            catch
            {
                return "direct";
            }
        }

        private string GenerateWhatsAppUrl(LeadResponseDto lead)
        {
            var message = $"Hola! Me interesa la propiedad {lead.PropiedadCodigo} en {lead.PropiedadDireccion}. Mi nombre es {lead.Nombre}.";
            var encodedMessage = Uri.EscapeDataString(message);
            
            // Número de WhatsApp de la inmobiliaria (configurar en appsettings)
            var whatsappNumber = "5491234567890"; // Cambiar por número real
            
            return $"https://wa.me/{whatsappNumber}?text={encodedMessage}";
        }

        #endregion
    }
}