using System.Collections.Generic;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace Sitecore.Modules.SitemapXML.Filters
{
    public class TemplateFilter : IItemFilter
    {
        private readonly IEnumerable<ID> _allowedTemplates;

        public TemplateFilter()
        {
            _allowedTemplates = SitemapManagerConfiguration.ConfigItem.ToIds("Enabled templates");
        }

        public bool IsValid(Item item)
        {
            var valid = _allowedTemplates.Contains(item?.TemplateID);
            return valid;
        }
    }
}