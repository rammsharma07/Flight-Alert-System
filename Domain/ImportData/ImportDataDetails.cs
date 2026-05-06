using Domain.Base;

namespace Domain.ImportData
{
    public class ImportDataDetails : BaseModel
    {
        public string FileName { get; set; }
        public string FileFullPath { get; set; }
    }
}
