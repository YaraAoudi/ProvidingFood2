using ProvidingFood2.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;



var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";


builder.Services.AddCors(options =>
{
	options.AddPolicy(name: MyAllowSpecificOrigins,
		policy =>
		{
			policy.AllowAnyOrigin()
				  .AllowAnyMethod()
				  .AllowAnyHeader();
		});
});

builder.Services.AddAuthentication("Bearer")
	.AddJwtBearer("Bearer", options =>
	{
		var jwtSettings = builder.Configuration.GetSection("JwtSettings");
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = jwtSettings["Issuer"],
			ValidAudience = jwtSettings["Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]))
		};
	});

builder.Services.AddAuthorization();



builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<IDonationRestaurantRepository, DonationRestaurantRepository>();
builder.Services.AddScoped<IBeneficiaryRepository>(provider =>
	new BeneficiaryRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IDonationIndividalRepository>(provider =>
	new DonationIndividalRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IFoodBondRepository>(provider =>
	new FoodBondRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHostedService<BondStatusBackgroundService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache(); 
builder.Services.AddSession();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSession();

var app = builder.Build();


	if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseSession();
app.UseAuthentication();



app.Run();
