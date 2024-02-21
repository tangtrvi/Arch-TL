using Arch_TL.DAL.Context.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Arch_TL.API.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IBiologyDomainRepository _biologyDomainRepository;

    public WeatherForecastController(
        ILogger<WeatherForecastController> logger,
        IBiologyDomainRepository biologyDomainRepository,
        IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
        _biologyDomainRepository = biologyDomainRepository;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet("Token")]
    public string GetToken()
    {
        var encryptKey = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["Jwt:SignKey"]));
        string validIssusers = _configuration.GetValue<string>("Jwt:ValidIssuer");
        string validAudience = _configuration.GetValue<string>("Jwt:ValidAudience");
        int expires = _configuration.GetValue<int>("Jwt:ExpiresInMinutes");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = validIssusers,
            Audience = validAudience,
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(expires),
            Subject = new ClaimsIdentity(new List<Claim> {
                    new Claim("role", "user")
                }),
            SigningCredentials = new SigningCredentials(encryptKey, SecurityAlgorithms.HmacSha256Signature)
        };
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = jwtTokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        var token = jwtTokenHandler.WriteToken(jwtToken);
        Console.Write(token);
        return token;
    }

    //[Authorize]
    [AllowAnonymous]
    [HttpGet("Token/Test")]
    public async Task<ActionResult> TestToken(string searchText = null)
    {
        var result = await _biologyDomainRepository.GetPageAsync((1, 20), searchText);
        return Ok(new
        {
            message = "Success",
            result
        });
    }
}
