using ProvidingFood2.Model;
using System.Data.SqlClient;
using Dapper;

namespace ProvidingFood2.Repository
{
	public class QRRepository : IQRRepository
	{
		private readonly string _connectionString;

		public QRRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public async Task<int> CreateVoucherAsync(QRVoucher voucher)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				string query = @"
        INSERT INTO QRVouchers 
        (BeneficiaryId, MealType, ExpiryDate, CreatedAt, IsActive)
        VALUES 
        (@BeneficiaryId, @MealType, @ExpiryDate, @CreatedAt, @IsActive);
        
        SELECT SCOPE_IDENTITY();"; // إرجاع آخر ID تم إدراجه

				voucher.VoucherId = await connection.ExecuteScalarAsync<int>(query, voucher);
				return voucher.VoucherId;
			}
		}

		public async Task<QRVoucher> GetVoucherAsync(int voucherId)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				string query = @"
                SELECT VoucherId, BeneficiaryId, MealType, ExpiryDate, CreatedAt, IsActive
                FROM QRVouchers
                WHERE VoucherId = @VoucherId";

				return await connection.QueryFirstOrDefaultAsync<QRVoucher>(query, new { VoucherId = voucherId });
			}
		}

		public async Task<bool> ValidateVoucherAsync(int voucherId)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				string query = @"
                SELECT COUNT(1)
                FROM QRVouchers
                WHERE VoucherId = @VoucherId
                AND IsActive = 1
                AND ExpiryDate > GETUTCDATE()";

				return await connection.ExecuteScalarAsync<bool>(query, new { VoucherId = voucherId });
			}
		}

		public async Task<bool> RevokeVoucherAsync(int voucherId)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				string query = @"
                UPDATE QRVouchers
                SET IsActive = 0
                WHERE VoucherId = @VoucherId";

				int affectedRows = await connection.ExecuteAsync(query, new { VoucherId = voucherId });
				return affectedRows > 0;
			}
		}
	}
}
	
