namespace Beta.Platform
{
    public sealed class FileExtension
    {
        public string Extension { get; }

        public FileExtension(string extension)
        {
            this.Extension = extension;
        }

        public override string ToString()
        {
            return $"*.{Extension}";
        }
    }
}
