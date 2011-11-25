using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Principal;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;//mssql fuer cuso


/** Quellenverzeichnis:
 *  walkFolders:    http://dotnet-snippets.de/dns/rekursiver-verzeichnislauf-SID462.aspx
 *  md5:            http://dotnet-snippets.de/dns/md5-hash-von-dateien-ermitteln-SID66.aspx
 * 
 */

namespace worker  {
    
    
    
    class Program    {
        static string myConnectionString = "SERVER=53.100.11.229;" +
                                                  "DATABASE=worker;" +
                                                  "UID=root;" +
                                                  "PASSWORD=cNtN.5db6!;";
        static string path = "Q:\\";
        static string logfilename = "logfile.txt";
        static int gc = 0;
        static long globalsize = 0;

        static void Main(string[] args)   {
            //Daten des Sqlserver eintragen, spater per Kommandozeilenparameter/Konfigurationsdatei
            Sqlcreds sc = new Sqlcreds();
            sc.setHost("53.100.11.229");
            sc.setDatabase("workerdb");
            sc.setTable("testtable");
            sc.setUsername("dbworker");
            sc.setPassword("");

            //FileWrite(logfilename, DateTime.Now+": Starte Dateiindexierung fuer "+path+"\n\n");
            //mysql_trunc_table(sc);        
            walkFoldersSql(path, sc);
            //mysql_create_table(sc);
            //mysql_drop_table(sc);
            //truncmssql();
            //parsecuso();
        }

        
        private static void walkFoldersSql(string DirectorySql, Sqlcreds sc)
        {
            walkFoldersSql(new DirectoryInfo(DirectorySql), sc);
        }

