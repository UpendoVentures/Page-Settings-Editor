/*
MIT License

Copyright (c) Upendo Ventures, LLC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security;
using Upendo.Modules.PageSettingsEditor.Models;

namespace Upendo.Modules.PageSettingsEditor.Controllers
{
    [DnnHandleError]
    public class PageSettingsEditorController : DnnController
    {
        private IEnumerable<SettingNvp> SortedPageSettings
        {
            get
            {
                var tabSettings = ActivePage.TabID == Null.NullInteger ? new Hashtable() : TabController.Instance.GetTabSettings(ActivePage.TabID);
                var sortedSettings = new List<SettingNvp>();
                foreach (DictionaryEntry setting in tabSettings)
                {
                    sortedSettings.Add(new SettingNvp(setting.Key.ToString(), setting.Value.ToString()));
                }

                return sortedSettings.OrderBy(s => s.Key);
            }
        }

        public ActionResult Delete(string key)
        {
            TabController.Instance.DeleteTabSetting(this.ActivePage.TabID, key);
            return RedirectToDefaultRoute();
        }

        public ActionResult Index()
        {
            DotNetNuke.Framework.JavaScriptLibraries.JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            var model = new PageSettingsInfo { PageSettings = SortedPageSettings, NewSetting = new SettingNvp() };

            return View(model);
        }

        [HttpPost]
        [DotNetNuke.Web.Mvc.Framework.ActionFilters.ValidateAntiForgeryToken]
        public ActionResult Index(PageSettingsInfo model)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(model.NewSetting.Key) && !string.IsNullOrEmpty(model.NewSetting.Value))
                {
                    var newKey =
                        PortalSecurity.Instance.InputFilter(model.NewSetting.Key.Trim(), PortalSecurity.FilterFlag.NoMarkup);
                    var newValue =
                        PortalSecurity.Instance.InputFilter(model.NewSetting.Value.Trim(), PortalSecurity.FilterFlag.NoMarkup);

                    TabController.Instance.UpdateTabSetting(ActivePage.TabID, newKey, newValue);
                }
            }

            return RedirectToDefaultRoute();
        }

        public ActionResult Edit(string key)
        {
            var settings = SortedPageSettings;
            var model = new SettingNvp();
            foreach (var setting in settings)
            {
                if (setting.Key == key)
                {
                    model.Key = setting.Key;
                    model.Value = setting.Value;
                }
            }

            return View(model);
        }

        [HttpPost]
        [DotNetNuke.Web.Mvc.Framework.ActionFilters.ValidateAntiForgeryToken]
        public ActionResult Edit(SettingNvp model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Add a check to prevent a duplicate key
                if (!string.IsNullOrEmpty(model.Key))
                {
                    var newKey =
                        PortalSecurity.Instance.InputFilter(model.Key.Trim(), PortalSecurity.FilterFlag.NoMarkup);
                    var newValue =
                        PortalSecurity.Instance.InputFilter(model.Value.Trim(), PortalSecurity.FilterFlag.NoMarkup);

                    TabController.Instance.UpdateTabSetting(ActivePage.TabID, newKey, newValue);
                }
            }

            return RedirectToDefaultRoute();
        }
    }
}
