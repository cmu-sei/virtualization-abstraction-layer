using System;
using System.Collections;
using VMware.Security.CredentialStore;
using System.Net;
namespace AppUtil
{
    public class AppUtil
    {

        public Log log;
        private String _cname;
        private Hashtable optsEntered = new Hashtable();
        private Hashtable userOpts = new Hashtable();
        private Hashtable builtInOpts = new Hashtable();
        private String logfilepath = "";
        public SvcConnection _connection;
        public ServiceUtil _svcUtil;
        private ClientUtil _util;

        /**
       * If there is a global logger already available
       */
        private static Log gLog;

        public static AppUtil initialize(String name, OptionSpec[] userOptions, String[] args)
        {
            AppUtil cb = new AppUtil(name);
            if (userOptions != null)
            {
                cb.addOptions(userOptions);
                cb.parseInput(args);
                cb.validate();
            }
            else
            {
                cb.parseInput(args);
                cb.validate();
            }
            return cb;
        }

        public static AppUtil initialize(String name, String[] args)
        {
            AppUtil cb = initialize(name, null, args);
            return cb;
        }
        public static void ALog(Log glog)
        {
            gLog = glog;
        }

        public AppUtil(String name)
        {
            _util = new ClientUtil(this);
            setup();
            init(name);
        }

        public void setup()
        {
            _svcUtil = new ServiceUtil();
            _connection = new SvcConnection("ServiceInstance");
        }

        public void init(String name)
        {
            builtinOptions();
            if (gLog == null)
            {
                String logDir = System.Environment.GetEnvironmentVariable("TEMP");
                if (logDir == null || logDir.Length == 0)
                {
                    logDir = "";
                }
                logfilepath = logDir + "/" + name + "_";
                log = new Log();
                log.Init(logfilepath, true, true);
            }
            else
            {
                log = gLog;
            }
            _cname = name;
        }

        public void initConnection()
        {
            _svcUtil.Init(this);
        }

        public void addOptions(OptionSpec[] userOptions)
        {
            for (int i = 0; i < userOptions.Length; i++)
            {
                if (userOptions[i].getOptionName() != null && userOptions[i].getOptionName().Length > 0 &&
                   userOptions[i].getOptionDesc() != null && userOptions[i].getOptionDesc().Length > 0 &&
                   userOptions[i].getOptionType() != null && userOptions[i].getOptionType().Length > 0 &&
                   (userOptions[i].getOptionRequired() == 0 || userOptions[i].getOptionName().Length > 1))
                {
                    userOpts.Add(userOptions[i].getOptionName(), userOptions[i]);
                    // DO NOTHING
                }
                else
                {
                    Console.WriteLine("Option " + userOptions[i].getOptionName()
                                      + " definition is not valid");
                    throw new ArgumentException("Option " + userOptions[i].getOptionName()
                                                          + " definition is not valid");
                }
            }
        }

        private void builtinOptions()
        {
            OptionSpec url = new OptionSpec("url", "String", 1, "VI SDK URL to connect to", null);
            OptionSpec userName = new OptionSpec("userName", "String", 1, "Username to connect to the host", null);
            OptionSpec password = new OptionSpec("password", "String", 1, "password of the corresponding user", null);
            OptionSpec config = new OptionSpec("config", "String", 0, "Location of the VI perl configuration file", null);
            OptionSpec protocol = new OptionSpec("protocol", "String", 0, "Protocol used to connect to server", null);
            OptionSpec server = new OptionSpec("server", "String", 0, "VI server to connect to", null);
            OptionSpec portNumber = new OptionSpec("portNumber", "String", 0, "Port used to connect to server", "443");
            OptionSpec servicePath = new OptionSpec("servicePath", "String", 0, "Service path used to connect to server", null);
            OptionSpec sessionFile = new OptionSpec("sessionFile", "String", 0, "File containing session ID/cookie to utilize", null);
            OptionSpec help = new OptionSpec("help", "String", 0, "Display user information for the script", null);
            OptionSpec ignorecert = new OptionSpec("ignorecert", "String", 0, "Ignore the server certificate validation", null);
            OptionSpec ssoUrl = new OptionSpec("ssoUrl", "String", 0, "Single Sign On Server URL", null);
            OptionSpec disablesso = new OptionSpec("disablesso", "String", 0, "Disable the use of SSO server for login, and use deprecated login mechanism instead. 'false' by default", null);
            builtInOpts.Add("url", url);
            builtInOpts.Add("username", userName);
            builtInOpts.Add("password", password);
            builtInOpts.Add("config", config);
            builtInOpts.Add("protocol", protocol);
            builtInOpts.Add("server", server);
            builtInOpts.Add("portnumber", portNumber);
            builtInOpts.Add("servicepath", servicePath);
            builtInOpts.Add("sessionfile", sessionFile);
            builtInOpts.Add("help", help);
            builtInOpts.Add("ignorecert", ignorecert);
            builtInOpts.Add("ssoUrl", ssoUrl);
            builtInOpts.Add("disablesso", disablesso);
        }

