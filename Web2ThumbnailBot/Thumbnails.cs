using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Connector;
using RestSharp;

namespace Web2ThumbnailBot
{
    public class Thumbnails
    {

        public static async Task ProcessScreenshot(ConnectorClient connector, Activity msg)
        {
            Activity reply = msg.CreateReply($"Processing: {msg.Text}");
            await connector.Conversations.ReplyToActivityAsync(reply);

            await Task.Run(async () =>
            {

                if (GetThumbnail(msg.Text)) {
                    reply = CreateResponseCard(msg, constants.cStrThumbApi + constants.cStrApiParms + msg.Text);
                } else
                {
                    reply = msg.CreateReply($"Your request has exceeded the required limits, please try again later { msg.Text}");
                }
                

                await connector.Conversations.ReplyToActivityAsync(reply);
            });
        }

        public static bool GetThumbnail(string url)
        {

            RestClient rc = new RestClient(constants.cStrThumbApi);
            RestRequest rq = new RestRequest(constants.cStrApiParms + url, Method.GET);

            IRestResponse response = rc.Execute(rq);
            int _retry = 0;
            while(response.StatusCode!=System.Net.HttpStatusCode.OK && _retry<=10)
            {
                _retry++;
                response = rc.Execute(rq);
                System.Threading.Thread.Sleep(3000);
            }

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }

        public static Activity CreateResponseCard(Activity msg, string imgUrl)
        {
            Activity reply = msg.CreateReply(imgUrl);

            reply.Recipient = msg.From; reply.Type = "message"; reply.Attachments = new List<Attachment>();

            List<CardImage> cardImages = new List<CardImage>();

            cardImages.Add(new CardImage(imgUrl));

            ThumbnailCard plCard = new ThumbnailCard()

            { Subtitle = msg.Text, Images = cardImages };

            Attachment plAttachment = plCard.ToAttachment(); reply.Attachments.Add(plAttachment);

            return reply;
        }

    }
}