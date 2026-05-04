using AuthService.WebApi.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthService.WebApi.Utilities
{
    public class JwtTokenGenerator
    {
        private readonly AuthOptions _options;

        public JwtTokenGenerator(IOptions<AuthOptions> options)
        {
            _options = options.Value;
        }

        public string GenerateServiceToken(string serviceId, List<string> scopes, int expirationMinutes)
        {
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_options.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("sub", serviceId),
                new Claim("type", "service"),
                new Claim("scope", string.Join(" ", scopes)),
                new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim("iss", _options.Issuer),
                new Claim("aud", _options.Audience)
            };

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateUserToken(string userId, string role, string email, int expirationMinutes)
        {
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_options.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("sub", userId),
                new Claim("type", "user"),
                new Claim("role", role),
                new Claim("email", email),
                new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim("iss", _options.Issuer),
                new Claim("aud", _options.Audience)
            };

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
