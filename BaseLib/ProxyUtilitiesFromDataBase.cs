﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace BaseLib
{
    public class ProxyUtilitiesFromDataBase
    {
        #region global declaratiopn
        int accountsPerProxy = 10;
        static int i = 0; 
        #endregion

        #region GetPrivateProxies
        public List<string> GetPrivateProxies()
        {
            List<string> lst_Proxies = new List<string>();
            try
            {
                clsDBQueryManager setting = new clsDBQueryManager();
                DataSet ds = setting.SelectPrivateProxyData();
                if (ds.Tables != null && ds.Tables["tb_Proxies"].Rows.Count > 0)
                {
                    foreach (DataRow dRow in ds.Tables["tb_Proxies"].Rows)
                    {
                        string Proxy = dRow.ItemArray[0].ToString() + ":" + dRow.ItemArray[1].ToString() + ":" + dRow.ItemArray[2].ToString() + ":" + dRow.ItemArray[3].ToString();
                        lst_Proxies.Add(Proxy);
                    }
                }
            }
            catch { }

            return lst_Proxies;
        } 
        #endregion

        #region GetPublicProxies
        public List<string> GetPublicProxies()
        {
            List<string> lst_Proxies = new List<string>();
            try
            {
                clsDBQueryManager setting = new clsDBQueryManager();
                DataSet ds = setting.SelectPublicProxyData();
                if (ds.Tables != null && ds.Tables["tb_Proxies"].Rows.Count > 0)
                {
                    foreach (DataRow dRow in ds.Tables["tb_Proxies"].Rows)
                    {
                        string Proxy = dRow.ItemArray[0].ToString() + ":" + dRow.ItemArray[1].ToString() + ":" + dRow.ItemArray[2].ToString() + ":" + dRow.ItemArray[3].ToString();
                        lst_Proxies.Add(Proxy);
                    }
                }
            }
            catch { }

            return lst_Proxies;
        } 
        #endregion
               
        /// <summary>
        /// Assigns "accountsPerProxy" number of proxies to accounts in Database, only picks up only those accounts where ProxyAddress is Null or Empty
        /// </summary>
        #region AssignProxiesToAccounts
        public void AssignProxiesToAccounts(List<string> lstProxies, int noOfAccountsPerProxy)
        {


            try
            {
                DataSet ds = new DataSet();

                if (noOfAccountsPerProxy > 0)
                {
                    accountsPerProxy = noOfAccountsPerProxy;
                }
                else
                {
                    MessageBox.Show("You entered invalid Accounts per Proxy... Default value \"10\" Set");
                }

                using (SQLiteConnection con = new SQLiteConnection(DataBaseHandler.CONstr))
                {

                    foreach (string item in lstProxies)
                    {
                        try
                        {
                            ds.Clear();

                            string account = item;

                            string proxyAddress = string.Empty;
                            string proxyPort = string.Empty;
                            string proxyUserName = string.Empty;
                            string proxyPassword = string.Empty;

                            int DataCount = account.Split(':').Length;

                            if (DataCount == 2)
                            {
                                proxyAddress = account.Split(':')[0];
                                proxyPort = account.Split(':')[1];
                            }
                            else if (DataCount > 2)
                            {
                                proxyAddress = account.Split(':')[0];
                                proxyPort = account.Split(':')[1];
                                proxyUserName = account.Split(':')[2];
                                proxyPassword = account.Split(':')[3];
                            }

                            //using (SQLiteDataAdapter ad = new SQLiteDataAdapter("SELECT * FROM tb_FBAccount WHERE ProxyAddress = '" + proxyAddress + "'", con))
                            using (SQLiteDataAdapter ad = new SQLiteDataAdapter("SELECT * FROM tb_LinkedInAccount WHERE IPAddress = '" + proxyAddress + "' and IPPort = '" + proxyPort + "'", con))
                            {
                                ad.Fill(ds);
                                if (ds.Tables[0].Rows.Count == 0)
                                {
                                    ds.Clear();
                                    using (SQLiteDataAdapter ad1 = new SQLiteDataAdapter("SELECT * FROM tb_LinkedInAccount WHERE IPAddress = '" + "" + "' OR IPAddress = '" + null + "'", con))
                                    {
                                        ad1.Fill(ds);

                                        int count = accountsPerProxy;  //Set count = accountsPerProxy so that it sets max this number of accounts to each proxy
                                        if (ds.Tables[0].Rows.Count < count)
                                        {
                                            count = ds.Tables[0].Rows.Count;
                                        }
                                        for (int i = 0; i < count; i++)
                                        {
                                            string UpdateQuery = "Update tb_LinkedInAccount Set IPAddress='" + proxyAddress + "', IPPort='" + proxyPort + "', IPUserName='" + proxyUserName + "', IPPassword='" + proxyPassword + "' WHERE UserName='" + ds.Tables[0].Rows[i]["UserName"].ToString() + "'";
                                            DataBaseHandler.UpdateQuery(UpdateQuery, "tb_LinkedInAccount");
                                        }
                                    }

                                }
                            }

                        }
                        catch
                        {
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                GlobusFileHelper.AppendStringToTextfileNewLine(DateTime.Now + " --> Error --> ProxyUtillitesFromDB -- AssignProxiesToAccounts() --> " + ex.Message + "StackTrace --> >>>" + ex.StackTrace, Globals.Path_ProxySettingErroLog);
                GlobusFileHelper.AppendStringToTextfileNewLine("Error --> ProxyUtillitesFromDB -- AssignProxiesToAccounts() --> " + ex.Message + "StackTrace --> >>>" + ex.StackTrace, Globals.Path_LinkedinErrorLogs);
            }

        } 
        #endregion

    }
}
