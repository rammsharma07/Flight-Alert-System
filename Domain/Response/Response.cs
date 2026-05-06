using Newtonsoft.Json;
using System.Net;

namespace Domain.Response
{
	public class Response<T>
	{
		[JsonProperty("recordsFiltered")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
		public int recordsFiltered { get; set; }

		[JsonProperty("recordsTotal")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
		public int recordsTotal { get; set; }

		[JsonProperty("status")]
		public HttpStatusCode Status { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("data")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
		public List<T> Data { get; set; }

		[JsonProperty("previousPage")]
		public string previousPage { get; set; }

		[JsonProperty("nextPage")]
		public string nextPage { get; set; }
		public List<int> Extra { get; set; }

		public int TotalRecords { get; set; }
		public int PageNumber { get; set; }

		public int FilteredRecords { get; set; }

		public int entityHistoryId { get; set; }

		public bool status  { get; set; }

	}

}
