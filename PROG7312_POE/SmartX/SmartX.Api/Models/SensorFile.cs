namespace SmartX.Api.Models
{
    public class SensorFile
    {
        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }
    }
}