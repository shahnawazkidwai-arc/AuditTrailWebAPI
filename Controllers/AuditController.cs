using audit_trail_.NET_Core_Web_API.Models;
using audit_trail_.NET_Core_Web_API.Repositories;
using audit_trail_.NET_Core_Web_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace audit_trail_.NET_Core_Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _svc;

        public AuditController(IAuditService svc)
        {
            _svc = svc;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAudit([FromBody] AuditRequest request, CancellationToken cancellationToken)
        {
            if (request == null) return BadRequest("Request body missing.");
            if (string.IsNullOrWhiteSpace(request.EntityName)) return BadRequest("EntityName required.");
            if (string.IsNullOrWhiteSpace(request.UserId)) return BadRequest("UserId required.");

            var record = await _svc.CreateAuditAsync(request, cancellationToken);

            // Return 201 with created entity (or you could return a location header)
            return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromServices] IAuditRepository repo, Guid id, CancellationToken cancellationToken)
        {
            var rec = await repo.GetAsync(id, cancellationToken);
            if (rec == null) return NotFound();
            return Ok(rec);
        }
    }
}
