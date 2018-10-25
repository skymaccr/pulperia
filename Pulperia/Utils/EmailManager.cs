using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Pulperia.Utils
{
    /// <summary>
    /// Description: Get the email template from sendgrid
    /// Date            Ticket          Email                                   Description
    /// 03/01/2018      JJB-3524        rmunoz@cecropiasolutions.com            Creation 
    /// </summary>
    public class Templates
    {
        /// <summary>
        /// The client
        /// </summary>
        private HttpClient client = new HttpClient();

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <param name="templateId">The template identifier.</param>
        /// <returns></returns>
        public string GetHTMLTemplate(string templateId)
        {
            var task = Task.Run<string>(async () => await GetTemplate(templateId));
            return task.Result;
        }

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <param name="templateId">The template identifier.</param>
        /// <returns></returns>       
        private async Task<string> GetTemplate(string templateId)
        {
            string result = null;
            SendGridTemplate template = null;
            var path = "https://api.sendgrid.com/v3/templates/" + templateId;
            client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(Encoding.ASCII.GetBytes(
                            $"{ConfigurationManager.AppSettings.Get("SendGridUserName")}:{ConfigurationManager.AppSettings.Get("SendGridPassword")}")));

            var response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                var strTemplate = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(strTemplate))
                    template = JsonConvert.DeserializeObject<SendGridTemplate>(strTemplate);
            }

            if (template != null)
                result = template.Versions.Where(x => x.Active.Equals("1")).Select(t => t.Html_content).FirstOrDefault();

            return result;
        }
    }

    /// <summary>
    /// The sendgrid template
    /// </summary>
    internal class SendGridTemplate
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the versions.
        /// </summary>
        /// <value>
        /// The versions.
        /// </value>
        public List<TemplateVersion> Versions { get; set; }
    }

    /// <summary>
    /// the sendgrid version template
    /// </summary>
    internal class TemplateVersion
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public string User_id { get; set; }
        /// <summary>
        /// Gets or sets the template identifier.
        /// </summary>
        /// <value>
        /// The template identifier.
        /// </value>
        public string Template_id { get; set; }
        /// <summary>
        /// Gets or sets the active.
        /// </summary>
        /// <value>
        /// The active.
        /// </value>
        public string Active { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the content of the HTML.
        /// </summary>
        /// <value>
        /// The content of the HTML.
        /// </value>
        public string Html_content { get; set; }
        /// <summary>
        /// Gets or sets the content of the plain.
        /// </summary>
        /// <value>
        /// The content of the plain.
        /// </value>
        public string Plain_content { get; set; }
        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject { get; set; }
        /// <summary>
        /// Gets or sets the updated at.
        /// </summary>
        /// <value>
        /// The updated at.
        /// </value>
        public string Updated_at { get; set; }
        /// <summary>
        /// Gets or sets the editor.
        /// </summary>
        /// <value>
        /// The editor.
        /// </value>
        public string Editor { get; set; }
    }
}