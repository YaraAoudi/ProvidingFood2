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
	}
}
