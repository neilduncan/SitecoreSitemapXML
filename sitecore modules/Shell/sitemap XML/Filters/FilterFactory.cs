using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Xml;

namespace Sitecore.Modules.SitemapXML.Filters
{
    public static class FilterFactory
    {
        public static IEnumerable<IItemFilter> GetFilters()
        {
            return Factory.GetConfigNodes("sitemapVariables/filters/filter")
                .Cast<XmlNode>()
                .Where(node => !string.IsNullOrEmpty(XmlUtil.GetAttribute("type", node)))
                .Select(node => Factory.CreateType(node, true))
                .Select(Activator.CreateInstance)
                .Cast<IItemFilter>();
        }
    }
}