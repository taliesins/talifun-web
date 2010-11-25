using System;
using System.Globalization;
using System.Web.Configuration;
using System.Configuration;
using System.Web;

namespace Talifun.Web.Configuration
{
    /// <summary>
    /// Provides methods to access strongly-typed configuration elements from the application configuration.
    /// </summary>
    public class CurrentConfigurationManager
    {
        protected static volatile System.Configuration.Configuration Config = null;
        /// <summary>
        /// Finds the first configuration section matching any of the supplied <paramref name="sectionNames"/>,
        /// attempt to cast it to the generic type parameter <typeparamref name="T"/> and return the result
        /// if successful. If no matching configuration section is found, raise an exception.
        /// </summary>
        /// <typeparam name="T">A type derived from <see cref="System.Configuration.ConfigurationSection" /> </typeparam>
        /// <param name="sectionNames">A list of strings identifying the names of configuration sections to be searched.</param>
        /// <returns>The first matching configuration section as an instance of <typeparamref name="T"/></returns>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">Thrown when no matching configuration section can be found in the default configuration file.</exception>
        public static T GetSection<T>(params string[] sectionNames) where T : ConfigurationSection
        {
            // Here we're using WebConfigurationManager instead of ConfigurationManager
            // because we're using the same framework for both Web and client configuration.
            T result;
            foreach (var sectionName in sectionNames)
            {
                if ((result = WebConfigurationManager.GetSection(sectionName) as T) != null) return (result);
            }
            var currentConfiguration = GetCurrentConfiguration();

            foreach (ConfigurationSection section in currentConfiguration.Sections)
            {
                if ((result = section as T) != null) return (result);
            }
            // If we've got this far, it means we didn't find any matching sections in the configuration file. so we raise an exception:
            throw new ConfigurationErrorsException("Unable to find configuration section " + typeof(T).FullName);
        }

        /// <summary>
        /// Finds the first configuration section group matching the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">A type derived from <see cref="System.Configuration.ConfigurationSectionGroup" /></typeparam>
        /// <returns>The first matching configuration section as an instance of <typeparamref name="T"/></returns>
        public static T GetSectionGroup<T>() where T : ConfigurationSectionGroup
        {
            var currentConfiguration = GetCurrentConfiguration();
            foreach (ConfigurationSectionGroup group in currentConfiguration.SectionGroups)
            {
                var result = group as T;
                if (result != null) return (result);
            }
            // If we've got this far, it means we didn't find any matching sections in the 
            // configuration file. so we raise an exception:
            throw new ConfigurationErrorsException("Unable to find configuration section group " + typeof(T).FullName);
        }

        /// <summary>
        /// Gets a reference to the currently active application configuration object.
        /// </summary>
        private static System.Configuration.Configuration GetCurrentConfiguration()
        {
            // First - if we've previously cached the current configuration, return it
            if (Config != null) return (Config);

            // Next - if we're running in a web context, cache & return the current web configuration
            if (HttpContext.Current != null)
            {
                var rootPath = HttpContext.Current.Request.ApplicationPath;
                var currentPath = VirtualPathUtility.GetDirectory(HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath);

                while (currentPath.Length >= rootPath.Length)
                {
                    Config = WebConfigurationManager.OpenWebConfiguration(currentPath);

                    if (Config.HasFile)
                    {
                        return (Config);
                    }
                    else
                    {
                        currentPath = currentPath.Substring(0, currentPath.LastIndexOf('/'));
                    }
                }

                Config = WebConfigurationManager.OpenWebConfiguration("~");
                return (Config);
            }

            var configFileName = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            configFileName = configFileName.Replace(".config", "").Replace(".temp", "");
            // check for design mode
            if (configFileName.ToLower(CultureInfo.InvariantCulture).Contains("devenv.exe"))
            {
                Config = GetDesignTimeConfiguration();
            }
            else
            {
                Config = GetRuntimeTimeConfiguration();
            }

            return Config;
        }

        /// <summary>
        /// Returns the active configuration when the project is running standalone.
        /// </summary>
        /// <returns>The active configuration when the project is running standalone.</returns>
        private static System.Configuration.Configuration GetRuntimeTimeConfiguration()
        {
            var configMap = new ExeConfigurationFileMap();

            var configFileName = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            configMap.ExeConfigFilename = configFileName;

            return (ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None));
        }

        /// <summary>
        /// Returns the active configuration when the project is running within the Visual Studio design environment.
        /// </summary>
        /// <returns>The active configuration when the project is running within the Visual Studio design environment.</returns>
        private static System.Configuration.Configuration GetDesignTimeConfiguration()
        {
            ExeConfigurationFileMap configMap = null;
            // Get an instance of the currently running Visual Studio IDE (dte = "design time environment")
            var dte = (EnvDTE80.DTE2)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.8.0");
            if (dte != null)
            {
                dte.SuppressUI = true;
                var item = dte.Solution.FindProjectItem("web.config");
                if (item != null)
                {
                    configMap = new ExeConfigurationFileMap();
                    if (item.ContainingProject.FullName.ToLower(CultureInfo.InvariantCulture).StartsWith("http:"))
                    {
                        configMap.ExeConfigFilename = item.get_FileNames(0);
                    }
                    else
                    {
                        var info = new System.IO.FileInfo(item.ContainingProject.FullName);
                        configMap.ExeConfigFilename = String.Format(CultureInfo.InvariantCulture, "{0}\\{1}", info.Directory.FullName, item.Name);
                    }
                }
            }
            return (ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None));
        }
    }
}
