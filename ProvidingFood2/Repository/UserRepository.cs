using ProvidingFood2.Model;
using Dapper;
using System.Data.SqlClient;



namespace ProvidingFood2.Repository
{
	public class UserRepository : IUserRepository
	{
		private readonly string _connectionString;

		private readonly IConfiguration _configuration;

		public UserRepository(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("DefaultConnection"); ;
			_configuration = configuration;
		}


		public async Task<int> AddBaseUserAsync(User user, string userTypeName)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				
				string getUserTypeIdQuery = "SELECT UserTypeId FROM UserType WHERE TypeName = @UserTypeName";
				var userTypeId = await connection.ExecuteScalarAsync<int?>(getUserTypeIdQuery, new { UserTypeName = userTypeName });

				if (userTypeId == null)
				{
					throw new ArgumentException("UserTypeName غير موجود في جدول UserType");
				}

				
				string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

				
				string query = @"
		INSERT INTO [User] 
			(FullName, Email, Password, PhoneNumber, UserTypeId)
		VALUES 
			(@FullName, @Email, @Password, @PhoneNumber, @UserTypeId);
		SELECT CAST(SCOPE_IDENTITY() as int);";

				var parameters = new
				{
					user.FullName,
					user.Email,
					Password = hashedPassword,
					user.PhoneNumber,
					UserTypeId = userTypeId
				};

				return await connection.ExecuteScalarAsync<int>(query, parameters);
			}
		}



		public async Task<LoginResult> Login(Login login)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				
				var userQuery = @"
		SELECT u.UserId, u.Password, u.FullName, u.UserTypeId, ut.TypeName
		FROM [User] u
		JOIN UserType ut ON u.UserTypeId = ut.UserTypeId
		WHERE u.Email = @Email";

				var user = await connection.QueryFirstOrDefaultAsync<dynamic>(userQuery, new { login.Email });

				// التحقق من صحة البريد وكلمة المرور
				if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, (string)user.Password))
				{
					return new LoginResult
					{
						Success = false,
						Message = "البريد الإلكتروني أو كلمة المرور غير صحيحة"
					};
				}

				// التحقق إذا كان مشرف
				var adminQuery = "SELECT * FROM [Admin] WHERE UserId = @UserId";
				var adminInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(adminQuery, new { UserId = user.UserId });

				if (adminInfo != null)
				{
					return new LoginResult
					{
						Success = true,
						UserId = user.UserId,
						FullName = user.FullName,
						UserTypeId = 2,
						Message = "تم تسجيل الدخول بنجاح كمشرف"
					};
				}

				// التحقق إذا كان صاحب مطعم
				var restaurantQuery = "SELECT * FROM [Restaurant] WHERE UserId = @UserId";
				var restaurantInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(restaurantQuery, new { UserId = user.UserId });

				if (restaurantInfo != null)
				{
					return new LoginResult
					{
						Success = true,
						UserId = user.UserId,
						FullName = user.FullName,
						UserTypeId = 1,
						Message = "تم تسجيل الدخول بنجاح كصاحب مطعم"
					};
				}

				// التحقق إذا كان متبرع
				if ((string)user.TypeName == "Donor")
				{
					return new LoginResult
					{
						Success = true,
						UserId = user.UserId,
						FullName = user.FullName,
						UserTypeId = user.UserTypeId,
						Message = "تم تسجيل الدخول بنجاح كمتبرع"
					};
				}

				// في حال لم يتم تحديد نوع المستخدم
				return new LoginResult
				{
					Success = false,
					Message = "لم يتم العثور على بيانات المستخدم في النظام"
				};
			}
		}




	}
}