        public void parseInput(string[] args)
        {
            try
            {
                getCmdArguments(args);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Exception running : " + e);
            }
            IEnumerator It = optsEntered.Keys.GetEnumerator();
            while (It.MoveNext())
            {
                String keyValue = It.Current.ToString();
                String keyOptions;

                if (optsEntered[keyValue] == null)
                {
                    keyOptions = optsEntered.Keys.ToString();

                }
                else
                {
                    keyOptions = optsEntered[keyValue].ToString();
                }

                Boolean result = checkInputOptions(builtInOpts, keyValue);
                Boolean valid = checkInputOptions(userOpts, keyValue);
                if (result == false && valid == false)
                {
                    Console.WriteLine("Invalid Input Option '" + keyValue + "'");
                    displayUsage();
                }
                result = checkDatatypes(builtInOpts, keyValue, keyOptions);
                valid = checkDatatypes(userOpts, keyValue, keyOptions);
                if (result == false && valid == false)
                {
                    Console.WriteLine("Invalid datatype for Input Option '" + keyValue + "'");
                    displayUsage();
                }
            }
        }

        private void getCmdArguments(string[] args)
        {
            int len = args.Length;
            int i = 0;

            if (len == 0)
            {
                displayUsage();
            }
            while (i < args.Length)
            {
                String val = "";
                String opt = args[i];
                if (opt.StartsWith("--") && optsEntered.ContainsKey(opt.Substring(2)))
                {
                    Console.WriteLine("key '" + opt.Substring(2) + "' already exists ");
                    displayUsage();
                }
                if (args[i].StartsWith("--"))
                {
                    if (args.Length > i + 1)
                    {
                        if (!args[i + 1].StartsWith("--"))
                        {
                            val = args[i + 1];
                            optsEntered[opt.Substring(2)] = val;
                        }
                        else
                        {
                            optsEntered[opt.Substring(2)] = null;
                        }
                    }
                    else
                    {
                        optsEntered[opt.Substring(2)] = null;
                    }
                }
                i++;
            }
        }

        private Boolean checkDatatypes(Hashtable Opts, String keyValue, String keyOptions)
        {
            Boolean valid = false;
            valid = Opts.ContainsKey(keyValue);
            if (valid)
            {
                OptionSpec oSpec = (OptionSpec)Opts[keyValue];
                String dataType = oSpec.getOptionType();
                Boolean result = validateDataType(dataType, keyOptions);
                return result;
            }
            else
            {
                return false;
            }
        }

