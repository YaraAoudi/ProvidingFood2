using Dapper;
using ProvidingFood2.DTO;
using System.Data.SqlClient;

namespace ProvidingFood2.Repository
{
	public class FoodBondRepository : IFoodBondRepository
	{
		private readonly string _connectionString;

		public FoodBondRepository(string connectionString)
		{
			_connectionString = connectionString;
		}


		///////////////////////////////////Function for Scan QRCode/////////////////////
		public async Task<QRScanResult> ScanQRCodeAsync(string qrCode)
		{
			using var connection = new SqlConnection(_connectionString);

			const string sql = @"
    SELECT 
        b.BondId,
        b.NumberOfMeals,
        b.ExpiryDate,
        s.StatusName AS Status,
        ben.FullName AS BeneficiaryName,
        r.RestaurantName AS RestaurantName
    FROM FoodBonds b
    JOIN FoodBondStatus s ON b.StatusId = s.StatusId
    JOIN Beneficiaries ben ON b.BeneficiaryId = ben.BeneficiaryId
    JOIN Restaurant r ON b.RestaurantId = r.RestaurantId
    WHERE b.QRCode = @QRCode";

			var result = await connection.QueryFirstOrDefaultAsync<QRScanResult>(sql, new { QRCode = qrCode });

			if (result == null)
				throw new KeyNotFoundException("QR Code غير صالح");

			return result;
		}
		///////////////////////////////////Function for Change bonds status/////////////////////
		public async Task<bool> UpdateBondStatusAsync(int bondId, string newStatus)
		{
			using var connection = new SqlConnection(_connectionString);

			const string sql = @"
                UPDATE FoodBonds
                SET StatusId = (SELECT StatusId FROM FoodBondStatus WHERE StatusName = @Status)
                WHERE BondId = @BondId";

			var affected = await connection.ExecuteAsync(sql,
				new { BondId = bondId, Status = newStatus });

			return affected > 0;
		}
		///////////////////////////////////Function for Validate expir of Bond/////////////////////
		public async Task CheckAndExpireBondsAsync()
		{
			using var connection = new SqlConnection(_connectionString);

			const string sql = @"
                UPDATE FoodBonds
                SET StatusId = (SELECT StatusId FROM FoodBondStatus WHERE StatusName = 'Expired')
                WHERE StatusId = (SELECT StatusId FROM FoodBondStatus WHERE StatusName = 'Pending')
                AND ExpiryDate < GETUTCDATE()";

			await connection.ExecuteAsync(sql);
		}
		///////////////////////////////////Function for Screate Food Bond/////////////////////
		public async Task<int> CreateFoodBondAsync(FoodBondCreateRequest request)
		{
			await using var connection = new SqlConnection(_connectionString);

			var beneficiaryId = await connection.ExecuteScalarAsync<int?>(
				"SELECT BeneficiaryId FROM Beneficiaries WHERE FullName = @Name AND IsActive = 1",
				new { Name = request.BeneficiaryName });

			var restaurantId = await connection.ExecuteScalarAsync<int?>(
				"SELECT RestaurantId FROM Restaurant WHERE RestaurantName = @Name",
				new { Name = request.RestaurantName });

			if (beneficiaryId == null || restaurantId == null)
				throw new KeyNotFoundException("المستفيد أو المطعم غير موجود");

			var qrCode = $"FBOND_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";

			return await connection.ExecuteScalarAsync<int>(
				@"INSERT INTO FoodBonds 
           (BeneficiaryId, RestaurantId, StatusId, QRCode, NumberOfMeals, ExpiryDate)
           OUTPUT INSERTED.BondId
           VALUES (@BeneficiaryId, @RestaurantId, 1, @QRCode, @NumberOfMeals, @ExpiryDate)",
				new
				{
					BeneficiaryId = beneficiaryId,
					RestaurantId = restaurantId,
					QRCode = qrCode,
					request.NumberOfMeals,
					request.ExpiryDate
				});
		}

		///////////////////////////////////Function for get all Food Bond/////////////////////
		public async Task<FoodBondResponse?> GetFoodBondByIdAsync(int id)
		{
			await using var connection = new SqlConnection(_connectionString);

			var sql = @"
            SELECT 
    fb.BondId AS Id,
    b.FullName AS BeneficiaryName,
    r.RestaurantName AS RestaurantName,
    fb.NumberOfMeals,
    fb.CreatedAt,
    fb.QRCode,
    fb.ExpiryDate,
    s.StatusName
FROM FoodBonds fb
INNER JOIN Beneficiaries b ON fb.BeneficiaryId = b.BeneficiaryId
INNER JOIN Restaurant r ON fb.RestaurantId = r.RestaurantId
INNER JOIN FoodBondStatus s ON fb.StatusId = s.StatusId
WHERE fb.BondId = @Id;
";

			var result = await connection.QuerySingleOrDefaultAsync<FoodBondResponse>(sql, new { Id = id });

			return result;
		}
		public async Task<IEnumerable<FoodBondResponse>> GetAllFoodBondsAsync()
		{
			await using var connection = new SqlConnection(_connectionString);

			var sql = @"
		SELECT 
			fb.BondId AS Id,
			b.FullName AS BeneficiaryName,
			r.RestaurantName AS RestaurantName,
			fb.NumberOfMeals,
			fb.CreatedAt,
			fb.QRCode,
			fb.ExpiryDate,
			s.StatusName
		FROM FoodBonds fb
		INNER JOIN Beneficiaries b ON fb.BeneficiaryId = b.BeneficiaryId
		INNER JOIN Restaurant r ON fb.RestaurantId = r.RestaurantId
		INNER JOIN FoodBondStatus s ON fb.StatusId = s.StatusId;
	";

			var results = await connection.QueryAsync<FoodBondResponse>(sql);

			return results;
		}

	}
}

