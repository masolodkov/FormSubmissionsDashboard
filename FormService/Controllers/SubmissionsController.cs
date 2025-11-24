using FormService.Models;
using FormService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FormService.Controllers
{
    /// <summary>
    /// Form Submission API - Handles form creation, retrieval, search and management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionsController(IFormProcessor formProcessor) : ControllerBase
    {
        private readonly IFormProcessor _formProcessor = formProcessor;

        /// <summary>
        /// Submit a new form
        /// </summary>
        /// <param name="submission">Form submission data</param>
        /// <returns>Submission ID</returns>
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

        /// <summary>
        /// Get paginated form submissions
        /// </summary>
        /// <param name="formType">Type of form</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated list of submissions</returns>
        [HttpGet("all/{formType}")]
        public async Task<IActionResult> GetAll(string formType, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _formProcessor.GetAllAsync(formType, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get a submitted form data
        /// </summary>
        /// <param name="formType">Type of form</param>
        /// <param name="id">Submission Id</param>
        /// <returns></returns>
        [HttpGet("{formType}/{id}")]
        public async Task<IActionResult> GetById(string formType, Guid id)
        {
            var submission = await _formProcessor.GetByIdAsync(formType, id);
            return submission != null ? Ok(submission) : NotFound();
        }

        /// <summary>
        /// Delete a submission
        /// </summary>
        /// <param name="formType">Type of form</param>
        /// <param name="id">Submission id</param>
        /// <returns></returns>
        [HttpDelete("{formType}/{id}")]
        public async Task<IActionResult> Delete(string formType, Guid id)
        {
            await _formProcessor.DeleteAsync(formType, id);
            return Ok();
        }

        /// <summary>
        /// Advanced search with filtering and pagination
        /// </summary>
        /// <param name="request">Search criteria including field filters</param>
        /// <returns>Paginated search results</returns>
        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] FormSearchRequest request)
        {
            try
            {
                var result = await _formProcessor.SearchAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #region === FOR TEST PURPOSE ===

        /// <summary>
        /// Generates a random submissions to test
        /// </summary>
        /// <param name="count">Number of submissions to generate</param>
        /// <returns>Generated submissions</returns>
        [HttpPost("testData")]
        public async Task<IActionResult> GenerateTestData([FromQuery] int count)
        {
            try
            {
                var testSubmissions = GenerateRandomSubmissions(count);
                var results = new List<FormSubmission>();

                foreach (var submission in testSubmissions)
                {
                    var record = await _formProcessor.ProcessAndStoreAsync(submission);
                    if (record != null)
                    {
                        results.Add(record);
                    }
                }

                return Ok(results?.Count ?? 0);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private List<FormSubmissionDTO> GenerateRandomSubmissions(int count)
        {
            var random = new Random();
            var submissions = new List<FormSubmissionDTO>();

            // Sample data for random generation
            var firstNames = new[] { "John", "Jane", "Michael", "Sarah", "David", "Emily", "Robert", "Lisa", "William", "Maria" };
            var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
            var countries = new[] { "ru", "us", "de", "gb" };
            var genders = new[] { "male", "female" };

            for (int i = 0; i < count; i++)
            {
                var firstName = firstNames[random.Next(firstNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];

                // Generate random birth date (between 18 and 80 years ago)
                var startDate = DateTime.Now.AddYears(-80);
                var endDate = DateTime.Now.AddYears(-18);
                var timeSpan = endDate - startDate;
                var randomSpan = new TimeSpan((long)(random.NextDouble() * timeSpan.Ticks));
                var birthDate = startDate + randomSpan;

                // Generate random submission date (within last 30 days)
                var submittedAt = DateTime.Now.AddDays(-random.Next(30));

                var formData = new
                {
                    name = $"{firstName} {lastName}",
                    country = countries[random.Next(countries.Length)],
                    birthDate = birthDate.ToString("yyyy-MM-dd"),
                    gender = genders[random.Next(genders.Length)],
                    newsletter = random.Next(2) == 1 // 50% chance
                };

                var submission = new FormSubmissionDTO
                {
                    FormType = "ContactForm",
                    SubmittedAt = submittedAt,
                    FormData = System.Text.Json.JsonSerializer.Serialize(formData)
                };

                submissions.Add(submission);
            }

            return submissions;
        }

        #endregion === FOR TEST PURPOSE ===

    }
}