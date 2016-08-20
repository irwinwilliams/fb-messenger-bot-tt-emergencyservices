using System;
using System.Collections.Generic;
using System.IO;
using fb_messenger_bot_tt_emergencyservices.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace fb_messenger_bot_tt_emergencyservices.Controllers
{
    [Route("[controller]")]
    public class WebhookController : Controller, IMessageSender
    {
        private MessengerSettings Settings { get; set; }
        private readonly ILogger<WebhookController> _logger;
        private List<IMessengerHandler> _handlers;

        public WebhookController(
            IOptions<MessengerSettings> settings, 
            ILogger<WebhookController> logger)
        {            
            Settings = settings.Value;
            _logger = logger;
            _handlers = new List<IMessengerHandler>();
            _handlers.Add(new AuthenticationHandler<WebhookController>(_logger, this));
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

        // POST /webhook
        [HttpPost]
        public void Post([FromBody]dynamic data)
        {
            if (data["object"] == "page") {
                var entries = data["entry"];
                foreach (var pageEntry in entries)
                {
                    var pageID = pageEntry.id;
                    var timeOfEvent = pageEntry.time;
                    foreach (var messagingEvent in pageEntry["messaging"])
                    {
                        foreach (var handler in _handlers)
                        {
                            if (handler.MessageHandled(messagingEvent))
                                break;
                        }
                    }
                }  
            }

        }

        public void SendTextMessage(string senderId, string message)
        {
            _logger.LogInformation("Send: "+message);
            //throw new NotImplementedException();
        }
    }
}
