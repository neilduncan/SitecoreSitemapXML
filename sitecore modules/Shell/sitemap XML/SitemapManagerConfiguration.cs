/* *********************************************************************** *
 * File   : SitemapManagerConfiguration.cs                Part of Sitecore *
 * Version: 1.0.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Class for getting config information from db and conf file     *
 *                                                                         *
 * Copyright (C) 1999-2009 by Sitecore A/S. All rights reserved.           *
 *                                                                         *
 * This work is the property of:                                           *
 *                                                                         *
 *        Sitecore A/S                                                     *
 *        Meldahlsgade 5, 4.                                               *
 *        1613 Copenhagen V.                                               *
 *        Denmark                                                          *
 *                                                                         *
 * This is a Sitecore published work under Sitecore's                      *
 * shared source license.                                                  *
 *                                                                         *
 * *********************************************************************** */

using System.Collections.Generic;
using System.Xml;
using Sitecore.Configuration;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Xml;

namespace Sitecore.Modules.SitemapXML
{
    public class SitemapManagerConfiguration
    {
        #region properties
        public static string XmlnsTpl => GetValueByName("xmlnsTpl");
        public static string WorkingDatabase => GetValueByName("database");
        public static string SitemapConfigurationItemPath => GetValueByName("sitemapConfigurationItemPath");
        public static bool IsProductionEnvironment => GetBoolValueByName("productionEnvironment");
        public static bool GenerateRobotsFile => GetBoolValueByName("generateRobotsFile");
        public static Item ConfigItem => Factory.GetDatabase(WorkingDatabase).GetItem(SitemapConfigurationItemPath);
        #endregion properties

        private static string GetValueByName(string name)
        {
            var node = Factory.GetConfigNodes("sitemapVariables/sitemapVariable")
                .Cast<XmlNode>()
                .FirstOrDefault(x => XmlUtil.GetAttribute("name", x) == name);

            return node != null ? XmlUtil.GetAttribute("value", node) : string.Empty;
        }

        private static bool GetBoolValueByName(string name)
        {
            bool result;
            bool.TryParse(GetValueByName(name), out result);

            return result;
        }

        public static IDictionary<string, string> GetSites()
        {
            return Factory.GetConfigNodes("sitemapVariables/sites/site").Cast<XmlNode>()
                .Where(x => !string.IsNullOrEmpty(XmlUtil.GetAttribute("name", x)) &&
                            !string.IsNullOrEmpty(XmlUtil.GetAttribute("filename", x)))
                .ToDictionary(x => XmlUtil.GetAttribute("name", x), x => XmlUtil.GetAttribute("filename", x));
        }

        public static string GetServerUrlBySite(string name)
        {
            var node = Factory.GetConfigNodes("sitemapVariables/sites/site")
                .Cast<XmlNode>()
                .FirstOrDefault(x => XmlUtil.GetAttribute("name", x) == name);

            return node != null ? XmlUtil.GetAttribute("serverUrl", node) : string.Empty;
        }
    }
}
