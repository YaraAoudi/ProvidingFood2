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


		public async Task<LoginResult> Login(Login login)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				// البحث في جدول User أولاً بالبريد الإلكتروني
				var userQuery = @"
            SELECT u.UserId, u.Password, u.FullName
            FROM [User] u
            WHERE u.Email = @Email";

				var user = await connection.QueryFirstOrDefaultAsync<dynamic>(userQuery, new { login.Email });

				if (user == null || (string)user.Password != login.Password)
				{
					return new LoginResult
					{
						Success = false,
						Message = "البريد الإلكتروني أو كلمة المرور غير صحيحة"
					};
				}

				// البحث في جدول Admin
				var adminQuery = "SELECT * FROM [Admin] WHERE UserId = @UserId";
				var adminInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(adminQuery, new { UserId = user.UserId });

				if (adminInfo != null)
				{
					return new LoginResult
					{
						Success = true,
						UserId = user.UserId,
						FullName = user.FullName,
						UserTypeId = 2, // افترض أن 1 هو معرف نوع المشرف
						Message = "تم تسجيل الدخول بنجاح كمشرف"
					};
				}

				// إذا لم يكن مشرفاً، نبحث في جدول المطاعم
				var restaurantQuery = "SELECT * FROM [Restaurant] WHERE UserId = @UserId";
				var restaurantInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(restaurantQuery, new { UserId = user.UserId });

				if (restaurantInfo != null)
				{
					return new LoginResult
					{
						Success = true,
						UserId = user.UserId,
						FullName = user.FullName,
						Token = user.Token,
						UserTypeId = 1, // افترض أن 2 هو معرف نوع صاحب المطعم
						Message = "تم تسجيل الدخول بنجاح كصاحب مطعم"
					};
				}

				// إذا لم يتم العثور على المستخدم في أي من الجدولين
				return new LoginResult
				{
					Success = false,
					Message = "لم يتم العثور على بيانات المستخدم في النظام"
				};
			}
		}

		///////Function for login depend on UserType




	}
}

