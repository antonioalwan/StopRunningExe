using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml;
using System.Web.Script.Serialization;
using System.IO;
using System.Xml.Serialization;

namespace StopRunningExe
{
    class Program
    {
        static void Main(string[] args)
        {
            Process[] processes = Process.GetProcesses();
            ConfigReader config =new ConfigReader("config.xml");
            Settings settings = config.loadConfigDocument();
            settings.Setting.ForEach(x =>
            {
                if (x.LookFor == "Title")
                {
                    var procs = processes.Where( p => p.MainWindowTitle.ToLower().Contains(x.Title.ToLower()));
                    if (procs != null)
                    {
                        //procs.ToList().ForEach(pr => pr.Kill());
                        foreach (Process p in procs)
                        {
                            p.Kill();
                        }
                    }
                }
            });
        }
    }
    class ConfigReader
    {
        private readonly String path;

        public ConfigReader(String path)
        {
            this.path = path;
        }
        public Settings loadConfigDocument()
        {
            XmlDocument doc = null;
            try
            {
                doc = new XmlDocument();
                doc.Load(path);
                using (var stringWriter = new StringWriter())
                {
                    using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                    {
                        doc.WriteTo(xmlTextWriter);
                        xmlTextWriter.Flush();
                        String s = stringWriter.GetStringBuilder().ToString();
                        Settings settings = ParseHelpers.ParseXML<Settings>(s);
                        return settings;
                    }
                }
            }
            catch (System.IO.FileNotFoundException e)
            {
                throw new Exception("No configuration file found.", e);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
    internal static class ParseHelpers
    {
        private static JavaScriptSerializer json;
        private static JavaScriptSerializer JSON
        {
            get
            {
                return json ?? (json = new JavaScriptSerializer());
            }
        }

        public static Stream ToStream(this string @this)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(@this);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


        public static T ParseXML<T>(this string @this) where T : class
        {
            var reader = XmlReader.Create(@this.Trim().ToStream(),
                new XmlReaderSettings()
                {
                    ConformanceLevel = ConformanceLevel.Auto,
                }
            );
            return new XmlSerializer(typeof(T)).Deserialize(reader) as T;
        }

        public static T ParseJSON<T>(this string @this) where T : class
        {
            return JSON.Deserialize<T>(@this.Trim());
        }
    }
    [Serializable()]
    public class Settings
    {
        public Settings()
        {

        }
        [XmlElement("Setting")]
        public List<OneSetting> Setting;
    }
    [Serializable()]
    public class OneSetting
    {
        public OneSetting()
        {
        }
        [XmlAttribute]
        public String LookFor;

        public String Name { get; set; }
        public String Command { get; set; }
        public String Title { get; set; }
    }
}
