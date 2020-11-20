using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;


namespace HCALib
{

    public class Engine
    {

        /// <summary>
        /// ...
        /// </summary>
        public static async Task Exec(Data data)
        {
            X509Certificate2 myCertificate = null;

            var handler = new WebRequestHandler()
            {
                //Proxy = null,
                //UseProxy = true,
                //UseDefaultCredentials=true,                
                DefaultProxyCredentials = CredentialCache.DefaultCredentials,   // ????
                AllowAutoRedirect = false,

                ServerCertificateValidationCallback =
                    (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
                    {
                        myCertificate = new X509Certificate2(certificate);
                        return true;
                    }
            };


            using (var httpClient = new HttpClient(handler))
            {
                foreach (var check in data.CheckList.Where(x => x.Active))
                {
                    if (DateTime.UtcNow.Subtract(check.LastCheckDT).TotalHours > data.CheckIntervalHours || data.ContinuousCheck)
                    {
                        try
                        {
                            myCertificate = null;
                            //await httpClient.GetStringAsync(check.Url);
                            await httpClient.GetAsync(check.Url);
                            var dt = myCertificate.NotAfter;
                            check.ValidToDT = myCertificate.NotAfter;
                            check.LastError = null;

                            double expirationDays = myCertificate.NotAfter.Subtract(DateTime.UtcNow).TotalDays;
                            if (expirationDays > check.AlertDays)
                            {
                                check.Status = Status.OK;
                            }
                            else if (expirationDays<=check.AlertDays && expirationDays>0)
                            {
                                check.Status = Status.EXPIRING;
                            }
                            else
                            {
                                check.Status = Status.EXPIRED;
                            }
                        }
                        catch (Exception ex)
                        {
                            //check.ValidToDT = null;
                            check.Status = Status.ERROR;
                            check.ValidToDT = DateTime.MinValue;
                            check.LastError = "EX:" + ex.Message;
                        }
                        finally
                        {
                            check.LastCheckDT = DateTime.UtcNow;
                        }                       
                    }
                }
            }
        }

    }

}
