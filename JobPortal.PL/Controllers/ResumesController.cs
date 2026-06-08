using System.Web.Http;
using JobPortal.BLL.DTOs;
using JobPortal.BLL.Interfaces;
using JobPortal.PL.Filters;

namespace JobPortal.PL.Controllers
{
    [ApiExceptionFilter]
    [RoutePrefix("api/resumes")]
    public class ResumesController : ApiController
    {
        private readonly IResumeService _resumeService;
        public ResumesController(IResumeService service) { _resumeService = service; }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll([FromUri] string search = null, [FromUri] string sortBy = null)
        {
            return Ok(_resumeService.GetResumes(search, sortBy));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Create([FromBody] ResumeDto dto)
        {
            _resumeService.CreateResume(dto);
            return StatusCode(System.Net.HttpStatusCode.Created);
        }

        [HttpGet]
        [Route("matched-for-vacancy/{vacancyId}")]
        public IHttpActionResult GetMatched(int vacancyId)
        {
            return Ok(_resumeService.GetResumesByVacancyRequirements(vacancyId));
        }

        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult Update(int id, [FromBody] ResumeDto dto)
        {
            dto.Id = id;
            _resumeService.UpdateResume(dto);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult Delete(int id, [FromUri] int requesterId)
        {
            _resumeService.DeleteResume(id, requesterId);
            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }
    }
}