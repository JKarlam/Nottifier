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
            { "alertWindowPositionX", Screen.PrimaryScreen.WorkingArea.Width - 400 },
            { "alertWindowPositionY", Screen.PrimaryScreen.WorkingArea.Height - 200 }
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
                Debug.WriteLine("Éxito al crear el archivo de configuración");
                return true;
            }
            catch (IOException e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine("Fallo al crear el archivo de configuración");
                return false;
            }
            finally
            {
                if (fs != null) fs.Close();
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
                Debug.WriteLine(e.Message);
                return false;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }

        public static void SetFadeTime(int ms)
        {
            dic["timeForAlertToFade"] = ms;
            WriteConfigFile();
        }
    }
}
