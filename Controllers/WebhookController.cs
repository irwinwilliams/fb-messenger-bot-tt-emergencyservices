using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace fb_messenger_bot_tt_emergencyservices.Controllers
{
    [Route("[controller]")]
    public class WebhookController : Controller
    {
        private MessengerSettings Settings { get; set; }
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            IOptions<MessengerSettings> settings, 
            ILogger<WebhookController> logger)
        {            
            Settings = settings.Value;
            _logger = logger;
        }
        [HttpGet]
        public string Get()
        {
            var req = Request;
            var res = Response;
            var result = string.Empty;
            if (req.Query["hub.mode"] == "subscribe"
                && req.Query["hub.verify_token"] == Settings.FBValidationToken) 
            {
                _logger.LogInformation("Validating webhook");
                res.StatusCode = 200;
                result = req.Query["hub.challenge"];
            }
            else 
            {
                _logger.LogError("Failed validation. Make sure the validation tokens match.");
                res.StatusCode = 403;          
            }
            return result;  
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
