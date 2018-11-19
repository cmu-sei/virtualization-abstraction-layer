using System;


namespace AppUtil
{
    public class OptionSpec
    {
        private string optionName;
        private int optionRequired;
        private string optionDesc;
        private string optionType;
        private string optionDefault;

        public OptionSpec(string optionName, string optionType, int optionRequired, string optionDesc, string optionDefault)
        {
            this.optionName = optionName;
            this.optionType = optionType;
            this.optionRequired = optionRequired;
            this.optionDesc = optionDesc;
            this.optionDefault = optionDefault;
        }

        public string getOptionName()
        {
            return optionName;
        }

        public int getOptionRequired()
        {
            return optionRequired;
        }

        public string getOptionDesc()
        {
            return optionDesc;
        }

        public string getOptionType()
        {
            return optionType;
        }

        public string getOptionDefault()
        {
            return optionDefault;
        }
    }
}
