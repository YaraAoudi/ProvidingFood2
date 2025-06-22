using ProvidingFood2.Model;
using Dapper;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;



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

		public async Task<bool> AddAdminUserAsync(User user, string userTypeName, string position)
		{
			// إضافة المستخدم الأساسي والحصول على UserId
			int userId = await AddBaseUserAsync(user, userTypeName);

			await using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				string insertAdminQuery = @"
        INSERT INTO [Admin] 
            (UserId, Position, CreatedAt)
        VALUES 
            (@UserId, @Position, @CreatedAt);";

				var parameters = new
				{
					UserId = userId,
					Position = position,
					CreatedAt = DateTime.UtcNow // أو DateTime.Now حسب توقيتك
				};

				int rows = await connection.ExecuteAsync(insertAdminQuery, parameters);
				return rows > 0;
			}
		}



		public async Task<LoginResult> Login(Login login)
			{
				using var connection = new SqlConnection(_connectionString);
				await connection.OpenAsync();

				var userQuery = @"
SELECT u.UserId, u.Password, u.FullName, u.UserTypeId, ut.TypeName
FROM [User] u
JOIN UserType ut ON u.UserTypeId = ut.UserTypeId
WHERE u.Email = @Email";

				var user = await connection.QueryFirstOrDefaultAsync<dynamic>(userQuery, new { login.Email });

				if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, (string)user.Password))
				{
					return new LoginResult
					{
						Success = false,
						Message = "البريد الإلكتروني أو كلمة المرور غير صحيحة"
					};
				}

				int userId = user.UserId;
				string fullName = user.FullName;
				int userTypeId = user.UserTypeId;
				string role = "Unknown";

				// تحقق نوع المستخدم
				var admin = await connection.QueryFirstOrDefaultAsync("SELECT 1 FROM [Admin] WHERE UserId = @UserId", new { UserId = userId });
				if (admin != null)
					role = "Admin";

				var restaurant = await connection.QueryFirstOrDefaultAsync("SELECT 1 FROM [Restaurant] WHERE UserId = @UserId", new { UserId = userId });
				if (restaurant != null)
					role = "Restaurant";

				if ((string)user.TypeName == "Donor")
					role = "Donor";

				// توليد التوكن
				string token = GenerateJwtToken(userId, fullName, userTypeId, role);

				// نتيجة تسجيل الدخول
				return new LoginResult
				{
					Success = true,
					UserId = userId,
					FullName = fullName,
					UserTypeId = userTypeId,
					Token = token,
					Message = $"تم تسجيل الدخول بنجاح كـ {role}"
				};
			}

			private string GenerateJwtToken(int userId, string fullName, int userTypeId, string role)
			{
				var jwtSettings = _configuration.GetSection("JwtSettings");
				var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
				var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

				var claims = new[]
				{
			new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
			new Claim("fullName", fullName),
			new Claim("userTypeId", userTypeId.ToString()),
			new Claim(ClaimTypes.Role, role),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

				var token = new JwtSecurityToken(
					issuer: jwtSettings["Issuer"],
					audience: jwtSettings["Audience"],
					claims: claims,
					expires: DateTime.UtcNow.AddHours(2),
					signingCredentials: creds
				);

				return new JwtSecurityTokenHandler().WriteToken(token);
			}
		}



	}


