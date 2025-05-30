using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProvidingFood2.Model
{
	public class Beneficiary
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int BeneficiaryId { get; set; }

		[Required]
		[StringLength(100)]
		public string FullName { get; set; }

		[Phone]
		[StringLength(20)]
		public string? PhoneNumber { get; set; }

		[Range(1, 20)]
		public int FamilySize { get; set; } = 1;

		public bool IsActive { get; set; } = true;
	}
}
