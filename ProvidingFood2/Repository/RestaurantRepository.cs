using Dapper;
using ProvidingFood2.Model;
using System.Data.SqlClient;

namespace ProvidingFood2.Repository
{
	public class RestaurantRepository:IRestaurantRepository
	{
		private readonly string _connectionString;

		public RestaurantRepository (IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("DefaultConnection");
		}
		
		/////////////////function to return all reasturant in Restaurant Table///////////////////////////
		public async Task<IEnumerable<Restaurant>> GetRestaurantAsync()
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				string query = "SELECT * FROM [Restaurant]";
				return await connection.QueryAsync<Restaurant>(query);
			}
		}

		///////////////////function to Add all User in User Table/////////////////////////////
		private async Task<int> AddBaseUserAsync(User user)
		{
			// التحقق من وجود UserTypeId في جدول UserType
			using (var connection = new SqlConnection(_connectionString))   
			{
				await connection.OpenAsync();

				// التحقق من صحة UserTypeId
				string checkUserTypeQuery = "SELECT COUNT(1) FROM UserType WHERE UserTypeId = @UserTypeId";
				var userTypeExists = await connection.ExecuteScalarAsync<bool>(checkUserTypeQuery, new { user.UserTypeId });

				if (!userTypeExists)
				{
					throw new ArgumentException("UserTypeId غير موجود في جدول UserType");
				}

				// إدخال المستخدم بدون Address
				string query = @"INSERT INTO [User] 
                         (fullName, email, password, phoneNumber, UserTypeId)
                         VALUES (@FullName, @Email, @Password, @PhoneNumber, @UserTypeId);
                         SELECT CAST(SCOPE_IDENTITY() as int);";

				var parameters = new
				{
					user.FullName,
					user.Email,
					user.Password,
					user.PhoneNumber,
					user.UserTypeId
				};

				return await connection.ExecuteScalarAsync<int>(query, parameters);
			}
		}


		///////////////////function to Add all restaurant in Restaurant Table/////////////////////////////
		public async Task<bool> AddRestaurantUserAsync(User user, Restaurant restaurant)
		{
			
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				
				string checkCategoryQuery = "SELECT COUNT(1) FROM Category WHERE CategoryId = @CategoryId";
				var categoryExists = await connection.ExecuteScalarAsync<bool>(checkCategoryQuery, new { restaurant.CategoryId });

				if (!categoryExists)
				{
					throw new ArgumentException("CategoryId غير موجود في جدول Category");
				}
			}

			int userId = await AddBaseUserAsync(user);

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				string query = @"INSERT INTO [Restaurant] 
                        (UserId, RestaurantName,RestaurantEmail , RestaurantPhone, Address, CategoryId)
                        VALUES (@UserId, @RestaurantName, @RestaurantEmail, @RestaurantPhone, @RestaurantAddress, @CategoryId);";

				restaurant.UserId = userId;
				int rows = await connection.ExecuteAsync(query, restaurant);
				return rows > 0;
			}
		}


		///////////////////function Delet restaurant from Restaurant Table/////////////////////////////
		public async Task<bool> DeleteRestaurantUserAsync(int userId)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				
				using (var transaction = connection.BeginTransaction())
				{
					try
					{
						
						string deleteRestaurantQuery = @"DELETE FROM [Restaurant] 
                                              WHERE UserId = @UserId";

						int restaurantRows = await connection.ExecuteAsync(
							deleteRestaurantQuery,
							new { UserId = userId },
							transaction);

						
						string deleteUserQuery = @"DELETE FROM [User] 
                                         WHERE UserId = @UserId";

						int userRows = await connection.ExecuteAsync(
							deleteUserQuery,
							new { UserId = userId },
							transaction);

						
						transaction.Commit();

						
						return (restaurantRows > 0 || userRows > 0);
					}
					catch
					{
						
						transaction.Rollback();
						throw;
					}
				}
			}
		}

		///////////////////function for Update field in Restaurant and UserTable/////////////////////////////

		private string KeepOldIfEmpty(string newValue, string oldValue)
		{
			return string.IsNullOrWhiteSpace(newValue) ? oldValue : newValue;
		}

		public async Task<bool> UpdateRestaurantUserAsync(User newUser, Restaurant newRestaurant)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				using (var transaction = connection.BeginTransaction())
				{
					try
					{
						// 1. قراءة القيم القديمة من الجدول
						var existingUser = await connection.QuerySingleOrDefaultAsync<User>(
							"SELECT * FROM [User] WHERE UserId = @UserId",
							new { UserId = newUser.UserId }, transaction);

						var existingRestaurant = await connection.QuerySingleOrDefaultAsync<Restaurant>(
							"SELECT * FROM [Restaurant] WHERE UserId = @UserId",
							new { UserId = newUser.UserId }, transaction);

						if (existingUser == null || existingRestaurant == null)
							return false;

						// 2. دمج القيم: الجديدة إذا موجودة، وإلا نستخدم القديمة
						newUser.FullName = KeepOldIfEmpty(newUser.FullName, existingUser.FullName);
						newUser.Email = KeepOldIfEmpty(newUser.Email, existingUser.Email);
						newUser.PhoneNumber = KeepOldIfEmpty(newUser.PhoneNumber, existingUser.PhoneNumber);
						newUser.Password = KeepOldIfEmpty(newUser.Password, existingUser.Password);

						newRestaurant.RestaurantName = KeepOldIfEmpty(newRestaurant.RestaurantName, existingRestaurant.RestaurantName);
						newRestaurant.RestaurantEmail = KeepOldIfEmpty(newRestaurant.RestaurantEmail, existingRestaurant.RestaurantEmail);
						newRestaurant.RestaurantPhone = KeepOldIfEmpty(newRestaurant.RestaurantPhone, existingRestaurant.RestaurantPhone);
						newRestaurant.RestaurantAddress = KeepOldIfEmpty(newRestaurant.RestaurantAddress, existingRestaurant.RestaurantAddress);

						// 3. تنفيذ التحديث باستخدام الحقول المدموجة
						string updateUserQuery = @"UPDATE [User] SET 
						FullName = @FullName, 
						Email = @Email, 
						PhoneNumber = @PhoneNumber, 
						Password = @Password
					WHERE UserId = @UserId";

						int userRows = await connection.ExecuteAsync(updateUserQuery, newUser, transaction);

						string updateRestaurantQuery = @"UPDATE [Restaurant] SET 
						RestaurantName = @RestaurantName, 
						RestaurantEmail = @RestaurantEmail, 
						RestaurantPhone = @RestaurantPhone, 
						Address = @RestaurantAddress
					WHERE UserId = @UserId";

						newRestaurant.UserId = newUser.UserId;
						int restRows = await connection.ExecuteAsync(updateRestaurantQuery, newRestaurant, transaction);

						transaction.Commit();
						return userRows > 0 || restRows > 0;
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
