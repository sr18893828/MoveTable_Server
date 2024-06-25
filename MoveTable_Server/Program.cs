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

// �[�J CORS ����
string MyAllowSpecificOrigins = "AllowAny";
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: MyAllowSpecificOrigins,
        policy => policy.WithOrigins("*").WithMethods("*").WithHeaders("*")
    );
});


#region JWT����
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        //�o�������
        ValidateIssuer = true,
        // �]�m���Ī��o���
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        //����������
        ValidateAudience = true,
        //�]�m���Ī�������
        ValidAudience = builder.Configuration["Jwt:Audience"],
        //�n�J�ɶ����ҡA�w�]�Otrue�A�i�g�i���g
        ValidateLifetime = true,
        //���� Token ��ñ�W���_
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

//API���n����n�J������
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

// �ҥ� wwwroot
app.UseStaticFiles();

// �M�Φ۩w�q CORS �]�w
app.UseCors(MyAllowSpecificOrigins);

//JWT�ϥ�
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
