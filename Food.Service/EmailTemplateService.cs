using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Food.Domain.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Food.Service
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<EmailTemplateService> _logger;

        // In-memory cache so each template file is read from disk only once
        private readonly ConcurrentDictionary<string, string> _templateCache = new();

        public EmailTemplateService(IWebHostEnvironment env, ILogger<EmailTemplateService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public string LoadTemplate(string templateName)
        {
            return _templateCache.GetOrAdd(templateName, name =>
            {
                var path = Path.Combine(_env.WebRootPath, "EmailTemplates", name);

                if (!File.Exists(path))
                {
                    _logger.LogError("Email template not found at path: {Path}", path);
                    return string.Empty;
                }

                return File.ReadAllText(path);
            });
        }

        public string PopulateTemplate(string template, Dictionary<string, string> placeholders)
        {
            foreach (var (key, value) in placeholders)
                template = template.Replace($"{{{{{key}}}}}", value ?? string.Empty);

            return template;
        }
    }
}
