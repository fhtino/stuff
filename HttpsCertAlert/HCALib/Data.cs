using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HCALib
{
    public class Data
    {

        private string _fileName;
        private object _saveFileLock = new object();

        [XmlAttribute]
        public int CheckIntervalHours { get; set; }

        [XmlAttribute]
        public DateTime DT { get; set; }

        [XmlAttribute]
        public bool ContinuousCheck { get; set; }

        [XmlAttribute]
        public string ProxySetup_TODO { get; set; }


        public List<HttpsCheck> CheckList = new List<HttpsCheck>();




        // ----------------------------------------------------------------

        public static Data LoadFromFile(string fileName)
        {
            Data instace;

            if (!File.Exists(fileName))
            {
                instace = new Data() { CheckList = new List<HttpsCheck>() };
            }
            else
            {
                instace = DeserializeFromFile<Data>(fileName);
            }

            instace._fileName = fileName;

            return instace;
        }


        public void Save()
        {
            lock (_saveFileLock)
            {
                DT = DateTime.UtcNow;
                SerializeToFile(this, _fileName);
            }
        }


        private static void SerializeToFile(object obj, string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Copy(fileName, fileName + ".bak", true);
            }

            XmlSerializer xs = new XmlSerializer(obj.GetType());
            XmlSerializerNamespaces xsn = new XmlSerializerNamespaces();
            xsn.Add(string.Empty, string.Empty);
            using (var fs = File.Create(fileName))
            {
                xs.Serialize(fs, obj, xsn);
            }
        }


        private static T DeserializeFromFile<T>(string fileName)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (var fs = File.OpenRead(fileName))
            {
                var obj = xs.Deserialize(fs);
                return (T)obj;
            }
        }

    }

}
