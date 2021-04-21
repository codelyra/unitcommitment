using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UnitCommitment.Models;
using UnitCommitment.Services;

namespace UnitCommitment.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductionPlanController : ControllerBase
    {
        private readonly ILogger<ProductionPlanController> _logger;
        //private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;

        public ProductionPlanController(ILogger<ProductionPlanController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _appSettings = new AppSettings();
            configuration.GetSection("UnitCommitment").Bind(_appSettings);
        }

        [HttpPost]
        public ActionResult<List<Commitment>> Schedule([FromBody] Payload payload)
        {
            _logger.LogInformation("controller started");

            CommitmentService commitmentService = new CommitmentService(payload, _appSettings);
            try
            {
                return commitmentService.CommitPoweplants();
            }
            catch(Exception exc)
            {
                _logger.LogError(exc.Message);
                return null;
            }
        }
    }
}
