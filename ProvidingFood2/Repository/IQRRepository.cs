using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
	public interface IQRRepository
	{
		Task<int> CreateVoucherAsync(QRVoucher voucher);
		Task<QRVoucher> GetVoucherAsync(int voucherId);
		Task<bool> ValidateVoucherAsync(int voucherId);
		Task<bool> RevokeVoucherAsync(int voucherId);
	}
}
