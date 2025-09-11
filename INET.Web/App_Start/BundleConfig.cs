using System.Configuration;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Optimization;

namespace INET.Web
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/scripts/lib/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/scripts/lib/jquery-ui-{version}.custom.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/scripts/lib/jquery.unobtrusive*",
                        "~/scripts/lib/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/globalize").Include(
                        "~/scripts/lib/globalize.js",
                        "~/scripts/lib/cultures/globalize.culture.es-AR.js",
                        "~/scripts/js/init-globalize.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/scripts/lib/modernizr-*"));

            bundles.Add(new StyleBundle("~/content/css").Include("~/content/site.css"));

            bundles.Add(new StyleBundle("~/content/themes/base/css").Include(
                        "~/content/themes/base/jquery.ui.core.css",
                        "~/content/themes/base/jquery.ui.resizable.css",
                        "~/content/themes/base/jquery.ui.selectable.css",
                        "~/content/themes/base/jquery.ui.accordion.css",
                        "~/content/themes/base/jquery.ui.autocomplete.css",
                        "~/content/themes/base/jquery.ui.button.css",
                        "~/content/themes/base/jquery.ui.dialog.css",
                        "~/content/themes/base/jquery.ui.slider.css",
                        "~/content/themes/base/jquery.ui.tabs.css",
                        "~/content/themes/base/jquery.ui.datepicker.css",
                        "~/content/themes/base/jquery.ui.progressbar.css",
                        "~/content/themes/base/jquery.ui.theme.css"));
        }
    }

    public static class ApplicationInformation
    {
        /// <summary>
        /// Gets the executing assembly.
        /// </summary>
        /// <value>The executing assembly.</value>
        public static System.Reflection.Assembly ExecutingAssembly
        {
            get { return executingAssembly ?? (executingAssembly = System.Reflection.Assembly.GetExecutingAssembly()); }
        }
        private static System.Reflection.Assembly executingAssembly;

        /// <summary>
        /// Gets the executing assembly version.
        /// </summary>
        /// <value>The executing assembly version.</value>
        public static System.Version ExecutingAssemblyVersion
        {
            get { return executingAssemblyVersion ?? (executingAssemblyVersion = ExecutingAssembly.GetName().Version); }
        }
        private static System.Version executingAssemblyVersion;

        /// <summary>
        /// Gets the executing Copyright.
        /// </summary>
        /// <value>The executing assembly version.</value>
        public static string ExecutingAssamblyCopyright
        {
            get
            {
                object[] attribs = ExecutingAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
                if (attribs.Length > 0)
                {
                    return ((AssemblyCopyrightAttribute)attribs[0]).Copyright;
                }
                else
                {
                    return "n/d";
                }
            }
        }
        private static string executingAssamblyCopyright;

        /// <summary>
        /// Gets the compile date of the currently executing assembly.
        /// </summary>
        /// <value>The compile date.</value>
        public static System.DateTime CompileDate
        {
            get
            {
                if (!compileDate.HasValue)
                    compileDate = RetrieveLinkerTimestamp(ExecutingAssembly.Location);
                return compileDate ?? new System.DateTime();
            }
        }
        private static System.DateTime? compileDate;

        /// <summary>
        /// Retrieves the linker timestamp.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        /// <remarks>http://www.codinghorror.com/blog/2005/04/determining-build-date-the-hard-way.html</remarks>
        private static System.DateTime RetrieveLinkerTimestamp(string filePath)
        {
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            var b = new byte[2048];
            System.IO.FileStream s = null;
            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                    s.Close();
            }
            var dt = new System.DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(System.BitConverter.ToInt32(b, System.BitConverter.ToInt32(b, peHeaderOffset) + linkerTimestampOffset));
            return dt.AddHours(System.TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
        }

        public static int MaxRequestLength
        {
            get
            {
                int maxRequestLength = 0;
                HttpRuntimeSection section =
                ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
                if (section != null)
                    maxRequestLength = section.MaxRequestLength;
                return maxRequestLength;
            }
        }

        public static int MaxRequestLengthMB
        {
            get
            {
                return MaxRequestLength > 0 ? MaxRequestLength / 1024 : 0;
            }
        }
    }
}