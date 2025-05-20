using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;

namespace ProvidingFood2.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RestaurantController : ControllerBase
	{
		private readonly IRestaurantRepository _restaurantRepository;

		public RestaurantController(IRestaurantRepository restaurantRepository)
		{
			_restaurantRepository = restaurantRepository;
		}

		[HttpGet]
		public async Task<IActionResult> GetRestaurant()
		{
			var restaurants = await _restaurantRepository.GetRestaurantAsync();
			return Ok(restaurants);
		}


		[HttpPost("restaurant")]
		public async Task<IActionResult> AddRestaurantUser([FromBody] RestaurantUserDto dto)
		{
			try
			{
				var user = new User
				{
					FullName = dto.FullName,
					Email = dto.Email,
					Password = dto.Password,
					PhoneNumber = dto.PhoneNumber,
					UserTypeId = dto.UserTypeId
				};

				var restaurant = new Restaurant
				{
					RestaurantName = dto.RestaurantName,
					RestaurantEmail = dto.RestaurantEmail,
					RestaurantPhone = dto.RestaurantPhone,
					RestaurantAddress = dto.RestaurantAddress,
					CategoryId = dto.CategoryId
				};

				bool result = await _restaurantRepository.AddRestaurantUserAsync(user, restaurant);
				return result ? Ok("تمت الإضافة بنجاح") : StatusCode(500, "فشل في الإضافة");
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}


		[HttpDelete("delete/{userId}")]
		public async Task<IActionResult> DeleteRestaurant(int userId)
		{
			try
			{
				bool isDeleted = await _restaurantRepository.DeleteRestaurantUserAsync(userId);

				if (isDeleted)
				{
					return Ok(new
					{
						Success = true,
						Message = "تم حذف المطعم والمستخدم المرتبط به بنجاح"
					});
				}
				else
				{
					return NotFound(new
					{
						Success = false,
						Message = "لم يتم العثور على المطعم أو المستخدم"
					});
				}
			}
			catch (System.Exception ex)
			{
				// تسجيل الخطأ في نظام التسجيل (Logging)


				return StatusCode(500, new
				{
					Success = false,
					Message = "حدث خطأ أثناء محاولة حذف المطعم",
					Error = ex.Message
				});
			}
		}

		[HttpPut("update")]
		public async Task<IActionResult> UpdateRestaurantUser([FromBody] UpdateRestaurantUserDto dto)
		{
			if (dto == null || dto.User == null || dto.Restaurant == null)
				return BadRequest("البيانات ناقصة");

			try
			{
				// تحويل DTO إلى كائنات User و Restaurant (قد تحتوي فقط على بعض الحقول)
				var user = new User
				{
					UserId = dto.User.UserId,
					FullName = dto.User.FullName,
					Email = dto.User.Email,
					PhoneNumber = dto.User.PhoneNumber,
					Password = dto.User.Password
				};

				var restaurant = new Restaurant
				{
					UserId = dto.Restaurant.UserId,
					RestaurantName = dto.Restaurant.RestaurantName,
					RestaurantEmail = dto.Restaurant.RestaurantEmail,
					RestaurantPhone = dto.Restaurant.RestaurantPhone,
					RestaurantAddress = dto.Restaurant.RestaurantAddress
				};

				bool updated = await _restaurantRepository.UpdateRestaurantUserAsync(user, restaurant);

				if (updated)
					return Ok("تم التحديث بنجاح");
				else
					return NotFound("لم يتم العثور على البيانات أو لم يتم التعديل");
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"حدث خطأ: {ex.Message}");

			}

		}

		}
}