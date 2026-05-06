using Application.Interfaces.ImportData;
using Domain.ImportData;
using Domain.Login;
using Domain.Response;
using Domain.User;
using Persistance.CommonFunctions;
using Persistance.Interfaces.ImportData;
using Persistance.Services.User;
using System.Net;
using static Dapper.SqlMapper;

namespace Application.Services.ImportData
{
    public class ImportDataDetailService(IImportDataDetailRepository importDataDetailRepository) : IImportDataDetailService
    {
        private readonly IImportDataDetailRepository importDataDetailRepository = importDataDetailRepository;

        /// <summary>
        /// Add Import Data Detail
        /// </summary>
        /// <param name="importDataDetails"></param>
        /// <returns></returns>
        public async Task<Response<ImportDataDetails>> AddImportDataDetailAsync(ImportDataDetails importDataDetails)
        {
            var res = await this.importDataDetailRepository.Insert(importDataDetails);
            res.Message = "File Uploaded Successfully.";
            return res;
        }
    }
}
