namespace System.Windows.Forms
{
    public class LinkLabelLinkClickedEventArgs : EventArgs
    {
        private readonly LinkLabel.Link link;

        private readonly MouseButtons button;

        public MouseButtons Button => button;

        public LinkLabel.Link Link => link;

        public LinkLabelLinkClickedEventArgs(LinkLabel.Link link)
        {
            this.link = link;
            button = MouseButtons.Left;
        }

        public LinkLabelLinkClickedEventArgs(LinkLabel.Link link, MouseButtons button)
            : this(link)
        {
            this.button = button;
        }
    }
}
