
using System;
using fb_messenger_bot_tt_emergencyservices.Handlers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace fb_messenger_bot_tt_emergencyservices
{
   /*
 * Message Event
 *
 * This event is called when a message is sent to your page. The 'message' 
 * object format can vary depending on the kind of message that was received.
 * Read more at https://developers.facebook.com/docs/messenger-platform/webhook-reference/message-received
 *
 * For this example, we're going to echo any text that we get. If we get some 
 * special keywords ('button', 'generic', 'receipt'), then we'll send back
 * examples of those bubbles to illustrate the special message bubbles we've 
 * created. If we receive a message with an attachment (image, video, audio), 
 * then we'll simply confirm that we've received the attachment.
 * 
 */
    public class MessageHandler<T> : IMessengerHandler
    {
        ILogger<T> _logger;
        IMessageSender _messageSender;
        string _serverUrl;
        public MessageHandler (ILogger<T> logger, IMessageSender messageSender, string serverUrl)
        {
            _logger = logger;
            _messageSender = messageSender;
            _serverUrl = serverUrl;
        }
        public bool MessageHandled(dynamic message)
        {
            var result = message.message != null;
            if (result)
            {
                string senderID = message.sender.id;
                string recipientID = message.recipient.id;
                string timeOfMessage = message.timestamp;
                dynamic text = message.message;
                _logger.LogInformation(
                    string.Format("Received message for user {0} and page {1} at {2} with message:", 
                        senderID, recipientID, timeOfMessage));
                string textData= text.ToString();
                _logger.LogInformation(textData);
                
                var isEcho = text.is_echo != null;
                string messageId = text.mid;
                string appId = text.app_id;
                var metadata = text.metadata;
                
                  // You may get a text or attachment but not both
                string messageText = text.text;
                var messageAttachments = text.attachments;
                var quickReply = text.quick_reply;

                if (isEcho) {
                    // Just logging message echoes to console
                    _logger.LogInformation(
                        string.Format("Received echo for message {0} and app {1} with metadata {2}", 
                    messageId, appId, (string)metadata));
                    return true;
                } else if (quickReply != null) {
                    var quickReplyPayload = quickReply.payload;
                    _logger.LogInformation(string.Format("Quick reply for message {0} with payload {1}",
                    messageId, (string)quickReplyPayload));

                    _messageSender.SendTextMessage(senderID, "Quick reply tapped");
                    return true;
                }

                if (messageText != null)
                {
                    // If we receive a text message, check to see if it matches any special
                    // keywords and send back the corresponding example. Otherwise, just echo
                    // the text we received.
                    switch (messageText)
                    {
                         case "account linking":
                            SendAccountLinking(senderID);
                            break;
                        default:
                             _messageSender.SendTextMessage(senderID, messageText);
                             break;

                    }
                }
                else if (messageAttachments != null)
                {
                    _messageSender.SendTextMessage(senderID, "Message with attachment received");                    
                }

                    
            }
            return result;
        }
        /*
        * Send a message with the account linking call-to-action
        *
        */
        private void SendAccountLinking(string recipientId)
        {

            var messageData = JObject.FromObject(new
            {
                recipient = new
                {
                    id= recipientId
                },
                message = new 
                {
                    attachment = new {
                        type = "template",
                        payload = new {
                        template_type =  "button",
                        text = "Welcome. Link your account.",
                        buttons = new[]{ new {
                            type = "account_link",
                            url = _serverUrl + "/authorize"
                        }}
                        }
                    }
                }
            });

            _messageSender.CallSendAPI(messageData);

        }
    }
}
