using FormService.Models;
using FormService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FormService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionsController(IFormProcessor formProcessor) : ControllerBase
    {
        private readonly IFormProcessor _formProcessor = formProcessor;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FormSubmissionDTO submission)
        {
            try
            {
                var id = await _formProcessor.ProcessAndStoreAsync(submission);
                return Ok(new { id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("all/{formType}")]
        public async Task<IActionResult> GetAll(string? formType)
        {
            var submissions = await _formProcessor.GetAllAsync(formType);
            return Ok(submissions);
        }

        [HttpGet("{formType}/{id}")]
        public async Task<IActionResult> GetById(string formType, Guid id)
        {
            var submission = await _formProcessor.GetByIdAsync(formType, id);
            return submission != null ? Ok(submission) : NotFound();
        }

        [HttpDelete("{formType}/{id}")]
        public async Task<IActionResult> Delete(string formType, Guid id)
        {
            await _formProcessor.DeleteAsync(formType, id);
            return Ok();
        }
    }
}