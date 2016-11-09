/* *********************************************************************** *
 * File   : SitemapManagerForm.cs                         Part of Sitecore *
 * Version: 1.0.0                                         www.sitecore.net *
 *                                                                         *
 *                                                                         *
 * Purpose: Codebehind of ManagerForm                                      *
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

using System;
using System.Text;
using Sitecore.Diagnostics;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.Modules.SitemapXML
{
    public class SitemapManagerForm : BaseForm
    {
        protected Literal Message;
        protected Button RefreshButton;

        protected override void OnLoad(EventArgs args)
        {
            base.OnLoad(args);
            if (!Context.ClientPage.IsEvent)
                RefreshButton.Click = "RefreshButtonClick";
        }

        protected void RefreshButtonClick()
        {
            var sh = new SitemapHandler();
            sh.RefreshSitemap(this, new EventArgs());

            var sites = SitemapManagerConfiguration.GetSites();
            var sb = new StringBuilder();
            foreach (string sitemapFile in sites.Values)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(sitemapFile);
            }

            Message.Text = $" - The sitemap file <b>\"{sb}\"</b> has been refreshed<br /> - <b>\"{sb}\"</b> has been registered to \"robots.txt\"";

            RefreshPanel("MainPanel");
        }

        private static void RefreshPanel(string panelName)
        {
            var ctl = Context.ClientPage.FindControl(panelName) as
                Panel;
            Assert.IsNotNull(ctl, "can't find panel");

            Context.ClientPage.ClientResponse.Refresh(ctl);
        }
    }
}