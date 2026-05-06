using Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.User
{
    public class UserRole : BaseModel
    {
        public int UserId { get; set; }
      
        public int RoleId { get; set; }
      
    }
}
