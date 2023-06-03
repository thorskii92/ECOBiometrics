using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace BiometricsNew
    {
        public class msqlcon
        {
            public static MySqlConnection con = new MySqlConnection ("server=localhost; user id=root; password='rootpass123'; database=ecosoldb;persistsecurityinfo=True; SslMode=none" );
        }
        public class CheckOpen
        {
                public static void cons ( )
                    {
                        if (msqlcon.con.State != ConnectionState.Open)
                            {
                                msqlcon.con.Open ();
                            }

                    }

                public static void conx ( )
                    {
                        if (msqlcon.con.State != ConnectionState.Closed)
                            {
                                msqlcon.con.Close ();
                            }
                    }
        }
    }
