using System.Web.Http;
using JobPortal.BLL.DTOs;
using JobPortal.BLL.Interfaces;
using JobPortal.PL.Filters;

namespace JobPortal.PL
{
    [ApiExceptionFilter]
    [RoutePrefix("api/vacancies")]
    public class VacanciesController : ApiController
    {
        private readonly IVacancyService _vacancyService;
        public VacanciesController(IVacancyService service) { _vacancyService = service; }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll([FromUri] string search = null, [FromUri] string sortBy = null, [FromUri] decimal? minSalary = null)
        {
            return Ok(_vacancyService.GetVacancies(search, sortBy, minSalary));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Create([FromBody] VacancyDto dto)
        {
            _vacancyService.CreateVacancy(dto);
            return StatusCode(System.Net.HttpStatusCode.Created);
        }

        [HttpGet]
        [Route("matched-for-resume/{resumeId}")]
        public IHttpActionResult GetMatched(int resumeId)
        {
            return Ok(_vacancyService.GetVacanciesByResumeSkills(resumeId));
        }

        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult Update(int id, [FromBody] VacancyDto dto)
        {
            dto.Id = id;
            _vacancyService.UpdateVacancy(dto);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult Delete(int id, [FromUri] int requesterId)
        {
            _vacancyService.DeleteVacancy(id, requesterId);
            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }
    }
}