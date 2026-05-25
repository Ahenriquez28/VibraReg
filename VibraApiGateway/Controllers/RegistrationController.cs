using Microsoft.AspNetCore.Mvc;
using VibraApiGateway.Interfaces;
using VibraApiGateway.DTOs;

namespace VibraApiGateway.Controllers
{
    [ApiController]
    [Route("api")]
    public class AdminController : ControllerBase
    {
        private readonly IRegistrationProxy _registrationProxy;
        
        public AdminController(IRegistrationProxy registrationProxy)
        {
            _registrationProxy = registrationProxy;
        }
        
        [HttpGet("getTeams")]
        public async Task<IActionResult> GetTeams()
        {
            try
            {
                var authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var result = await _registrationProxy.GetTeamsAsync(string.IsNullOrEmpty(authToken) ? null : authToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("registration")]  // ✅ This makes the endpoint /api/registration
        public async Task<IActionResult> Register([FromForm] RegisterDTO dto)
        {
            try
            {
                var result = await _registrationProxy.RegisterAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost("updateTeams")]
        public async Task<IActionResult> UpdateTeams([FromBody] object dto)
        {
            try
            {
                var authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var result = await _registrationProxy.UpdateTeamsAsync(dto, authToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        
        [HttpPost("removeStudents")]
        public async Task<IActionResult> RemoveStudents([FromBody] object dto)
        {
            try
            {
                var authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var result = await _registrationProxy.RemoveStudentsAsync(dto, authToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        
        [HttpPost("removeTeam")]
        public async Task<IActionResult> RemoveTeam([FromBody] object dto)
        {
            try
            {
                var authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var result = await _registrationProxy.RemoveTeamAsync(dto, authToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        
        [HttpPost("createTeam")]
        public async Task<IActionResult> CreateTeam([FromBody] object dto)
        {
            try
            {
                var authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var result = await _registrationProxy.CreateTeamAsync(dto, authToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("togglePresent")]
        public async Task<IActionResult> TogglePresent([FromBody] object dto)
        {
            try
            {
                var authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var result = await _registrationProxy.TogglePresentAsync(dto, authToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("confirm/{token}")]
        public async Task<IActionResult> ConfirmAttendance(string token)
        {
            var result = await _registrationProxy.ConfirmAttendanceAsync(token);
            return Ok(result);
        }

        [HttpGet("team-names")]
        public async Task<IActionResult> GetTeamNames()
        {
            try
            {
                var result = await _registrationProxy.GetTeamNamesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("trigger-confirmation-emails")]
        public async Task<IActionResult> TriggerConfirmationEmails()
        {
            try
            {
                var result = await _registrationProxy.TriggerConfirmationEmailsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}