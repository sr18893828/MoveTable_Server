using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoveTable_Server;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 加入 CORS 策略
string MyAllowSpecificOrigins = "AllowAny";
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: MyAllowSpecificOrigins,
        policy => policy.WithOrigins("*").WithMethods("*").WithHeaders("*")
    );
});


#region JWT驗證
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        //發行者驗證
        ValidateIssuer = true,
        // 設置有效的發行者
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        //接收者驗證
        ValidateAudience = true,
        //設置有效的接收者
        ValidAudience = builder.Configuration["Jwt:Audience"],
        //登入時間驗證，預設是true，可寫可不寫
        ValidateLifetime = true,
        //驗證 Token 的簽名金鑰
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

//API都要受到登入的限制
//builder.Services.AddMvc(options =>
//{
//    options.Filters.Add(new AuthorizeFilter());
//});

#endregion

// DataBase Connection String
var MoveTablesConnectionString = builder.Configuration.GetConnectionString("MoveTables");
// Add MoveTablesDbContext
builder.Services.AddDbContext<MoveTablesDbContext>(options => options.UseSqlServer(MoveTablesConnectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 啟用 wwwroot
app.UseStaticFiles();

// 套用自定義 CORS 設定
app.UseCors(MyAllowSpecificOrigins);

//JWT使用
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
