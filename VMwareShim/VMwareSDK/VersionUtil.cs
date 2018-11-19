using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace AppUtil
{
    // <summary>
    /// VersionUtil that determines the NameSpace And Supported Versions
    /// </summary>
    public class VersionUtil
    {

        public static ArrayList getSupportedVersions(String urlString)
        {
            HttpWebRequest request;
            HttpWebResponse resp;
            String respString;
            String targetNameSpace = "";

            XmlDocument versionXML = new XmlDocument();
            ArrayList appVersionList = new ArrayList();

            String wsdlUrlString = urlString.Substring(0, urlString.IndexOf("/sdk") + 4)
                                                       + "/vimService?wsdl";
            String vimServiceXmlUrlString = urlString.Substring(0, urlString.IndexOf("/sdk") + 4)
                                     + "/vimServiceVersions.xml";
            try
            {
                Boolean isServiceXmlExists = true;
                try
                {
                    request = (HttpWebRequest)WebRequest.Create(vimServiceXmlUrlString);
                    request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.None;
                    request.ContentType = "text/xml";
                    resp = (HttpWebResponse)request.GetResponse();
                    StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.ASCII);
                    respString = sr.ReadToEnd();
                    targetNameSpace = "urn:vim25";
                    appVersionList = getAPIVersions(respString);
                }
                catch (WebException e)
                {
                    if (e.Message.Equals("The remote server returned an error: (404) Not Found."))
                    {
                        isServiceXmlExists = false;
                    }
                }
                if (!isServiceXmlExists)
                {
                    request = (HttpWebRequest)WebRequest.Create(wsdlUrlString);
                    request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.None;
                    request.ContentType = "text/xml";
                    resp = (HttpWebResponse)request.GetResponse();
                    StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.ASCII);
                    respString = sr.ReadToEnd();
                    versionXML.LoadXml(respString);
                    targetNameSpace = versionXML.DocumentElement.GetAttribute("targetNamespace");

                    if (targetNameSpace.Equals("urn:vim25Service"))
                    {
                        appVersionList.Add("2.5");
                    }
                    else
                    {
                        appVersionList.Add("2.0");
                    }
                }
                return appVersionList;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(" Failed to find Version XML");
                throw e;
            }
            finally
            {
            }
        }
        public static ArrayList getAPIVersions(String respString)
        {
            String targetNameSpace;
            String name_NODE;
            String version_NODE;
            XmlDocument versionXML = new XmlDocument();
            XmlNodeList versionNodeList;
            ArrayList priorVersionsList = new ArrayList();
            ArrayList appVersionList = new ArrayList();
            versionXML = new XmlDocument();
            versionXML.LoadXml(respString);
            targetNameSpace = "urn:vim25";
            name_NODE = "/namespaces/namespace/name";
            if (versionXML.SelectSingleNode(name_NODE).InnerText.Equals(targetNameSpace))
            {
                version_NODE = "/namespaces/namespace/priorVersions/version";
                versionNodeList = versionXML.SelectNodes(version_NODE);

                for (int i = 0; i < versionNodeList.Count; i++)
                {
                    XmlNode pversionNode = versionNodeList.Item(i);
                    priorVersionsList.Add(pversionNode.InnerText);

                }
                version_NODE = "/namespaces/namespace/version";
                XmlNode versionNode = versionXML.SelectSingleNode(version_NODE);
                priorVersionsList.Add(versionNode.InnerText);
            }
            return priorVersionsList;
        }

        public static Vim25Api.ManagedObjectReference convertManagedObjectReference(
         Vim25Api.ManagedObjectReference mor)
        {
            Vim25Api.ManagedObjectReference morr1
               = new Vim25Api.ManagedObjectReference();
            morr1.type = mor.type;
            morr1.Value = mor.Value;
            return morr1;
        }
        public static Boolean isApiVersionSupported(ArrayList apiVersions, String version)
        {
            Boolean flag = false;
            for (int i = 0; i < apiVersions.Count; i++)
            {
                String ver = (String)apiVersions[i];
                if (ver.Equals(version))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }
    }
}
