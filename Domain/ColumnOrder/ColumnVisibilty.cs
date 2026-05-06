using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ColumnOrder
{
    public class ColumnVisibilityDTO
    {
        public int Id { get; set; }

        public long UserId { get; set; }

        public string ColumnSettings { get; set; }
    }

    public class SaveColumnRequest
    {
        public Dictionary<string, bool> ColumnSettings { get; set; }
    }
}
