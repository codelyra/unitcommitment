using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UnitCommitment.Models;

namespace UnitCommitment.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductionPlanController : ControllerBase
    {
        private readonly ILogger<ProductionPlanController> _logger;

        public ProductionPlanController(ILogger<ProductionPlanController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public ActionResult<ProductionPayload> Get([FromBody] ProductionPayload payload)
        {
            _logger.LogInformation("Index page says hello");
            return payload;
        }
    }
}
