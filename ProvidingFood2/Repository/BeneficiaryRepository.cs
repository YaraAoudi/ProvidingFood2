using Dapper;
using ProvidingFood2.Model;
using System.Data.SqlClient;

namespace ProvidingFood2.Repository
{
	public class BeneficiaryRepository : IBeneficiaryRepository
	{

		private readonly string _connectionString;

		public BeneficiaryRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public async Task<int> AddBeneficiaryAsync(Beneficiary beneficiary)
		{
			await using var connection = new SqlConnection(_connectionString);

			const string sql = @"
                INSERT INTO Beneficiaries 
                    (FullName, PhoneNumber, FamilySize, IsActive)
                OUTPUT INSERTED.BeneficiaryId
                VALUES 
                    (@FullName, @PhoneNumber, @FamilySize, @IsActive)";

			return await connection.ExecuteScalarAsync<int>(sql, beneficiary);
		}

		public async Task<IEnumerable<Beneficiary>> GetAllBeneficiariesAsync()
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				string query = "SELECT * FROM [Beneficiaries]";
				return await connection.QueryAsync<Beneficiary>(query);
			}
		}

		private string KeepOldIfEmpty(string newValue, string oldValue)
		{
			return string.IsNullOrWhiteSpace(newValue) ? oldValue : newValue;
		}
		public async Task<bool> UpdateBeneficiaryAsync(Beneficiary newBeneficiary)
		{
			using var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();

			using var transaction = connection.BeginTransaction();

			try
			{
				// جلب السجل القديم للتحقق أو للإبقاء على البيانات القديمة إن لم يتم تمرير الجديدة
				var existing = await connection.QuerySingleOrDefaultAsync<Beneficiary>(
					"SELECT * FROM Beneficiaries WHERE BeneficiaryId = @BeneficiaryId",
					new { newBeneficiary.BeneficiaryId }, transaction);

				if (existing == null)
					return false;

				// إبقاء القيم القديمة إذا لم تُمرر قيم جديدة
				newBeneficiary.FullName = KeepOldIfEmpty(newBeneficiary.FullName, existing.FullName);
				newBeneficiary.PhoneNumber = KeepOldIfEmpty(newBeneficiary.PhoneNumber, existing.PhoneNumber);
				newBeneficiary.FamilySize = newBeneficiary.FamilySize == 0 ? existing.FamilySize : newBeneficiary.FamilySize;
				newBeneficiary.IsActive = newBeneficiary.IsActive;

				
				string updateQuery = @"
UPDATE Beneficiaries SET
	FullName = @FullName,
	PhoneNumber = @PhoneNumber,
	FamilySize = @FamilySize,
	IsActive = @IsActive
WHERE BeneficiaryId = @BeneficiaryId";

				int affectedRows = await connection.ExecuteAsync(updateQuery, newBeneficiary, transaction);
				transaction.Commit();
				return affectedRows > 0;
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}

		public async Task<bool> DeleteBeneficiariesUserAsync(int BeneficiaryId)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();


				using (var transaction = connection.BeginTransaction())
				{
					try
					{



						string deleteUserQuery = @"DELETE FROM [Beneficiaries] 
                                         WHERE BeneficiaryId = @BeneficiaryId";

						int BeneficiaryRows = await connection.ExecuteAsync(
							deleteUserQuery,
							new { BeneficiaryId = BeneficiaryId },
							transaction);


						transaction.Commit();


						return (BeneficiaryRows > 0);
					}
					catch
					{

						transaction.Rollback();
						throw;
					}
				}
			}
		}

	}
}
