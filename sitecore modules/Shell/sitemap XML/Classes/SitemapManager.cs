﻿/* *********************************************************************** *
 * File   : SitemapManager.cs                             Part of Sitecore *
 * Version: 1.0.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Manager class what contains all main logic                     *
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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Modules.SitemapXML.Filters;
using Sitecore.Security.Accounts;
using Sitecore.Sites;
using Sitecore.Web;

namespace Sitecore.Modules.SitemapXML
{
    public class SitemapManager
    {
        private static IDictionary<string, string> _sites;

        public SitemapManager()
        {
            Database = Factory.GetDatabase(SitemapManagerConfiguration.WorkingDatabase);
            Assert.IsNotNull(Database, $"Cannot load database for {SitemapManagerConfiguration.WorkingDatabase}");

            ConfigItem = Database.GetItem(SitemapManagerConfiguration.SitemapConfigurationItemPath);
            Assert.IsNotNull(ConfigItem,
                $"Can not find sitemap config item on path {SitemapManagerConfiguration.SitemapConfigurationItemPath}.");

            _sites = SitemapManagerConfiguration.GetSites();
            foreach (var site in _sites)
                BuildSiteMap(site.Key, site.Value);
        }

        private Database Database { get; }
        private Item ConfigItem { get; }

        private void BuildSiteMap(string sitename, string sitemapUrlNew)
        {
            var site = SiteManager.GetSite(sitename);
            var siteContext = Factory.GetSite(sitename);
            var items = GetSitemapItems(siteContext.StartPath);
            var fullPath = MainUtil.MapPath(string.Concat("/", sitemapUrlNew));
            var xmlContent = BuildSitemapXml(items, site);

            using (var writer = new XmlTextWriter(fullPath, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;

                xmlContent.WriteContentTo(writer);
                writer.Flush();
            }
        }


        public void SubmitSitemapToSearchenginesByHttp()
        {
            if (!SitemapManagerConfiguration.IsProductionEnvironment)
                return;

            var engines = ConfigItem.ToIds("Search engines");
            foreach (var id in engines)
            {
                var engine = Database.GetItem(id);
                if (engine != null)
                {
                    var engineHttpRequestString = engine.Fields["HttpRequestString"].Value;
                    foreach (var sitemapUrl in _sites.Values)
                        SubmitEngine(engineHttpRequestString, sitemapUrl);
                }
            }
        }

        public void RegisterSitemapToRobotsFile()
        {
            if (!SitemapManagerConfiguration.GenerateRobotsFile)
                return;

            var robotsPath = MainUtil.MapPath(string.Concat("/", "robots.txt"));
            var sitemapContent = new StringBuilder(string.Empty);
            if (File.Exists(robotsPath))
            {
                var sr = new StreamReader(robotsPath);
                sitemapContent.Append(sr.ReadToEnd());
                sr.Close();
            }

            var sw = new StreamWriter(robotsPath, false);
            foreach (var sitemapUrl in _sites.Values)
            {
                var sitemapLine = string.Concat("Sitemap: ", sitemapUrl);
                if (!sitemapContent.ToString().Contains(sitemapLine))
                    sitemapContent.AppendLine(sitemapLine);
            }
            sw.Write(sitemapContent.ToString());
            sw.Close();
        }

        private static XmlDocument BuildSitemapXml(IEnumerable<Item> items, Site site)
        {
            var doc = new XmlDocument();
            XmlNode declarationNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(declarationNode);
            XmlNode urlsetNode = doc.CreateElement("urlset");
            var xmlnsAttr = doc.CreateAttribute("xmlns");
            xmlnsAttr.Value = SitemapManagerConfiguration.XmlnsTpl;
            urlsetNode.Attributes.Append(xmlnsAttr);

            doc.AppendChild(urlsetNode);

            foreach (var itm in items)
                doc = BuildSitemapItem(doc, itm, site);

            return doc;
        }

        private static XmlDocument BuildSitemapItem(XmlDocument doc, Item item, Site site)
        {
            var urlsetNode = doc.LastChild;
            var urlNode = doc.CreateElement("url");
            urlsetNode.AppendChild(urlNode);

            var locNode = doc.CreateElement("loc");
            urlNode.AppendChild(locNode);
            var url = HtmlEncode(GetItemUrl(item, site));
            locNode.AppendChild(doc.CreateTextNode(url));

            var lastmodNode = doc.CreateElement("lastmod");
            urlNode.AppendChild(lastmodNode);
            var lastMod = HtmlEncode(item.Statistics.Updated.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            lastmodNode.AppendChild(doc.CreateTextNode(lastMod));

            return doc;
        }

        private static string GetItemUrl(Item item, Site site)
        {
            var options = UrlOptions.DefaultOptions;
            options.SiteResolving = Settings.Rendering.SiteResolving;
            options.Site = SiteContext.GetSite(site.Name);
            options.AlwaysIncludeServerUrl = false;

            var url = LinkManager.GetItemUrl(item, options);

            var serverUrl = SitemapManagerConfiguration.GetServerUrlBySite(site.Name);
            if (serverUrl.Contains("http://"))
                serverUrl = serverUrl.Substring("http://".Length);

            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(serverUrl))
            {
                if (url.Contains("://") && !url.Contains("http"))
                {
                    sb.Append("http://");
                    sb.Append(serverUrl);
                    if (url.IndexOf("/", 3) > 0)
                        sb.Append(url.Substring(url.IndexOf("/", 3)));
                }
                else
                {
                    sb.Append("http://");
                    sb.Append(serverUrl);
                    sb.Append(url);
                }
            }
            else if (!string.IsNullOrEmpty(site.Properties["hostname"]))
            {
                sb.Append("http://");
                sb.Append(site.Properties["hostname"]);
                sb.Append(url);
            }
            else
            {
                if (url.Contains("://") && !url.Contains("http"))
                {
                    sb.Append("http://");
                    sb.Append(url);
                }
                else
                {
                    sb.Append(WebUtil.GetFullUrl(url));
                }
            }

            return sb.ToString();
        }

        private static string HtmlEncode(string text)
        {
            var result = HttpUtility.HtmlEncode(text);

            return result;
        }

        private void SubmitEngine(string engine, string sitemapUrl)
        {
            //Check if it is not localhost because search engines returns an error
            if (!sitemapUrl.Contains("http://localhost"))
            {
                var request = string.Concat(engine, HtmlEncode(sitemapUrl));

                var httpRequest = (HttpWebRequest)WebRequest.Create(request);
                try
                {
                    var webResponse = httpRequest.GetResponse();

                    var httpResponse = (HttpWebResponse)webResponse;
                    if (httpResponse.StatusCode != HttpStatusCode.OK)
                        Log.Error($"Cannot submit sitemap to \"{engine}\"", this);
                }
                catch
                {
                    Log.Warn($"The search engine \"{request}\" returns an 404 error", this);
                }
            }
        }


        private IEnumerable<Item> GetSitemapItems(string rootPath)
        {
            var contentRoot = Database.Items[rootPath];
            Item[] descendants;
            var user = User.FromName(@"extranet\Anonymous", true);
            using (new UserSwitcher(user))
            {
                descendants = contentRoot.Axes.GetDescendants();
            }

            var filters = FilterFactory.GetFilters();

            return new[] { contentRoot }
                .Concat(descendants)
                .Where(item => filters.All(x => x.IsValid(item)))
                .ToList();
        }
    }
}