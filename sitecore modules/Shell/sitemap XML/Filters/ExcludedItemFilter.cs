using System.Collections.Generic;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace Sitecore.Modules.SitemapXML.Filters
{
    public class ExcludedItemFilter : IItemFilter
    {
        private readonly IEnumerable<ID> _excludedItems;

        public ExcludedItemFilter()
        {
            _excludedItems = SitemapManagerConfiguration.ConfigItem.ToIds("Exclude items");
        }

        public bool IsValid(Item item)
        {
            var valid = !_excludedItems.Contains(item?.ID);
            return valid;
        }
    }
}