using Amazon;
using Amazon.Rekognition;
using Amazon.Runtime;
using AmazonRekognitionTest.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IAmazonRekognition>(_ =>
{
    var user = builder.Configuration.GetSection("User");
    var credentials = new BasicAWSCredentials(user["accessKey"], user["secretKey"]);
    var region = RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"] ?? "eu-central-1");
    return new AmazonRekognitionClient(credentials, region);
});
builder.Services.AddScoped<RekognitionService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
