using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProvidingFood2.Model
{
	public class QRVoucher
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int VoucherId { get; set; } 
		public string BeneficiaryId { get; set; }
		public string MealType { get; set; }
		public DateTime ExpiryDate { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public bool IsActive { get; set; } = true;
	}
}
