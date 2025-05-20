using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;

namespace ProvidingFood2.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class QRController : ControllerBase
	{
		private readonly IQRRepository _qrRepository;

		public QRController(IQRRepository qrRepository)
		{
			_qrRepository = qrRepository;
		}

		[HttpPost("generate")]
		public async Task<IActionResult> GenerateVoucher([FromBody] GenerateVoucherRequestDto request)
		{
			var voucher = new QRVoucher
			{
				BeneficiaryId = request.BeneficiaryId,
				MealType = request.MealType,
				ExpiryDate = DateTime.UtcNow.AddHours(request.ValidHours)
			};

			var voucherId = await _qrRepository.CreateVoucherAsync(voucher);

			var response = new QRVoucherResponse
			{
				VoucherData = voucherId.ToString(), // تحويل الرقم إلى string
				ExpiryDate = voucher.ExpiryDate
			};

			return Ok(response);
		}

		// باقي الـ Endpoints تبقى كما هي مع تغيير نوع الـ voucherId إلى int
		[HttpGet("validate/{voucherId}")]
		public async Task<IActionResult> ValidateVoucher(int voucherId) // تغيير هنا
		{
			bool isValid = await _qrRepository.ValidateVoucherAsync(voucherId);
			return Ok(new { IsValid = isValid });
		}

		[HttpPost("revoke/{voucherId}")]
		public async Task<IActionResult> RevokeVoucher(int voucherId) // تغيير هنا
		{
			bool success = await _qrRepository.RevokeVoucherAsync(voucherId);
			return Ok(new { Success = success });
		}

		[HttpGet("details/{voucherId}")]
		public async Task<IActionResult> GetVoucherDetails(int voucherId) // تغيير هنا
		{
			var voucher = await _qrRepository.GetVoucherAsync(voucherId);
			if (voucher == null)
				return NotFound();

			return Ok(voucher);
		}
	}

	
}

