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
            //walkFolders("C:\\Users\\kirbst\\Desktop\\buecher");

            
            sql_dump();
        }

        public static bool sql_dump()
        {
            bool erg = true;

            string myConnectionString = "SERVER=localhost;" +
                                        "DATABASE=test;" +
                                        "UID=admin;" +
                                        "PASSWORD=cNtN.5db6!;";

            Console.WriteLine("Versuche Verbindung zu: " + myConnectionString);
            
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


            Console.WriteLine("Trenne Verbindung...");
            return erg;
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
