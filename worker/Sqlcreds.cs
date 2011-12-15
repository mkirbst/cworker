using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace worker
{
    class Sqlcreds
    {
        public Sqlcreds()
        {
            ctime = this.getTimestamp();
            username = "";
            password = "";
            host = "";
            database = "";
            table = "snapshot_" + path + "_" + ctime;

        }

        public Sqlcreds(String tName)
        {
            ctime = this.getTimestamp();
            username = "";
            password = "";
            host = "";
            database = "";
            table = tName;

        }

        private string username;
        private string password;
        private string host;
        private string database;
        private string table;
        private string ctime;
        private string path;

        public void     setUsername(String un)  { username = un;    }
        public string   getUsername()           { return username;  }
        public void     setPassword(String pw)  { password = pw;    }
        public string   getPassword()           { return password;  }
        public void     setHost(String hs)      { host = hs;        }
        public string   getHost()               { return host;      }
        public void     setDatabase(String db)  { database = db;    }
        public string   getDatabase()           { return database;  }
        public void     setTable(String tb)     { table = tb;       }
        public string   getTable()              { return table;     }
        public void     setPath(String p)       { path = p;         }
        public string   getPath()               { return path;      }

        public String getMysqlConStr() {
            return  "SERVER=" + host + 
                    ";DATABASE=" + database +
                    ";UID=" + username +
                    ";PASSWORD=" + password + ";";
        }

        public String getMssqlConStr()
        {
            return  "Data Source=" + host +
                    ";Initial Catalog= " + database + 
                    ";User Id= " + username + 
                    ";Password=" + password + ";";
        }

        private String getTimestamp()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        public string getTableName()
        {
            return "test";
            //return "snapshot_"+path+"_"+ctime;
        }
    
    }
}
