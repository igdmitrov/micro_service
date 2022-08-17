using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using micro_service_shared;
using micro_service_shared.Command;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace micro_service_webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IBusClient _busClient;

        public ReportController(IBusClient busClient)
        {
            _busClient = busClient ?? throw new ArgumentNullException(nameof(busClient));
        }

        [AllowAnonymous]
        [HttpPost]
        [Route(nameof(Generate))]
        public IActionResult Generate()
        {
            _busClient.Publish<GenerateReportCommand>(new GenerateReportCommand(DateTime.Now), default);

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route(nameof(Get))]
        public IActionResult Get()
        {
            var result = _busClient.GetMessage<SubmitReportCommand>(nameof(SubmitReportCommand), default);

            if(result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
