using Domain.ImportData;
using Domain.Response;

namespace Application.Interfaces.ImportData
{
    public interface IImportDataDetailService
    {
        /// <summary>
        /// Add Import Data Detail
        /// </summary>
        /// <param name="importDataDetails"></param>
        /// <returns></returns>
        Task<Response<ImportDataDetails>> AddImportDataDetailAsync(ImportDataDetails importDataDetails);
    }
}
