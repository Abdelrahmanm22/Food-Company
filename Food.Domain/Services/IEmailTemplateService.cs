using System.Collections.Generic;
using System.Threading.Tasks;

namespace Food.Domain.Services
{
    public interface IEmailTemplateService
    {
        /// <summary>
        /// Loads an HTML template file by name from the EmailTemplates directory.
        /// Results are cached in memory after the first load.
        /// </summary>
        /// <param name="templateName">File name without path, e.g. "NewSessionEmail.html"</param>
        string LoadTemplate(string templateName);

        /// <summary>
        /// Replaces {{Placeholder}} tokens in the template with the provided values.
        /// </summary>
        string PopulateTemplate(string template, Dictionary<string, string> placeholders);
    }
}
