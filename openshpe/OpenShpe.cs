//Steven Rodriguez
//OpenSHPE class provides access to openshpe via rest calls

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.ComponentModel;

namespace openshpe
{
    class OpenShpe
    {        
        private const string PARAM_ACCESS_KEY = "SHPEAccessKey";
	    private const string PARAM_SIGNATURE = "Signature";

        private string accessKey { get; set; } //open shpe key
        private string secretKey { get; set; } //private key
        private string host { get; set; }
	    private string protocol = "https";
        private int port { get; set; }
        private bool ignoreBadCertificates { get; set; }

        private System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
        
        private HttpMethod httpMethod { get; set; } //defaulted to GET 
        public enum HttpMethod
        {
           GET,
           PUT,
           POST,
           DELETE
        }

        public OpenShpe(string hostname, string accessKey, string secretKey, bool ignoreBadCertificates = false, int port = 443)
        {
            if (hostname == null || accessKey == null || secretKey == null)
            {
                throw new System.ArgumentException("None of the arguments can be null or empty");
            }

            this.host = hostname;
            this.accessKey = accessKey;
            this.secretKey = secretKey;
            this.port = port;
            this.ignoreBadCertificates = ignoreBadCertificates;
            this.httpMethod = HttpMethod.GET;
        }

        /// <summary>
        /// Builds the full uri required and then performs the Http request.
        /// </summary>
        /// <param name="servicePath">Path to the rest file</param>
        /// <param name="parameters">Parameters reqiured</param>
        /// <returns>The full stream recieved (xml)</returns>
        public Stream GetRestResponse(string servicePath, SortedDictionary<string, string> parameters)
        {
            if (servicePath == "" || servicePath == null)
            {
                throw new ArgumentNullException("The service path must be input");
            }

            Stream responseStream = null;
            string uriString = "";
            string parametersString = "";
            string signature = "";

            parametersString = BuildParameters(parameters);
            uriString = BuildURI(servicePath, parametersString);

            Uri uri = new Uri(uriString);

            signature = BuildSignature(uri, parametersString);

            uriString = uriString + "&" + PARAM_SIGNATURE + "=" + signature;

            uriString = HttpUtility.UrlDecode(uriString);

            uri = new Uri(uriString);

            HttpWebRequest httpRequest = WebRequest.Create(uri) as HttpWebRequest;

            // allows for validation of SSL conversations
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
            HttpWebResponse response;
            
            try
            {
                // Get response  
                response = httpRequest.GetResponse() as HttpWebResponse;
                responseStream = response.GetResponseStream();  
            }
            catch (Exception)
            {
                throw;
            }

            return responseStream;
        }

        /// <summary>
        /// Builds and calculates the full HMAC signature
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="parametersString"></param>
        /// <returns></returns>
        private string BuildSignature(Uri uri, string parametersString)
        {
            StringBuilder signatureBuilder = new StringBuilder();
            signatureBuilder.Append(httpMethod.ToString());
            signatureBuilder.Append("\n");
            signatureBuilder.Append(uri.Host);
            signatureBuilder.Append("\n");
            signatureBuilder.Append(uri.AbsolutePath);
            signatureBuilder.Append("\n");
            signatureBuilder.Append(parametersString);

            //Apply the key on hmac
            string signature = GenerateSignature(signatureBuilder.ToString());

            return signature;
        }

        /// <summary>
        /// Applies the hmac using the secret key
        /// </summary>
        /// <param name="currentSignature"></param>
        /// <returns></returns>
        private string GenerateSignature(string currentSignature)
        {
            string signature = "";

            HMACSHA256 myhmacsha256 = new HMACSHA256(encoding.GetBytes(secretKey));
            byte[] hash = myhmacsha256.ComputeHash(encoding.GetBytes(currentSignature));

            signature = Convert.ToBase64String(hash);

            return signature;
        }

        /// <summary>
        /// Builds the full URI without the calculated signature.
        /// </summary>
        /// <param name="servicePath"></param>
        /// <param name="parametersString"></param>
        /// <returns></returns>
        private string BuildURI(string servicePath, string parametersString)
        {
            StringBuilder uriBuilder = new StringBuilder();
            uriBuilder.Append(protocol);
            uriBuilder.Append("://");
            uriBuilder.Append(host);
            uriBuilder.Append(":");
            uriBuilder.Append(port);
            uriBuilder.Append(servicePath);
            uriBuilder.Append("?");
            uriBuilder.Append(HttpUtility.UrlEncode(encoding.GetBytes(parametersString)));
            return uriBuilder.ToString();
        }

        /// <summary>
        /// PARAM_ACCESS_KEY is always required
        /// additional parameters may not be
        /// additional parameters must be sorted
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string BuildParameters(SortedDictionary<string, string> parameters)
        {
            StringBuilder builder = new StringBuilder();

            //always add access key
            builder.Append(PARAM_ACCESS_KEY);
            builder.Append("=");
            builder.Append(accessKey);

            if (parameters != null && parameters.Count > 0)
            {
                foreach (KeyValuePair<string, string> pair in parameters)
                {
                    builder.Append("&");
                    builder.Append(pair.Key);
                    builder.Append("=");
                    builder.Append(pair.Value);
                }
            }

            return builder.ToString();
        }

        //To allow any certificate
        private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return ignoreBadCertificates;
        }

        public void SetHttpMethod(string method)
        {
            switch (method)
            {
                case "G":
                    httpMethod = HttpMethod.GET;
                    break;
                case "P":
                    httpMethod = HttpMethod.PUT;
                    break;
                case "O":
                    httpMethod = HttpMethod.POST;
                    break;
                case "D":
                    httpMethod = HttpMethod.DELETE;
                    break;
                default:
                    httpMethod = HttpMethod.GET;
                    break;
            }
        }
    }        
}
