using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Principal;
//using MySql.Data.VisualStudio;
using MySql.Data.MySqlClient;

/** Quellenverzeichnis:
 *  walkFolders:    http://dotnet-snippets.de/dns/rekursiver-verzeichnislauf-SID462.aspx
 *  md5:            http://dotnet-snippets.de/dns/md5-hash-von-dateien-ermitteln-SID66.aspx
 * 
 */

namespace worker  {
    class Program    {
        static void Main(string[] args)   {
            
            //walkFolders("q:\\");
            //walkFolders("C:\\Users\\kirbst\\Desktop\\buecher\\hardware\\apple");

            sql_trunc_table();

            walkFoldersSql("Q:\\");
//            walkFoldersSql("C:\\Users\\kirbst\\Desktop\\buecher\\hardware\\apple");

            

            //sql_insert();
            //sql_dump();
            //
        }

        //INSERT INTO `test`.`worker` (`name`, `path`, `loc`, `size`, `csum`, `owner`, `group`, `stime`, `atime`, `ctime`, `mtime`, `dups`) VALUES ('testdatei.pdf', 'c:\\testdateien\\testdatei.pdf', 'c:', '1234', '1f40fc92da241694750979ee6cf582f2d5d7d28e18335de05abc54d0560e0f530286 0c652bf08d560252aa5e74210546f369fbbbce8c12cfc7957b2652fe9a75', 'm', 'm', CURRENT_TIMESTAMP, '1983-10-10 22:11:02', '1983-01-01 00:11:02', '1983-10-10 22:11:11', '1');


