using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace worker
{
    class Sqlcreds
    {

        private string username = "";
        private string password = "";   //todo: gegen pwhash authentifizeren
        private string host     = "";
        private string database = "";
        private string table    = "";

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
    }
}