        private Boolean validateDataType(String dataType, String keyValue)
        {
            try
            {
                if (dataType.Equals("Boolean"))
                {
                    if (keyValue.Equals("true") || keyValue.Equals("false"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (dataType.Equals("Integer"))
                {
                    int val = Int32.Parse(keyValue);
                    return true;
                }
                else if (dataType.Equals("Float"))
                {
                    float.Parse(keyValue);
                    return true;
                }
                else if (dataType.Equals("Long"))
                {
                    long.Parse(keyValue);
                    return true;
                }
                else
                {
                    // DO NOTHING
                }
                return true;
            }
            catch (FormatException e)
            {
                return false;
            }
        }


        private Boolean checkInputOptions(Hashtable checkOptions, String value)
        {
            Boolean valid = false;
            valid = checkOptions.ContainsKey(value);
            return valid;
        }

        public void validate()
        {

            if (optsEntered.Count == 0)
            {
                displayUsage();
            }
            if (optsEntered.Contains("help"))
            {
                displayUsage();
            }
            if (option_is_set("help"))
            {
                displayUsage();
            }
            ArrayList vec = getValue(builtInOpts);
            Boolean flagUsername = true;
            Boolean flagPassword = true;
            for (int i = 0; i < vec.Count; i++)
            {
                if (!optsEntered.ContainsKey(vec[i]))
                {
                    String missingArg = vec[i].ToString();
                    if (missingArg.Equals("password"))
                    {
                        flagPassword = false;
                    }
                    else if (missingArg.Equals("username"))
                    {
                        flagUsername = false;
                    }
                    else
                    {
                        Console.Write("----ERROR: " + vec[i] + " not specified \n");
                        displayUsage();
                    }
                }
            }
            searchForUsernameAndPassword(flagUsername, flagPassword);
            vec = getValue(userOpts);
            for (int i = 0; i < vec.Count; i++)
            {
                if (!optsEntered.ContainsKey(vec[i]))
                {
                    Console.Write("----ERROR: " + vec[i] + " not specified \n");
                    displayUsage();
                }
            }

            if (!optsEntered.ContainsKey("disablesso"))
            {
                if (!optsEntered.ContainsKey("ssoUrl"))
                {
                    Console.WriteLine("Must specify the option --ssoUrl or --disablesso");
                    displayUsage();
                }
            }

            if ((optsEntered.ContainsKey("sessionfile")) &&
              ((optsEntered.ContainsKey("username")) && (optsEntered.ContainsKey("password"))))
            {
                Console.WriteLine("Must have one of command options 'sessionfile' or a 'username' and 'password' pair\n");
                displayUsage();
            }

            if (optsEntered.ContainsKey("ignorecert") || "true".Equals(Environment.GetEnvironmentVariable("VI_IGNORECERT"), StringComparison.CurrentCultureIgnoreCase))
            {
                _connection.ignoreCert = true;
            }
        }

        /*Search Order for Username and Password
        * 1. Command Line Arguments
        * 2. Session File
        * 3. Enviroment Variable
        * 4. Credential Store
        * 5. Prompt the User for Username and Password
        */
        private void searchForUsernameAndPassword(Boolean flagUserName, Boolean flagPassword)
        {
            String username = null;
            String password = null;
            if (flagUserName && flagPassword)
            {
                return;
            }
            else if (optsEntered.ContainsKey("sessionfile"))
            {
                // Do Nothing
                return;
            }
            else
            {

                if (flagPassword == false)
                {
                    password = System.Environment.GetEnvironmentVariable("VI_Password");
                    if (password == null)
                    {
                        String host = null;
                        if (optsEntered.ContainsKey("server"))
                        {
                            foreach (DictionaryEntry uniEntry in optsEntered)
                            {
                                if (uniEntry.Key.Equals("server") && uniEntry.Value != null)
                                {
                                    host = uniEntry.Value.ToString();
                                }
                            }
                        }
                        else if (optsEntered.ContainsKey("url"))
                        {
                            String urlString = get_option("url");
                            foreach (DictionaryEntry uniEntry in optsEntered)
                            {
                                if (uniEntry.Key.Equals("url") && uniEntry.Value != null)
                                {
                                    urlString = uniEntry.Value.ToString();
                                }
                            }
                            if (urlString.IndexOf("https://") != -1)
                            {
                                int sind = 8;
                                int lind = urlString.IndexOf("/sdk");
                                host = urlString.Substring(sind, lind - 8);
                            }
                            else if (urlString.IndexOf("http://") != -1)
                            {
                                int sind = 7;
                                int lind = urlString.IndexOf("/sdk");
                                host = urlString.Substring(sind, lind - 7);
                            }
                            else
                            {
                                host = urlString;
                            }
                        }
                        else
                        {
                            // User Neds to Specify Either User Name aand Password
                        }
                        try
                        {
                            ICredentialStore csObj = CredentialStoreFactory.CreateCredentialStore();
                            //from here
                            int count = 0;
                            if (flagUserName == false)
                            {
                                username = System.Environment.GetEnvironmentVariable("VI_Username");
                                optsEntered["username"] = username;
                                if (username == null)
                                {
                                    IEnumerable userNameSet = csObj.GetUsernames(host);
                                    IEnumerator userNameIt = userNameSet.GetEnumerator();

                                    while (userNameIt.MoveNext())
                                    {
                                        count++;
                                        username = userNameIt.Current.ToString();
                                        optsEntered["username"] = username;
                                        username = get_option("username");
                                    }
                                }
                                if (count > 1)
                                {
                                    username = readUserName();
                                    optsEntered["username"] = username;
                                    username = get_option("username");
                                }
                                flagUserName = true;
                            }
                            else
                            {
                                username = get_option("username");
                            }
                            // to here
                            char[] data = csObj.GetPassword(host, username);
                            password = new String(data);
                            optsEntered["password"] = password;
                        }
                        catch (Exception e)
                        {
                            // Do Noting
                            // Not able to find the password in Cred Store
                        }

                    }
                    else
                    {
                        optsEntered["password"] = password;
                    }
                    if (flagUserName == false)
                    {
                        username = System.Environment.GetEnvironmentVariable("VI_Username");
                        if (username == null)
                        {
                            // Read It From The Console
                            username = readUserName();
                        }
                        optsEntered["username"] = username;
                    }
                    if (password == null || password == "")
                    {
                        password = readPassword();
                        optsEntered["password"] = password;
                    }
                }
            }
        }

        /*
        *taking out value of a particular key in the Hashtable
        *i.e checking for required =1 options
        */
        private ArrayList getValue(Hashtable checkOptions)
        {
            IEnumerator It = checkOptions.Keys.GetEnumerator();
            ArrayList vec = new ArrayList();
            while (It.MoveNext())
            {
                String str = It.Current.ToString();
                OptionSpec oSpec = (OptionSpec)checkOptions[str];
                if (oSpec.getOptionRequired() == 1)
                {
                    vec.Add(str);
                }
            }
            return vec;
        }
        public void displayUsage()
        {
            Console.WriteLine("Common .Net Options :");
            print_options(builtInOpts);
            Console.WriteLine("\nCommand specific options: ");
            print_options(userOpts);
            Console.Write("Press Enter Key To Exit: ");
            Console.ReadLine();
            Environment.Exit(1);
        }
        private void print_options(Hashtable Opts)
        {
            String type = "";
            String defaultVal = "";
            IEnumerator It;
            String help = "";
            ICollection generalKeys = (ICollection)Opts.Keys;
            It = generalKeys.GetEnumerator();
            while (It.MoveNext())
            {
                String keyValue = It.Current.ToString();
                OptionSpec oSpec = (OptionSpec)Opts[keyValue];
                if ((oSpec.getOptionType() != null) && (oSpec.getOptionDefault() != null))
                {
                    type = oSpec.getOptionType();
                    defaultVal = oSpec.getOptionDefault();
                    Console.WriteLine("   --" + keyValue + " < type " + type + ", default " + defaultVal + ">");
                }
                if ((oSpec.getOptionDefault() != null) && (oSpec.getOptionType() == null))
                {
                    defaultVal = oSpec.getOptionDefault();
                    Console.WriteLine("   --" + keyValue + " < default " + defaultVal + " >");
                }
                else if ((oSpec.getOptionType() != null) && (oSpec.getOptionDefault() == null))
                {
                    type = oSpec.getOptionType();
                    Console.WriteLine("   --" + keyValue + " < type " + type + " >");
                }
                else if ((oSpec.getOptionType() == null) && (oSpec.getOptionDefault() == null))
                {
                    Console.WriteLine("   --" + keyValue + " ");
                }
                help = oSpec.getOptionDesc();
                Console.WriteLine("      " + help);
            }
        }

        public Boolean option_is_set(String option)
        {
            Boolean valid = false;
            IEnumerator It = optsEntered.Keys.GetEnumerator();
            while (It.MoveNext())
            {
                String keyVal = It.Current.ToString();
                if (option.Equals(keyVal))
                {
                    valid = true;
                }
            }
            return valid;
        }

        public String get_option(String key)
        {
            if (optsEntered.ContainsKey(key))
            {
                foreach (DictionaryEntry uniEntry in optsEntered)
                {
                    if (uniEntry.Key.Equals(key) && uniEntry.Value != null)
                    {
                        return uniEntry.Value.ToString();
                    }
                    else if (uniEntry.Key.Equals(key) && uniEntry.Value == null)
                    {
                        throw new ArgumentHandlingException("Missing Value for Arguement: " + key);
                    }
                }
            }
            else if (checkInputOptions(builtInOpts, key))
            {
                IEnumerator It = builtInOpts.Keys.GetEnumerator();
                while (It.MoveNext())
                {
                    String strC = It.Current.ToString();
                    if (strC.Equals(key))
                    {
                        OptionSpec oSpec = (OptionSpec)builtInOpts[strC];
                        if (oSpec.getOptionDefault() != null)
                        {
                            String str = oSpec.getOptionDefault();
                            return str;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            else if (checkInputOptions(userOpts, key))
            {
                IEnumerator It = userOpts.Keys.GetEnumerator();
                while (It.MoveNext())
                {
                    String strC = It.Current.ToString();
                    if (strC.Equals(key))
                    {
                        OptionSpec oSpec = (OptionSpec)userOpts[strC];
                        if (oSpec.getOptionDefault() != null)
                        {
                            String str = oSpec.getOptionDefault();
                            return str;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("undefined variable");
            }
            return null;
        }

        public void connect()
        {
            log.LogLine("Started ");
            try
            {
                initConnection();
                getServiceUtil().ClientConnect();
            }
            catch (Exception e)
            {
                log.LogLine("Exception running : " + getAppName());
                getUtil().LogException(e);
                log.Close();
                throw e;
            }
        }
        public void connect(Cookie cookie)
        {
            log.LogLine("Started ");
            try
            {
                initConnection();
                getServiceUtil().ClientConnect(cookie);
            }
            catch (Exception e)
            {
                log.LogLine("Exception running : " + getAppName());
                getUtil().LogException(e);
                log.Close();
                throw new ArgumentHandlingException("Exception running : "
                                                                       + getAppName());
            }
        }
        public void loadSession()
        {
            initConnection();
            getServiceUtil().ClientLoadSession();
        }

        public void saveSession(String fileName)
        {
            _svcUtil.ClientSaveSession(fileName);
        }
        public void disConnect()
        {
            try
            {
                getServiceUtil().ClientDisconnect();
            }
            catch (Exception e)
            {
                log.LogLine("Exception running : " + getAppName());
                getUtil().LogException(e);
                log.Close();
                throw new ArgumentException("Exception running : " + getAppName());
            }
        }

        /**
        * @return name of the client application
        */
        public String getAppName()
        {
            return _cname;
        }

        /**
        * @return current log
        */
        public Log getLog()
        {
            return log;
        }

        /**
        * @return the service connection object
        */
        public SvcConnection getConnection()
        {
            return _connection;
        }

        /**
        * @return Client Util object
        */
        public ClientUtil getUtil()
        {
            return _util;
        }

        /**
        * @return Service Util object
        */
        public ServiceUtil getServiceUtil()
        {
            return _svcUtil;
        }

        /**
        * @return web service url
        */
        public String getServiceUrl()
        {
            return get_option("url");
        }

        /**
          * @return web service url
          */
        public String getSsoServiceUrl()
        {
            return get_option("ssoUrl");
        }

        /**
        * @return web service username
        */
        public String getUsername()
        {
            return get_option("username");
        }

        /**
        * @return web service password
        */
        public String getPassword()
        {
            return get_option("password");
        }

        /**
        * @return if SSO is disabled
        */
        public Boolean isSSODisabled()
        {
            return optsEntered.ContainsKey("disablesso");
        }

        /// <summary>
        /// Returns the Hostname or IP of the url provided
        /// </summary>
        /// <returns></returns>
        public string getHostName()
        {
            return new UriBuilder(getServiceUrl()).Host;
        }

        private String readUserName()
        {
            Console.Write("Enter Username : ");
            String username = Console.ReadLine();
            return username;
        }

        private String readPassword()
        {
            char[] data = new Char[50];
            Console.Write("Enter Password : ");
            ConsoleKeyInfo key = Console.ReadKey(true);
            int i = 0;
            while (key.KeyChar != '\r')
            {
                data[i] = key.KeyChar;
                key = Console.ReadKey(true);
                i++;
            }
            return new String(data, 0, i); ;
        }

    }
}
