using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend_tetris.database;
using backend_tetris.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend_tetris.services;

public class JwtGenerator
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public JwtGenerator(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseModel?> Authenticate(LoginRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return null;
        var userAccount = await _context.Users.FirstOrDefaultAsync(x => x.Username == request.Username);
        if (userAccount == null || request.Password != userAccount.PasswordHash)
            return null;
        var issuer = _configuration["JwtConfig:Issuer"];
        var audience = _configuration["JwtConfig:Audience"];
        var key = _configuration["JwtConfig:Key"];
        var tokenValidityMinutes = _configuration.GetValue<int>("JwtConfig:TokenValidityMinutes");
        var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMinutes);
        var tokenDescripter = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Name, userAccount.Username)
            ]),
            Expires = tokenExpiryTimeStamp,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key!)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescripter);
        var accessToken = tokenHandler.WriteToken(securityToken);

        return new LoginResponseModel
        {
            AccessToken = accessToken,
            Username = request.Username,
            ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.UtcNow).TotalSeconds
        };
    }
}