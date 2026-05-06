namespace Domain.Base
{
	public class BaseModel
	{
        [Identity]
        public int Id { get; set; }
        public bool IsDeleted { get; set; } = false;
		public bool IsActive { get; set; }=true;
		public int? CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; } = DateTime.UtcNow;   
		public int? ModifiedBy { get;set; }		
		public DateTime? ModifiedDate { get; set; }
	}

    [AttributeUsage(AttributeTargets.Property)]
    public class IdentityAttribute : Attribute
    {
        public bool IsIdentity { get; set; }
        public IdentityAttribute()
        {
            IsIdentity = true;
        }
    }
}
