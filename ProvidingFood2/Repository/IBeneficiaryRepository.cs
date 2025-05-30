using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
	public interface IBeneficiaryRepository
	{
		Task<int> AddBeneficiaryAsync(Beneficiary beneficiary);
		Task<IEnumerable<Beneficiary>> GetAllBeneficiariesAsync();
	}
}
