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


        [HttpPost("testData")]
        public async Task<IActionResult> GenerateTestData()
        {
            try
            {
                var testSubmissions = GenerateRandomSubmissions(20);
                var results = new List<FormSubmission>();

                foreach (var submission in testSubmissions)
                {
                    var record = await _formProcessor.ProcessAndStoreAsync(submission);
                    if (record != null)
                    {
                        results.Add(record);
                    }
                }

                return Ok(results);
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

    }
}