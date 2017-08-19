using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Gluon
{
    public class Utils
    {
        /// <summary>
        ///     Geting file body from resource
        /// </summary>
        /// <param name="fileName"> Name of file </param>
        /// <returns> File body as string </returns>
        public static string GetFileBody(string fileName)
        {
            string fileBody = null;
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream s = assembly.GetManifestResourceStream(String.Format("{0}.{1}", 
                assembly.GetName().Name, fileName)))
            {
                if (s != null)
                {
                    using (StreamReader sr = new StreamReader(s))
                    {
                        fileBody = sr.ReadToEnd();
                    }
                }
            }
            return fileBody;
        }
    }

    public static class StringExtensions
    {
        public static string ToCapital(this string value)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
        }
    }

    /// <summary>
    ///     Class for config's objects deserialization
    ///     see:
    ///     https://sites.google.com/site/craigandera/craigs-stuff/clr-workings/the-last-configuration-section-handler-i-ll-ever-need
    /// </summary>
    public class XmlSerializerSectionHandler : IConfigurationSectionHandler
    {
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public object Create(object parent, object configContext, XmlNode section)
        {
            var nav = section.CreateNavigator();
            var typename = (string)nav.Evaluate("string(@type)");
            var t = Type.GetType(typename);
            var ser = new XmlSerializer(t);
            var sectionObject = ser.Deserialize(new XmlNodeReader(section));
            return sectionObject;
        }
    }

    /// <summary>
    ///     Сonfig section for working with TFS
    /// </summary>
    public class TfsSettings
    {
        /// <summary>
        ///     TFS uri
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        ///     TFS project path
        /// </summary>
        public string ProjectPath { get; set; }
    }

    /// <summary>
    ///     Config section for common settings
    /// </summary>
    public class CommonSettings
    {
        /// <summary>
        ///     Namespace that will be add to target project
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        ///     Target project name where will be created folder with generated files
        /// </summary>
        public string TargetProject { get; set; }

        /// <summary>
        ///     Target project namespace
        /// </summary>
        public string TargetProjectNameSpace { get; set; }

        /// <summary>
        ///     Common return name of stored procedures (Can be different for different providers)
        /// </summary>
        public string StoredProcReturnName { get; set; }

        /// <summary>
        ///     Space count for one indent level
        /// </summary>
        public int IndentValue { get; set; }

        /// <summary>
        ///     Need to adding of the folder and its files in a source control
        /// </summary>
        public bool UsingTfs { get; set; }
    }


}
