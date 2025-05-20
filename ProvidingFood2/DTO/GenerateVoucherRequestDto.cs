namespace ProvidingFood2.DTO
{
	public class GenerateVoucherRequestDto
	{

		public string BeneficiaryId { get; set; }
		public string MealType { get; set; }
		public int ValidHours { get; set; } = 24;
	}
}
