using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace Sitecore.Modules.SitemapXML
{
    internal static class ItemExtensions
    {
        public static IEnumerable<ID> ToIds(this Item item, string fieldName)
        {
            return item[fieldName].Split(new[] { "|" },
                StringSplitOptions.RemoveEmptyEntries).Select(x => new ID(x));

        }
    }
}