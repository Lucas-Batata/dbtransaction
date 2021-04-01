//using System;
////using System.Data.SqlClient;
////using System.IO;
////using System.Transactions;
////using System.Xml;
////using EnterpriseLib;
//using System.Data;
//using System.Data.Common;
//using Microsoft.Practices.EnterpriseLibrary.Common;
//using Microsoft.Practices.EnterpriseLibrary.Data;
//using System.Data.SqlClient;
//using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;
//using System.Configuration;

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data.Common;

namespace DBConexion_Enterprise
{

    class Program
    {
        private Database mCon;
        static void Main(string[] args)
        {


            Console.WriteLine("INICIOU \n");

            var Connect = new Program();

            //Connect.Conectar();

            //Connect.Conectar1();

            Connect.Conectar3();

            Console.WriteLine("\nFINALIZOU");
        }

        public void Conectar1()
        {
            var nameDb = ((DatabaseSettings)ConfigurationManager.GetSection("dataConfiguration")).DefaultDatabase;

            var strConn = ConfigurationManager.ConnectionStrings[nameDb].ConnectionString;

            mCon = new SqlDatabase(strConn);

            DbCommand cmd = mCon.GetStoredProcCommand("pp_selPessoas");
            mCon.DiscoverParameters(cmd);

            using (IDataReader reader = mCon.ExecuteReader(cmd))
            {
                while (reader.Read())
                {
                    Console.WriteLine(reader["nome"]);
                }
            }

        }
        public void Conectar()
        {
            var nameDb = ((DatabaseSettings)ConfigurationManager.GetSection("dataConfiguration")).DefaultDatabase;

            var strConn = ConfigurationManager.ConnectionStrings[nameDb].ConnectionString;

            mCon = new SqlDatabase(strConn);

            DbConnection con = mCon.CreateConnection();

            try
            {
               

                con.Open();
                var tran = con.BeginTransaction();

                DbCommand cmd = con.CreateCommand();

                var pn = cmd.CreateParameter();

                pn.ParameterName = "mNome";
                pn.DbType = DbType.String;
                pn.Value = "Joao";

                cmd.Parameters.Add(pn);

                pn = cmd.CreateParameter();
                pn.ParameterName = "mId";
                pn.DbType = DbType.Int32;
                pn.Value = 2;

                cmd.Parameters.Add(pn);
         

                cmd.CommandText = "select * from cadastros; insert into cadastros values (@mNome, @mId); insert into cadastros values (@mNome, @mId)";
                cmd.Transaction = tran;
                //var ret = cmd.ExecuteScalar();


                using(DbDataReader retorno = cmd.ExecuteReader())
                {
                    if ((retorno!= null) && (retorno.RecordsAffected != 0))
                    {
                        while(retorno.Read())
                        {
                            Console.Write(retorno["cadastroid"] + " - ");
                            Console.WriteLine(retorno["nome"].ToString());
                        }
                        // Console.WriteLine(retorno.RecordsAffected);
                        Console.WriteLine(con.State);
                    }
                }

                if (con.State.ToString() == "Open")
                {
                    Console.WriteLine("ABERTOOO");
                }


                tran.Commit();
                
                if (con.State.ToString() == "Open")
                {
                    con.Close();
                }

            }
            catch (Exception ex)
            {
                if (con.State.ToString() == "Open")
                {
                    con.Close();
                }
                Console.WriteLine(ex);
            }


        }

        //Como vai ficar no TDRoteador 31/03/2021
        public void Conectar3()
        {
            var nameDb = ((DatabaseSettings)ConfigurationManager.GetSection("dataConfiguration")).DefaultDatabase;

            var strConn = ConfigurationManager.ConnectionStrings[nameDb].ConnectionString;

            mCon = new SqlDatabase(strConn);

            using (DbConnection dbConnection = mCon.CreateConnection())
            {
                DbTransaction transaction = null;
                try
                {
                    dbConnection.Open();
                   // Console.WriteLine(transaction == null);
                    
                    transaction = dbConnection.BeginTransaction();
                   

                    DbCommand command = dbConnection.CreateCommand();

               

                    var parameterParceiro = command.CreateParameter();
                    parameterParceiro.ParameterName = "id";
                    parameterParceiro.DbType = DbType.Int32;
                    parameterParceiro.Value = 199;
                    command.Parameters.Add(parameterParceiro);

                    command.CommandText = "pp_selPessoas @id";
                    command.Transaction = transaction;


                    using (DbDataReader dbReturn = command.ExecuteReader())
                    {
                        while (dbReturn.Read())
                        {
                            Console.WriteLine(dbReturn["nome"]);
                        }

                        dbConnection.Close();
                        throw new Exception("timeout");
                    }

                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    if (dbConnection.State == ConnectionState.Open)
                    {
                        if(transaction != null)
                        {
                            transaction.Rollback();
                        }
                    }
                }
                finally
                {
                    if (dbConnection.State == ConnectionState.Open)
                    {
                        if (transaction != null)
                        {
                            transaction.Dispose();
                        }

                        dbConnection.Close();
                    }
                    
                }
            }
        }

        public void Conectar2()
        {
            var nameDb = ((DatabaseSettings)ConfigurationManager.GetSection("dataConfiguration")).DefaultDatabase;

            var strConn = ConfigurationManager.ConnectionStrings[nameDb].ConnectionString;

            mCon = new SqlDatabase(strConn);

            using (DbConnection dbConnection = mCon.CreateConnection())
            {

                dbConnection.Open();

                using (DbTransaction transaction = dbConnection.BeginTransaction())
                {

                    DbCommand command = dbConnection.CreateCommand();

                    var parameterParceiro = command.CreateParameter();
                    parameterParceiro.ParameterName = "id";
                    parameterParceiro.DbType = DbType.Int32;
                    parameterParceiro.Value = 199;
                    command.Parameters.Add(parameterParceiro);

                    command.CommandText = "pp_selPessoas @id";
                    command.Transaction = transaction;

                    try
                    {
                        using (DbDataReader dbReturn = command.ExecuteReader())
                        {
                            if ((dbReturn != null) && (dbReturn.RecordsAffected != 0))
                            {
                                while (dbReturn.Read())
                                {
                                    Console.WriteLine(dbReturn["nome"]);
                                }
                            }
                        }

                        transaction.Commit();
                        
                        if (dbConnection.State == ConnectionState.Open)
                        {
                            Console.WriteLine("OPEN");
                        }
                        dbConnection.Close();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        if (dbConnection.State.ToString() == "Open")
                        {
                            transaction.Rollback();
                            dbConnection.Close();
                        }   
                    }
                    finally
                    {
                        if (dbConnection.State == ConnectionState.Open)
                        {
                            Console.WriteLine("CLOSED");
                        }
                    }
                }
            }
        }

    }
}


//IDbConnection conn = DataAccess.CreateConnection();
//conn.Open();

//IDbTransaction tran = conn.BeginTransaction();
//cadastro.Transacao = tran;
