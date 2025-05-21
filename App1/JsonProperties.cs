namespace JsonProperties
{
    public class Properties
    {
        public string id { get; set; }
        public string identifier { get; set; }
        public bool mdi { get; set; }
        public bool resizable { get; set; }
        public bool maximize { get; set; }
        public bool minimize { get; set; }
        public string title { get; set; }
        public string icon { get; set; }
        public Size size { get; set; }
        public Position position { get; set; }

    }
    public class Position
    {
        public string my { get; set; }
        public string at { get; set; }
    }

    public class Size
    {
        public string width { get; set; }
        public string height { get; set; }
    }
}
