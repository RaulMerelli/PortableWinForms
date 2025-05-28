namespace System.Windows.Forms
{
    public class QueryAccessibilityHelpEventArgs : EventArgs
    {
        private string helpNamespace;

        private string helpString;

        private string helpKeyword;

        public string HelpNamespace
        {
            get
            {
                return helpNamespace;
            }
            set
            {
                helpNamespace = value;
            }
        }

        public string HelpString
        {
            get
            {
                return helpString;
            }
            set
            {
                helpString = value;
            }
        }

        public string HelpKeyword
        {
            get
            {
                return helpKeyword;
            }
            set
            {
                helpKeyword = value;
            }
        }

        public QueryAccessibilityHelpEventArgs()
        {
        }

        public QueryAccessibilityHelpEventArgs(string helpNamespace, string helpString, string helpKeyword)
        {
            this.helpNamespace = helpNamespace;
            this.helpString = helpString;
            this.helpKeyword = helpKeyword;
        }
    }
}
