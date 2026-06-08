using System.Web.Http;
using JobPortal.BLL.Interfaces;
using JobPortal.PL.Filters;

namespace JobPortal.PL.Controllers
{
    [ApiExceptionFilter]
    [RoutePrefix("api/applications")]
    public class JobApplicationsController : ApiController
    {
        private readonly IJobApplicationService _applicationService;
        public JobApplicationsController(IJobApplicationService service) { _applicationService = service; }

        [HttpPost]
        [Route("connect")]
        public IHttpActionResult Connect([FromUri] int resumeId, [FromUri] int vacancyId, [FromUri] string initiatorRole)
        {
            _applicationService.ApplyForVacancy(resumeId, vacancyId, initiatorRole);
            return Ok("Зв'язок успішно зареєстровано");
        }

        [HttpGet]
        [Route("vacancy/{vacancyId}")]
        public IHttpActionResult GetByVacancy(int vacancyId)
        {
            return Ok(_applicationService.GetApplicationsForVacancy(vacancyId));
        }

        [HttpGet]
        [Route("resume/{resumeId}")]
        public IHttpActionResult GetByResume(int resumeId)
        {
            return Ok(_applicationService.GetApplicationsForResume(resumeId));
        }
    }
}