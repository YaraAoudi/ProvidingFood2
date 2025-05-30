using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using System.Data.SqlClient;
using System.Security.Claims;

namespace ProvidingFood2.Repository
{
	public class DonationRestaurantRepository:IDonationRestaurantRepository
	{
		private readonly string _connectionString;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public DonationRestaurantRepository(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("DefaultConnection");

			

		}

		public async Task<IEnumerable<DonationRequestDto>> GetDonationsAsync()
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				var sql = @"
                SELECT 
                    d.Quantity, 
                    d.DateDonated, 
                    r.RestaurantName
                FROM 
                    Donation d
                INNER JOIN 
                    Restaurant r ON d.RestaurantId = r.RestaurantId
                ORDER BY 
                    d.DateDonated DESC";

				return await connection.QueryAsync<DonationRequestDto>(sql);
			}
		}


		public async Task<bool> AddDonationAsync(string restaurantName, int quantity, DateTime dateDonated)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				var restaurantId = await connection.QueryFirstOrDefaultAsync<int?>(
					"SELECT RestaurantId FROM Restaurant WHERE RestaurantName = @RestaurantName",
					new { RestaurantName = restaurantName });

				if (restaurantId == null)
					throw new KeyNotFoundException("المطعم غير موجود");

				var sql = @"
			INSERT INTO Donation 
				(RestaurantId,Quantity, DateDonated)
			VALUES 
				(@RestaurantId,@Quantity, @DateDonated)";

				var affectedRows = await connection.ExecuteAsync(sql, new
				{
					RestaurantId = restaurantId,
					Quantity = quantity,
					DateDonated = dateDonated
				});

				return affectedRows > 0;
			}
		}


	}
}
