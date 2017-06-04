using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using System.Diagnostics;

namespace Nottifier
{
    static class DBHelper
    {
        private static string dbName = "filters.sqlite";

        private static SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbName + ";Version=3;");

        private static string sql_CreateTable_nameoptions = "CREATE TABLE nameoptions ( " +
                                                                "name VARCHAR(50) UNIQUE NOT NULL, " +
                                                                "bgcolor CHARACTER(9) DEFAULT '#ffffffff', " +
                                                                "textcolor CHARACTER(9) DEFAULT '#ff000000'" +
                                                            ")";
        private static string sql_RetrieveAllData_nameoptions = "SELECT * FROM nameoptions";
        private static string sql_AddName_nameoptions = "INSERT INTO nameoptions (name) VALUES ( '{0}' )";
        private static string sql_ChangeName_nameoptions = "UPDATE nameoptions SET name = '{1}' WHERE name LIKE '{0}'";
        private static string sql_RemoveName_nameoptions = "DELETE FROM nameoptions WHERE name LIKE '{0}'";
        private static string sql_RetrieveNameColors_nameoptions = "SELECT bgcolor, textcolor FROM nameoptions WHERE name LIKE '{0}'";
        private static string sql_UpdateBackgroundColor_nameoptions = "UPDATE nameoptions SET bgcolor = '{0}' WHERE name LIKE '{1}'";
        private static string sql_UpdateTextColor_nameoptions = "UPDATE nameoptions SET textcolor = '{0}' WHERE name LIKE '{1}'";
        private static string sql_RemoveAll_nameoptions = "DELETE FROM nameoptions";

        private static string sql_CreateTable_filters = "CREATE TABLE filters (" +
                                                            "name VARCHAR(50) NOT NULL, " +
                                                            "text VARCHAR(50) UNIQUE NOT NULL," +
                                                            "FOREIGN KEY(name) REFERENCES nameoptions(name)" +
                                                        ")";
        private static string sql_RetrieveFilters_filters = "SELECT text FROM filters WHERE name LIKE '{0}'";
        private static string sql_AddFilter_filters = "INSERT INTO filters VALUES ( '{0}', '{1}' )";
        private static string sql_RemoveFilter_filters = "DELETE FROM filters WHERE name LIKE '{0}' AND text LIKE '{1}'";
        private static string sql_RemoveAll_filters = "DELETE FROM filters";

        public static void OpenConnection(bool forceDBCreation = false)
        {
            if (forceDBCreation)
            {
                if (File.Exists(dbName))
                    File.Delete(dbName);
                CreateNewDatabase();
            }
            else
            {
                if (!File.Exists(dbName))
                    CreateNewDatabase();
                else
                    connection.Open();
            }
        }

        private static void CreateNewDatabase()
        {
            Debug.WriteLine("-- Creando base de datos");
            SQLiteConnection.CreateFile(dbName);
            connection.Open();

            Debug.WriteLine("Creando tabla nameoptions");
            SQLiteCommand nameOptions = new SQLiteCommand(sql_CreateTable_nameoptions, connection);
            nameOptions.ExecuteNonQuery();

            Debug.WriteLine("Creando tabla filters");
            SQLiteCommand filters = new SQLiteCommand(sql_CreateTable_filters, connection);
            filters.ExecuteNonQuery();
        }

        public static List<string[]> GenerateNamesDataList()
        {
            List<string[]> l = new List<string[]>();

            SQLiteCommand c = new SQLiteCommand(sql_RetrieveAllData_nameoptions, connection);
            SQLiteDataReader r = c.ExecuteReader();
            while (r.Read())
            {
                string[] s = { r[0].ToString(), r[1].ToString(), r[2].ToString() };
                l.Add(s);
            }
            return l;
        }

        public static List<string> GenerateNamesList()
        {
            List<string> l = new List<string>();

            SQLiteCommand c = new SQLiteCommand(sql_RetrieveAllData_nameoptions, connection);
            SQLiteDataReader r = c.ExecuteReader();
            while (r.Read())
            {
                l.Add((string)r[0]);
            }
            return l;
        }

        public static string[] GetColors(string name)
        {
            SQLiteCommand c = new SQLiteCommand(String.Format(sql_RetrieveNameColors_nameoptions, name), connection);
            SQLiteDataReader r = c.ExecuteReader();
            if(r.Read())
                return new string[] { r[0].ToString(), r[1].ToString() };
            else return new string[] { "#ffffffff", "#ff000000" };
        }

        public static void AddName(string name)
        {
            SQLiteCommand c = new SQLiteCommand(String.Format(sql_AddName_nameoptions, name), connection);
            c.ExecuteNonQuery();
        }

        public static void ChangeName(string oldName, string newName)
        {
            SQLiteCommand c = new SQLiteCommand(String.Format(sql_ChangeName_nameoptions, oldName, newName), connection);
            c.ExecuteNonQuery();
        }

        public static void RemoveName(string name)
        {
            SQLiteCommand c = new SQLiteCommand(String.Format(sql_RemoveName_nameoptions, name), connection);
            c.ExecuteNonQuery();
        }

        public static bool UpdateBackgroundColor(string name, string color)
        {
            try
            {
                SQLiteCommand c = new SQLiteCommand(String.Format(sql_UpdateBackgroundColor_nameoptions, color, name), connection);
                c.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        public static bool UpdateTextColor(string name, string color)
        {
            try
            {
                SQLiteCommand c = new SQLiteCommand(String.Format(sql_UpdateTextColor_nameoptions, color, name), connection);
                c.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        public static List<string> GenerateFilterList(string name)
        {
            List<string> l = new List<string>();

            SQLiteCommand c = new SQLiteCommand(String.Format(sql_RetrieveFilters_filters, name), connection);
            SQLiteDataReader r = c.ExecuteReader();
            while (r.Read())
            {
                l.Add((string)r[0]);
            }
            Debug.WriteLine("Número de filtros para " + name + ": " + l.Count);
            return l;
        }

        public static void AddFilter(string name, string text)
        {
            SQLiteCommand c = new SQLiteCommand(String.Format(sql_AddFilter_filters, name, text), connection);
            int i = c.ExecuteNonQuery();
            Debug.WriteLine("Filtro añadido: " + text + " de " + name + " con resultado: " + i);
        }

        public static void RemoveFilter(string name, string text)
        {
            SQLiteCommand c = new SQLiteCommand(String.Format(sql_RemoveFilter_filters, name, text), connection);
            int i = c.ExecuteNonQuery();
            Debug.WriteLine("Filtro eliminado: " + text + " de " + name + " con resultado: " + i);
        }

        public static void RemoveAll()
        {
            SQLiteCommand c1 = new SQLiteCommand(sql_RemoveAll_nameoptions, connection);
            c1.ExecuteNonQuery();
            Debug.WriteLine("Tabla nameoptions vaciada");

            SQLiteCommand c2 = new SQLiteCommand(sql_RemoveAll_filters, connection);
            c2.ExecuteNonQuery();
            Debug.WriteLine("Tabla filters vaciada");

            // VACUUM es usado para limpiar el espacio liberado
            SQLiteCommand c3 = new SQLiteCommand("VACUUM", connection);
            c3.ExecuteNonQuery();
        }
    }
}
