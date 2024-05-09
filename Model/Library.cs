namespace ProjectAssetReader.Model
{
    public class Library
    {
        public string Sha512 { get; set; }

        public string Type { get; set; }

        public string Path { get; set; }

        public string[] Files { get; set; }

        public bool? HasTools { get; set; }

        public string MsBuildProject { get; set; }
    }
}