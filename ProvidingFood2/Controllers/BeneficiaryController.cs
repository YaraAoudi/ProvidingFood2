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


	}
}
