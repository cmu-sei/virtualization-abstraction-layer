using System;
using System.IO;
using System.Web.Services.Protocols;

namespace AppUtil
{
    /// <summary>
    /// useful Client utility functions.
    /// </summary>
    public class ClientUtil
    {

        private AppUtil _ci;

        public ClientUtil(AppUtil c)
        {
            _ci = c;
        }

        /// <summary>
        /// Prompt user for an integer value
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public int getIntInput(String prompt, int defaultVal)
        {
            string input = getStrInput(prompt);
            if (input == null || input.Length == 0)
                return defaultVal;
            else
                return Int32.Parse(input);
        }

        /// <summary>
        /// Prompt user for an integer value
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public long getLongInput(String prompt, long defaultVal)
        {
            string input = getStrInput(prompt);
            if (input == null || input.Length == 0)
                return defaultVal;
            else
                return long.Parse(input);
        }

        /// <summary>
        /// Prompt user for a string value
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public String getStrInput(String prompt)
        {
            Console.Write(prompt);
            TextReader reader = Console.In;
            return reader.ReadLine();
        }

        public void LogException(Exception e)
        {
            if (e.GetType() == System.Type.GetType("SoapException"))
            {
                SoapException se = (SoapException)e;
                _ci.log.LogLine("Caught SoapException - " +
                   " Actor : " + se.Actor +
                   " Code : " + se.Code +
                   " Detail XML : " + se.Detail.OuterXml);
            }
            else
            {
                _ci.log.LogLine("Caught Exception : " +
                   " Name : " + e.GetType().Name +
                   " Message : " + e.Message +
                   " Trace : " + e.StackTrace);
            }
        }

    }
}
