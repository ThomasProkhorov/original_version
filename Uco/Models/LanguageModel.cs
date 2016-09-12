namespace Uco.Models
{
    public class Language
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Url { get; set; }
        public bool Default { get; set; }
        public string Flag
        {
            get
            {
                if (Code == null) return "~/Content/Flags/none.png";
                if (Code.Length == 5) return "~/Content/Flags/" + Code.Remove(0, 3).ToLower() + ".png";
                else return "~/Content/Flags/" + Code + ".png";
            }
        }

        public Language()
        {
            Name = "";
            Code = "";
            Url = "";
            Default = false;
        }
    }
}