using Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ColumnOrder
{
    public class ColumnOrderDTO
    {
        [Identity]
        public int Id { get; set; }
        public string TableName { get; set; }
        public string Orders { get; set; }
        public int UserId { get; set; }


    }
}