        public static bool sql_insert()
        {
            bool erg = true;

            string myConnectionString = "SERVER=localhost;" +
                                        "DATABASE=test;" +
                                        "UID=admin;" +
                                        "PASSWORD=password;";

            Console.WriteLine("sql_insert: Versuche Verbindung zu: " + myConnectionString);

            MySqlConnection connection = new MySqlConnection(myConnectionString);
            MySqlCommand command = connection.CreateCommand();



            command.CommandText = "INSERT INTO `test`.`worker` (`name`, `path`, `loc`, `size`, `csum`, `owner`, `group`, `stime`, `atime`, `ctime`, `mtime`, `dups`) VALUES ('" + "" + "', 'c:\\testdateien\\mmmm.pdf', 'c:', '1234', 'mmmmfc92da241694750979ee6cf582f2d5d7d28e18335de05abc54d0560e0f5302860c652bf08d560252aa5e74210546f369fbbbce8c12cfc7957b2652fe9a75', 'm', 'm', CURRENT_TIMESTAMP, '1983-10-10 22:11:02', '1983-01-01 00:11:02', '1983-10-10 22:11:11', '1');";
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

        public static bool sql_trunc_table()
        {
            bool erg = true;
            string myConnectionString = "SERVER=localhost;" +
                                        "DATABASE=test;" +
                                        "UID=admin;" +
                                        "PASSWORD=password;";

            Console.WriteLine("sql_trunc_table: Versuche Verbindung zu: " + myConnectionString);
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "TRUNCATE TABLE worker;";
            MySqlDataReader Reader;
            connection.Open();
            Reader = command.ExecuteReader();
            connection.Close();
            Console.WriteLine("sql_trunc_table: Trenne Verbindung...");
            return erg;
        }

        public static bool sql_dump()
        {
            bool erg = true;
            int counter = 0;
            string myConnectionString = "SERVER=localhost;" +
                                        "DATABASE=test;" +
                                        "UID=admin;" +
                                        "PASSWORD=password;";

            Console.WriteLine("sql_dump: Versuche Verbindung zu: " + myConnectionString);
            
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM worker";
            MySqlDataReader Reader;
            connection.Open();
            Reader = command.ExecuteReader();
            while (Reader.Read())  {
                string row = "";
                for (int i = 0; i < Reader.FieldCount; i++)
                    row += Reader.GetValue(i).ToString() + ", ";
                
                Console.WriteLine(row);
            }
            connection.Close();


            Console.WriteLine("sql_dump: Trenne Verbindung...");
            return erg;
        }



        private static void walkFoldersSql(string DirectorySql)
        {
            walkFoldersSql(new DirectoryInfo(DirectorySql));
        }

        private static void walkFoldersSql(DirectoryInfo disql)
        {

            string sqlcommand = "";
            int counter = 0;
            string myConnectionString = "SERVER=localhost;" +
                                        "DATABASE=test;" +
                                        "UID=admin;" +
                                        "PASSWORD=password;";

            Console.WriteLine("walkFoldersSql: Versuche Verbindung ...");

            try
            {
                // Alle Verzeichnisse rekursiv durchlaufen
                foreach (DirectoryInfo subdir in disql.GetDirectories())
                {
                    walkFoldersSql(subdir);
                }

                // Alle Dateien durchlaufen
                foreach (FileInfo fi in disql.GetFiles())
                {
                    MySqlConnection connection = new MySqlConnection(myConnectionString);
                    MySqlCommand command = connection.CreateCommand();
                    Console.WriteLine("walkFoldersSql: add: " + fi.FullName + " " + UnixTime(DateTime.UtcNow));
                    connection.Open();
                    command.CommandText = "INSERT INTO `test`.`worker` (`name`, `path`, `loc`, `size`, `csum`, `dom`, `owner`, `group`, `stime`, `atime`, `ctime`, `mtime`, `dups`) VALUES ('" + fi.Name + "', '" + fi.FullName + "', '" + fi.FullName.Split('\\')[0] + "', '" + fi.Length + "', '" + Datei2SHA(fi.FullName) + "', '" + File.GetAccessControl(@fi.FullName).GetOwner(typeof(NTAccount)).ToString().Split('\\')[0] + "', '" + File.GetAccessControl(@fi.FullName).GetOwner(typeof(NTAccount)).ToString().Split('\\')[1] + "', '" + File.GetAccessControl(@fi.FullName).GetGroup(typeof(NTAccount)).ToString().Split('\\')[1] + "', '" + UnixTime(DateTime.Now) + "', '" +  UnixTime(fi.LastAccessTime) + "', '" +  UnixTime(fi.CreationTime) + "', '" +  UnixTime(fi.LastWriteTime) + "', '1');";
                    MySqlDataReader Reader;
                    Reader = command.ExecuteReader();
                    counter+=1;
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("walkFoldersSql: " + counter + " Zeilen hinzugefuegt.");
        }

        public static long UnixTimeNow()
        {
            TimeSpan _TimeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)_TimeSpan.TotalSeconds;
        }

        public static long UnixTime(DateTime filetime)
        {
            TimeSpan _TimeSpan = (filetime - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)_TimeSpan.TotalSeconds;
        }

        
        private static void walkFolders(string Directory)  {
            walkFolders(new DirectoryInfo(Directory));
        }

        private static void walkFolders(DirectoryInfo di)  {
            try  {
                // Alle Verzeichnisse rekursiv durchlaufen
                foreach (DirectoryInfo subdir in di.GetDirectories())  {
                    walkFolders(subdir);
                }

                // Alle Dateien durchlaufen
                foreach (FileInfo fi in di.GetFiles())
                {
                    //Console.Write(hrs(fi.Length) + " " + File.GetAccessControl(@fi.FullName).GetOwner(typeof(NTAccount)) + " " + fi.FullName + "  SHA512: [" + Datei2SHA(fi.FullName) + "]" + "  MD5: [" + Datei2MD5(fi.FullName) + "]" + "\r");
                    Console.Write("ctime: "+fi.CreationTime+"  atime: "+fi.LastAccessTime +"  mtime: "+fi.LastWriteTime+":  " + fi.Name + " L-SHA: " +Datei2SHA(fi.FullName).Length +"\n") ;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /**md5
         */
        public static string Datei2MD5(string Dateipfad)
        {
            //Datei einlesen
            System.IO.FileStream FileCheck = System.IO.File.OpenRead(Dateipfad);
            // MD5-Hash aus dem Byte-Array berechnen
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] md5Hash = md5.ComputeHash(FileCheck);
            FileCheck.Close();

            //in string wandeln
            string Berechnet = BitConverter.ToString(md5Hash).Replace("-", "").ToLower();
            return Berechnet;

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
    }
}
