using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;
using System.Data.SqlClient;

namespace ProvidingFood2.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BeneficiaryController : ControllerBase
	{
		private readonly IBeneficiaryRepository _repo;

		public BeneficiaryController(IBeneficiaryRepository repo)
		{
			_repo = repo;
		}

		[HttpPost]
		public async Task<IActionResult> AddBeneficiary([FromBody] Beneficiary beneficiary)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				var id = await _repo.AddBeneficiaryAsync(beneficiary);
				return Created($"/api/beneficiary/{id}", new { Id = id });
			}
			catch (SqlException ex) when (ex.Number == 2627) // تكرار مفتاح
			{
				return Conflict(new { Error = "المستفيد مسجل مسبقاً" });
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetBeneficiaries()
		{
			var restaurants = await _repo.GetAllBeneficiariesAsync();
			return Ok(restaurants);
		}
		[HttpPut("update")]
		public async Task<IActionResult> UpdateBeneficiary([FromBody] Beneficiary beneficiary)
		{
			if (beneficiary == null || beneficiary.BeneficiaryId == 0)
			{
				return BadRequest(new
				{
					Success = false,
					Message = "يجب توفير بيانات المستفيد ومعرّف صالح"
				});
			}

			try
			{
				bool result = await _repo.UpdateBeneficiaryAsync(beneficiary);

				if (!result)
				{
					return NotFound(new
					{
						Success = false,
						Message = "المستفيد غير موجود أو لم يتم تعديل أي بيانات"
					});
				}

				return Ok(new
				{
					Success = true,
					Message = "تم تحديث بيانات المستفيد بنجاح"
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					Success = false,
					Message = "حدث خطأ أثناء محاولة تحديث بيانات المستفيد",
					Error = ex.Message
				});
			}
		}
			[HttpDelete("delete/{BeneficiaryId}")]
		public async Task<IActionResult> DeleteBeneficiary(int BeneficiaryId)
		{
			try
			{
				bool isDeleted = await _repo.DeleteBeneficiariesUserAsync(BeneficiaryId);

				if (isDeleted)
				{
					return Ok(new
					{
						Success = true,
						Message = "تم حذف المستفيد بنجاح"
					});
				}
				else
				{
					return NotFound(new
					{
						Success = false,
						Message = "لم يتم العثور على المستفيد"
					});
				}
			}
			catch (System.Exception ex)
			{



				return StatusCode(500, new
				{
					Success = false,
					Message = "حدث خطأ أثناء محاولة حذف المستفيد",
					Error = ex.Message
				});
			}
		}


	}
}