        private static void walkFoldersSql(DirectoryInfo disql, Sqlcreds sc)
        {
            int lc = 0;
            string sqlcommand = "";
            
            Console.WriteLine("walkFoldersSql: Versuche Verbindung ...");

            try
            {
                
                
                // Alle Verzeichnisse rekursiv durchlaufen
                foreach (DirectoryInfo subdir in disql.GetDirectories())
                {
                    walkFoldersSql(subdir, sc);
                }

                // Alle Dateien des Verzeichnisses durchlaufen
                foreach (FileInfo fi in disql.GetFiles())
                {
                    ++gc;
                    ++lc;
                    MySqlConnection connection = new MySqlConnection(sc.getMysqlConStr());
                    MySqlCommand command = connection.CreateCommand();
                    
                    String output = DateTime.Now + ": add Nr." + gc + " [" + hrs(fi.Length) + "/" + hrs(globalsize) + " ges]: " + fi.FullName;
                    Console.WriteLine(output);
                    FileAppend(logfilename, output+"\n");
                    
                    connection.Open();
                    command.CommandText = "INSERT INTO `"+sc.getDatabase()+"`.`"+sc.getTable()+"` (`name`, `path`, `loc`, `size`, `csum`, `dom`, `owner`, `group`, `stime`, `atime`, `ctime`, `mtime`, `dups`) VALUES ('" + fi.Name + "', '" + fi.FullName + "', '" + fi.FullName.Split('\\')[0] + "', '" + fi.Length + "', '" + Datei2SHA(fi.FullName)/*"--not computed--"*/ + "', '" + File.GetAccessControl(@fi.FullName).GetOwner(typeof(NTAccount)).ToString().Split('\\')[0] + "', '" + File.GetAccessControl(@fi.FullName).GetOwner(typeof(NTAccount)).ToString().Split('\\')[1] + "', '" + File.GetAccessControl(@fi.FullName).GetGroup(typeof(NTAccount)).ToString().Split('\\')[1] + "', '" + UnixTime(DateTime.Now) + "', '" + UnixTime(fi.LastAccessTime) + "', '" + UnixTime(fi.CreationTime) + "', '" + UnixTime(fi.LastWriteTime) + "', '1');";
                    MySqlDataReader Reader;
                    Reader = command.ExecuteReader();
                    globalsize += fi.Length;
                    connection.Close();
                
                }
                Console.WriteLine("walkFoldersSql: " + lc + " Zeilen hinzugefuegt.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        
        public static void FileWrite(String sFilename, String sLines)
        {
            StreamWriter myFile = new StreamWriter(sFilename);
            myFile.Write(sLines);
            myFile.Close();
        }
        
        public static void FileAppend(string sFilename, string sLines)
        {
            StreamWriter myFile = new StreamWriter(sFilename, true);
            myFile.Write(sLines);
            myFile.Close();
        }

        /**get unix timeformat
         */ 
        public static long UnixTime(DateTime filetime)
        {
            TimeSpan _TimeSpan = (filetime - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)_TimeSpan.TotalSeconds;
        }

        /**sha512
         */
        public static string Datei2SHA(string Dateipfad)
        {
            //Datei einlesen
            System.IO.FileStream FileCheck = System.IO.File.OpenRead(Dateipfad);
            // MD5-Hash aus dem Byte-Array berechnen
            //System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            System.Security.Cryptography.SHA512 sha512 = new System.Security.Cryptography.SHA512CryptoServiceProvider();
            byte[] sha512Hash = sha512.ComputeHash(FileCheck);
            FileCheck.Close();

            //in string wandeln
            string Berechnet = BitConverter.ToString(sha512Hash).Replace("-", "").ToLower();
            return Berechnet;

        }

        //humanreadable filesize
        public static string hrs(long filebytes)  {
            string hrsize = "";
            int n = 0, nachkomma = 0;

            /** Annahme: viel mehr kleine als große Dateien */
            while (filebytes > 1024)
            {
                nachkomma = (int)(filebytes % 1024);
                filebytes /= 1024;
                n++;
            }
            hrsize = filebytes.ToString();
            if (nachkomma > 100)
            {
                nachkomma = nachkomma / 100;
                hrsize += "," + nachkomma.ToString();
            }
            switch (n)
            {
                case 1: hrsize += "KB"; break;  // " KiloByte"; break;
                case 2: hrsize += "MB"; break;  // " MegaByte"; break;
                case 3: hrsize += "GB"; break;  // " GigaByte"; break;
                case 4: hrsize += "TB"; break;  //" TerraByte"; break;
                case 5: hrsize += "PB"; break;  //" PetaByte"; break;
                case 6: hrsize += "EB"; break;  //" ExaByte"; break; /*long: file.attrs can be max. 9 exabyte*/
                default: hrsize +="B "; break;  //" Byte"; break;
            }
            return hrsize;
        }

        public static bool mysql_insert_row(Sqlcreds sc)
        {
            bool erg = true;

            
            Console.WriteLine(": Versuche Verbindung zu: " + sc.getMysqlConStr());

            MySqlConnection connection = new MySqlConnection(myConnectionString);
            MySqlCommand command = connection.CreateCommand();



            command.CommandText = "INSERT INTO `test`.`memworker` (`name`, `path`, `loc`, `size`, `csum`, `owner`, `group`, `stime`, `atime`, `ctime`, `mtime`, `dups`) VALUES ('" + "" + "', 'c:\\testdateien\\mmmm.pdf', 'c:', '1234', 'mmmmfc92da241694750979ee6cf582f2d5d7d28e18335de05abc54d0560e0f5302860c652bf08d560252aa5e74210546f369fbbbce8c12cfc7957b2652fe9a75', 'm', 'm', CURRENT_TIMESTAMP, '1983-10-10 22:11:02', '1983-01-01 00:11:02', '1983-10-10 22:11:11', '1');";
            MySqlDataReader Reader;
            connection.Open();
            Reader = command.ExecuteReader();
            while (Reader.Read())
            {
                string row = "";
                for (int i = 0; i < Reader.FieldCount; i++)
                    row += Reader.GetValue(i).ToString() + ", ";

                Console.WriteLine(row);
            }
            connection.Close();


            Console.WriteLine("Trenne Verbindung...");
            return erg;
        }

        public static bool mysql_trunc_table(Sqlcreds sc)
        {
            bool erg = true;

            Console.WriteLine("sql_trunc_table: Versuche Verbindung zu: " + sc.getMysqlConStr());
            MySqlConnection connection = new MySqlConnection(sc.getMysqlConStr());
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "TRUNCATE TABLE " + sc.getTable() + ";";
            MySqlDataReader Reader;
            connection.Open();
            Reader = command.ExecuteReader();
            connection.Close();
            Console.WriteLine("sql_trunc_table: Trenne Verbindung...");
            return erg;
        }

        public static bool mysql_drop_table(Sqlcreds sc)
        {
            bool erg = true;

            Console.WriteLine("mysql_drop_table: Versuche Verbindung zu: " + sc.getMysqlConStr());
            MySqlConnection connection = new MySqlConnection(sc.getMysqlConStr());
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "DROP TABLE " + sc.getTable() + ";";
            MySqlDataReader Reader;
            connection.Open();
            Reader = command.ExecuteReader();
            connection.Close();
            Console.WriteLine("mysql_drop_table: Trenne Verbindung...");
            return erg;
        }

        public static bool mysql_create_table(Sqlcreds sc)
        {   
            bool erg = true;
            
            Console.WriteLine("sql_trunc_table: Versuche Verbindung zu: " + sc.getMysqlConStr());
            MySqlConnection connection = new MySqlConnection(sc.getMysqlConStr());
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE IF NOT EXISTS `"+sc.getTable()+"` ("+
              "`name` varchar(255) COLLATE utf8_unicode_ci NOT NULL DEFAULT '' COMMENT 'Dateiname',"+
              "`path` varchar(255) COLLATE utf8_unicode_ci NOT NULL,"+
              "`loc` varchar(255) COLLATE utf8_unicode_ci NOT NULL COMMENT 'Ort ',"+
              "`size` bigint(14) unsigned NOT NULL COMMENT 'Dateigroesse in Byte',"+
              "`csum` varchar(129) CHARACTER SET ascii NOT NULL COMMENT 'SHA512-Pruefsumme',"+
              "`dom` varchar(255) COLLATE utf8_unicode_ci NOT NULL COMMENT 'Benutzerdomaene',"+
              "`owner` varchar(255) COLLATE utf8_unicode_ci NOT NULL COMMENT 'Eigentuemer der Datei',"+
              "`group` varchar(255) COLLATE utf8_unicode_ci NOT NULL COMMENT 'Gruppe der datei',"+
              "`stime` int(10) unsigned NOT NULL DEFAULT '1' COMMENT 'snapshottime im unixtimeformat',"+
              "`atime` int(10) unsigned NOT NULL DEFAULT '1' COMMENT 'accesstime',"+
              "`ctime` int(10) unsigned NOT NULL DEFAULT '1' COMMENT 'createtime',"+
              "`mtime` int(10) unsigned NOT NULL DEFAULT '1' COMMENT 'modifytime',"+
              "`dups` int(10) unsigned NOT NULL DEFAULT '1' COMMENT 'Duplikate',"+
              "PRIMARY KEY (`path`,`csum`)"+
            ") ENGINE=MEMORY DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci COMMENT='workertable';";
            MySqlDataReader Reader;
            connection.Open();
            Reader = command.ExecuteReader();
            connection.Close();
            Console.WriteLine("sql_trunc_table: Trenne Verbindung...");
            return erg; 
            
        }

        public static bool mysql_dump_table(Sqlcreds sc)
        {
            bool erg = true;
            int counter = 0;
            
            Console.WriteLine("sql_dump: Versuche Verbindung zu: " + sc.getMysqlConStr());

            MySqlConnection connection = new MySqlConnection(myConnectionString);
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM "+sc.getTable()+";";
            MySqlDataReader Reader;
            connection.Open();
            Reader = command.ExecuteReader();
            while (Reader.Read())
            {
                string row = "";
                for (int i = 0; i < Reader.FieldCount; i++)
                    row += Reader.GetValue(i).ToString() + ", ";

                Console.WriteLine(row);
            }
            connection.Close();


            Console.WriteLine("sql_dump: Trenne Verbindung...");
            return erg;
        }

        private static void mssql_insert_row(Sqlcreds sc)
        {
            try
            {                                                                           /*Encoding vom Betriebssystem holen, fuer Umlaute*/
                using (StreamReader readFile = new StreamReader("C:\\temp\\Cuso.csv", System.Text.Encoding.Default))
                {
                    string line;
                    string[] row;



                    while ((line = readFile.ReadLine()) != null)
                    {

                        row = line.Split(';');

                        if (!(row[0] == "ist Firma"))
                        {
                            ds++;
                            SqlConnection sqlCon = new SqlConnection();
                            sqlCon.ConnectionString = strConString;
                            sqlCon.Open();
                            string strSqlQuery = "INSERT INTO Cuso_Kunden(istFirma, VIPFlag, Kundennummer, Vorname, Name, Firmenname, Strasse, PLZ, Ort, Teilnehmer) VALUES ('" + row[0].Replace("'", "") + "', '" + row[1].Replace("'", "") + "', '" + row[2].Replace("'", "") + "', '" + row[3].Replace("'", "") + "', '" + row[4].Replace("'", "") + "', '" + row[5].Replace("'", "") + "', '" + row[6].Replace("'", "") + "', '" + row[7].Replace("'", "") + "', '" + row[8].Replace("'", "") + "', '" + row[9].Replace("'", "") + "')";

                            SqlCommand sqlCmd = new SqlCommand(strSqlQuery, sqlCon);
                            int intCheckQuery = sqlCmd.ExecuteNonQuery();
                            if (intCheckQuery > 0) Console.WriteLine(ds + ": " + row[4] + ", " + row[3]);
                            else Console.WriteLine("Datensatz konnte nicht hinzugefügt werden");
                            sqlCon.Close();
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("FEHLER: " + e.Message);
            }

            
        }

        private static void mssql_trunc_table(Sqlcreds sc)
        {
            // Neue Datenbankverbindung
            SqlConnection sqlCon = new SqlConnection();
            // Connectionstring wird sqlConnection zugewiesen
            sqlCon.ConnectionString = sc.getMssqlConStr();
            // Verbindung zur Datenbank herstellen
            sqlCon.Open();
            // Querystring
            string strSqlQuery = "TRUNCATE TABLE "+sc.getTable()+";";
            // SQLCommand
            SqlCommand sqlCmd = new SqlCommand(strSqlQuery, sqlCon);
            int intCheckQuery = sqlCmd.ExecuteNonQuery();
            Console.WriteLine("Tabelle geleert: "+ sc.getDatabase()+"."+sc.getTable());
            sqlCon.Close();
        }

    }
}
