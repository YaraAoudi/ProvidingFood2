using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProvidingFood2.Model
{
	
		public class Admin
		{
			[Key]
			public int AdminId { get; set; }

			[Required]
			public int UserId { get; set; }

			[ForeignKey("UserId")]
			public virtual User User { get; set; }

			[MaxLength(50)]
			public string Department { get; set; }

			public DateTime? LastLogin { get; set; }

			public bool IsSuperAdmin { get; set; } = false;
		
	}
}
