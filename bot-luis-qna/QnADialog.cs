using System;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;
using System.Net;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace bot_luis_qna.Dialogs
{
    public class QnADialog
    {
        public QnADialog() { }

        private class QnAMakerResult
        {
            public IList<Response> answers { get; set; }
        }

        private class Response
        {
            public string answer { get; set; }
            public IList<string> questions { get; set; }
            public double score { get; set; }
        }

        private static string knowledgebaseId = "{Your knowledgebase ID}"; // Use knowledge base id created.
        private static string qnamakerSubscriptionKey = "Your subscription key"; //Use subscription key assigned to you.

        /// <summary>
        /// Try to query a question and get the answer > 50 points
        /// </summary>
        /// <param name="question">The question you want to answer</param>
        /// <param name="answer">The answer you might get</param>
        /// <returns>If get an answer which scores more than 50</returns>
        public bool TryQuery(string question, out string answer)
        {
            string responseString = string.Empty;
            answer = string.Empty;
            var query = question; //User Query

            //Build the URI
            Uri qnamakerUriBase = new Uri("https://westus.api.cognitive.microsoft.com/qnamaker/v2.0");
            var builder = new UriBuilder($"{qnamakerUriBase}/knowledgebases/{knowledgebaseId}/generateAnswer");

            //Add the question as part of the body
            var postBody = $"{{\"question\": \"{query}\"}}";

            //Send the POST request
            using (WebClient client = new WebClient())
            {
                //Set the encoding to UTF8
                client.Encoding = System.Text.Encoding.UTF8;
                try
                {
                    //Add the subscription key header
                    client.Headers.Add("Ocp-Apim-Subscription-Key", qnamakerSubscriptionKey);
                    client.Headers.Add("Content-Type", "application/json");
                    responseString = client.UploadString(builder.Uri, postBody);
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var resp = ex.Response as HttpWebResponse;
                        if (resp != null)
                        {
                            Debug.WriteLine("HTTP Status Code: " + (int)resp.StatusCode);
                        }
                        else
                        {

                        }
                    }
                    else
                    {

                    }
                    return false;
                }
            }

            QnAMakerResult response;
            try
            {
                response = JsonConvert.DeserializeObject<QnAMakerResult>(responseString);
            }
            catch
            {
                return false;
                throw new Exception("Unable to deserialize QnA Maker response string.");
            }

            try
            {
                Debug.WriteLine(response.answers[0].answer + response.answers[0].score.ToString());
                answer = response.answers[0].answer;
                return response.answers[0].score > 50 ? true : false;
            }
            catch (Exception)
            {
                return false;
                throw;
            }

        }

        /// <summary>
        /// Add QnA pairs to your knowledgebase and retrain
        /// </summary>
        /// <param name="question">The question</param>
        /// <param name="answer">The answer to the given quesiton</param>
        /// <returns>Whether adding pair to QnA Maker successfully</returns>
        public bool AddPairs(string question, string answer)
        {
            string responseString = string.Empty;

            Uri qnamakerUriBase = new Uri("https://westus.api.cognitive.microsoft.com/qnamaker/v2.0");

            var builder = new UriBuilder($"{qnamakerUriBase}/knowledgebases/{knowledgebaseId}");

            //Add the question as part of the body
            var postBody = string.Format("{{\"add\": {{\"qnaPairs\": [{{\"answer\": \"{0}\",\"question\": \"{1}\" }} ] }} }}", answer, question);

            //Send the PATCH request
            using (WebClient client = new WebClient())
            {
                //Set the encoding to UTF8
                client.Encoding = System.Text.Encoding.UTF8;
                try
                {
                    //Add the subscription key header
                    client.Headers.Add("Ocp-Apim-Subscription-Key", qnamakerSubscriptionKey);
                    //client.Headers.Add("Content-Type", "application/json");
                    client.UploadString(builder.Uri, "PATCH", postBody);
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null)
                        {
                            Debug.WriteLine("HTTP Status Code: " + (int)response.StatusCode);
                        }
                        else
                        {

                        }
                    }
                    else
                    {

                    }
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Publish your retrained knowledgebase
        /// </summary>
        /// <returns>Whether published successully</returns>
        public bool Publish()
        {
            string responseString = string.Empty;

            Uri qnamakerUriBase = new Uri("https://westus.api.cognitive.microsoft.com/qnamaker/v2.0");

            var builder = new UriBuilder($"{qnamakerUriBase}/knowledgebases/{knowledgebaseId}");

            using (WebClient client = new WebClient())
            {
                //Set the encoding to UTF8
                client.Encoding = System.Text.Encoding.UTF8;
                try
                {
                    //Add the subscription key header
                    client.Headers.Add("Ocp-Apim-Subscription-Key", qnamakerSubscriptionKey);
                    //client.Headers.Add("Content-Type", "application/json");
                    client.UploadString(builder.Uri, "PUT", "");
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null)
                        {
                            Debug.WriteLine("HTTP Status Code: " + (int)response.StatusCode);
                        }
                        else
                        {

                        }
                    }
                    else
                    {

                    }
                    return false;
                }
            }
            return true;
        }

    }
}