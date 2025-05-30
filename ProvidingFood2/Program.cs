using ProvidingFood2.Repository;



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



app.Run();
