using Sitecore.Data.Items;

namespace Sitecore.Modules.SitemapXML.Filters
{
    public interface IItemFilter
    {
        bool IsValid(Item item);
    }
}