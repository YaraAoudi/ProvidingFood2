using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;
using System.Security.Claims;

namespace ProvidingFood2.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IUserRepository _userRepository;

		public UserController(IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		[HttpPost("register")]
		public async Task<IActionResult> RegisterDonor ( UserDto request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var user = new User
			{
				FullName = request.FullName,
				Email = request.Email,
				Password = request.Password,
				PhoneNumber = request.PhoneNumber
			};

			int userId = await _userRepository.AddBaseUserAsync(user, "Donor");

			return Ok(new { message = "تم تسجيل المتبرع بنجاح", userId });
		}

		[HttpPost("add-admin")]
		public async Task<IActionResult> AddAdmin([FromBody] AdminCreateDto adminDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var user = new User
			{
				FullName = adminDto.FullName,
				Email = adminDto.Email,
				PhoneNumber = adminDto.PhoneNumber,
				Password = adminDto.Password // تأكد من التشفير إذا كان ذلك مطلوباً
			};

			bool result = await _userRepository.AddAdminUserAsync(user, adminDto.UserTypeName, adminDto.Position);

			if (!result)
			{
				return StatusCode(500, "حدث خطأ أثناء إضافة المدير.");
			}

			return Ok("تمت إضافة المدير بنجاح.");
		}

		[HttpPost("login")]

		public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var loginModel = new Login
				{
					Email = loginDto.Email,
					Password = loginDto.Password
				};

				var result = await _userRepository.Login(loginModel);

				if (!result.Success)
				{
					return Unauthorized(new { message = result.Message });
				}

				return Ok(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "حدث خطأ أثناء معالجة طلبك", error = ex.Message });
			}
		}
		[Authorize(Roles = "Admin")]
		[HttpGet("admin/dashboard")]
		public IActionResult GetAdminDashboard()
		{
			return Ok("أهلاً بك في لوحة تحكم المشرف");
		}

		[Authorize(Roles = "Donor,Restaurant")]
		[HttpGet("user/profile")]
		public IActionResult GetUserProfile()
		{
			var role = User.FindFirst(ClaimTypes.Role)?.Value;
			return Ok($"أنت مستخدم من نوع: {role}");
		}

	}
}
