using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Nottifier
{
    static class ConfigManager
    {
        private static string configFileName = "config.ini";

        public static Dictionary<string, int> dic = new Dictionary<string, int>()
        {
            { "timeForAlertToFade", 6000 },
            { "alertWindowPositionX", Screen.PrimaryScreen.Bounds.Width - 400 },
            { "alertWindowPositionY", Screen.PrimaryScreen.Bounds.Height - 200 }
        };

        public static void Start()
        {
            if (File.Exists(configFileName)) {
                if (new FileInfo(configFileName).Length != 0)
                    LoadConfigFromFile();
                else WriteConfigFile();
            } else WriteConfigFile();
        }

        //TODO: no escribir valores repetidos, borrar el anterior y reescribir
        public static bool WriteConfigFile()
        {
            Debug.WriteLine("Creando archivo de configuración");
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                fs = new FileStream(configFileName, FileMode.OpenOrCreate);
                sw = new StreamWriter(fs);
                foreach (var key in dic.Keys)
                {
                    sw.WriteLine(key + " = " + dic[key]);
                    Debug.WriteLine(key + " = " + dic[key]);
                }
                sw.Close();
                fs.Close();
                Debug.WriteLine("Éxito al crear el archivo de configuración");
                return true;
            }
            catch (IOException e)
            {
                if (sw != null) sw.Close();
                if (fs != null) fs.Close();
                Debug.WriteLine(e.Message);
                Debug.WriteLine("Fallo al crear el archivo de configuración");
                return false;
            }
        }
        
        private static bool LoadConfigFromFile()
        {
            FileStream fs = null;
            StreamReader sr = null;
            try
            {
                fs = new FileStream(configFileName, FileMode.Open);
                sr = new StreamReader(fs);
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] splitLine = line.Split('=');
                    splitLine[0] = splitLine[0].Trim();
                    splitLine[1] = splitLine[1].Trim();
                    Debug.WriteLine(splitLine[0]+" = "+splitLine[1]);
                    dic[splitLine[0]] = int.Parse(splitLine[1]);
                }
                sr.Close();
                fs.Close();
                return true;
            }
            catch (IOException e)
            {
                if(sr != null) sr.Close();
                if (fs != null) fs.Close();
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        public static void SetFadeTime(int ms)
        {
            dic["timeForAlertToFade"] = ms;
            WriteConfigFile();
        }
    }
}
