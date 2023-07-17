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

using System;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using Upendo.Modules.PageSettingsEditor.Models;

namespace Upendo.Modules.PageSettingsEditor.Controllers
{
    /// <summary>
    /// PageSettingsEditorController class
    /// </summary>
    [DnnHandleError]
    public class PageSettingsEditorController : DnnController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PageSettingsEditorController));

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

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActionResult Delete(string key)
        {
            try
            {
                TabController.Instance.DeleteTabSetting(this.ActivePage.TabID, key);
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }

            return RedirectToDefaultRoute();
        }

        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            try
            {
                var model = new PageSettingsInfo {PageSettings = SortedPageSettings, NewSetting = new SettingNvp()};

                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        /// <summary>
        /// Index
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [DotNetNuke.Web.Mvc.Framework.ActionFilters.ValidateAntiForgeryToken]
        public ActionResult Index(PageSettingsInfo model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // the first two checks in the IF statement below are necessary for when the module is first added to a page
                    if (model != null && model.NewSetting != null && !string.IsNullOrEmpty(model.NewSetting.Key) &&
                        !string.IsNullOrEmpty(model.NewSetting.Value))
                    {
                        var newKey = WebUtility.HtmlEncode(model.NewSetting.Key.Trim());
                        var newValue = WebUtility.HtmlEncode(model.NewSetting.Value.Trim());

                        TabController.Instance.UpdateTabSetting(ActivePage.TabID, newKey, newValue);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }

            return RedirectToDefaultRoute();
        }

        /// <summary>
        /// Edit
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActionResult Edit(string key)
        {
            var settings = SortedPageSettings;
            var model = new SettingNvp();

            try
            {
                foreach (var setting in settings)
                {
                    if (setting.Key == key)
                    {
                        model.Key = setting.Key;
                        model.Value = setting.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }

            return View(model);
        }

        /// <summary>
        /// Edit
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [DotNetNuke.Web.Mvc.Framework.ActionFilters.ValidateAntiForgeryToken]
        public ActionResult Edit(SettingNvp model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // TODO: Add a check to prevent a duplicate key
                    if (!string.IsNullOrEmpty(model.Key))
                    {
                        var newKey = WebUtility.HtmlEncode(model.Key.Trim());
                        var newValue = WebUtility.HtmlEncode(model.Value.Trim());

                        TabController.Instance.UpdateTabSetting(ActivePage.TabID, newKey, newValue);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }

            return RedirectToDefaultRoute();
        }

        private void LogError(Exception ex)
        {
            if (ex != null)
            {
                Logger.Error(ex.Message, ex);
                try
                {
                    Exceptions.LogException(ex);
                }
                catch 
                {
                    // do nothing
                }

                if (ex.InnerException != null)
                {
                    LogError(ex.InnerException);
                }
            }
        }
    }
}
