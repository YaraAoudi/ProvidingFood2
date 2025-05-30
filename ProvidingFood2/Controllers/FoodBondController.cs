﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Repository;

namespace ProvidingFood2.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FoodBondController : ControllerBase
	{
		private readonly IFoodBondRepository _repository;

		public FoodBondController(IFoodBondRepository repository)
		{
			_repository = repository;
		}

		[HttpPost("scan")]
		public async Task<IActionResult> ScanQRCode([FromBody] ScanRequest request)
		{
			try
			{
				var result = await _repository.ScanQRCodeAsync(request.QRCode);

				if (result.Status == "Expired")
					return BadRequest(new { Error = "انتهت صلاحية السند" });

				return Ok(result);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { Error = ex.Message });
			}
		}

		[HttpPost("confirm")]
		public async Task<IActionResult> ConfirmReceipt([FromBody] ConfirmRequest request)
		{
			var success = await _repository.UpdateBondStatusAsync(request.BondId, "Received");
			return success ? Ok() : BadRequest();
		}

		[HttpPost]
		public async Task<IActionResult> CreateFoodBond([FromBody] FoodBondCreateRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var bondId = await _repository.CreateFoodBondAsync(request);

				return CreatedAtAction(
					nameof(GetFoodBond),
					new { id = bondId },
					new
					{
						BondId = bondId,
						QRCode = $"FBOND_{bondId}",
						Message = "تم إنشاء السند بنجاح"
					});
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new
				{
					Error = ex.Message,
					Details = "تأكد من صحة اسم المستفيد والمطعم"
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					Error = "خطأ داخلي",
					Details = ex.Message
				});
			}
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetFoodBond(int id)
		{
			var foodBond = await _repository.GetFoodBondByIdAsync(id);
			return foodBond == null ? NotFound() : Ok(foodBond);
		}

		[HttpGet]
		public async Task<IActionResult> GetAllFoodBonds()
		{
			var bonds = await _repository.GetAllFoodBondsAsync();
			return Ok(bonds);
		}

	}
}
