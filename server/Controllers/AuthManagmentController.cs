using app.Configuration;
using app.Model;
using app.Model.DBase;
using app.Model.DTOs.Requests;
using app.Model.DTOs.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace app.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthManagmentController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtConfig _jwtConfig;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly AppDbContext _context;

        public AuthManagmentController(
            UserManager<ApplicationUser> userManager,
            IOptionsMonitor<JwtConfig> optionsMonitor,
            TokenValidationParameters tokenValidationParameters,
            AppDbContext context)
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
            _tokenValidationParameters = tokenValidationParameters;
            _context = context;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistationDto user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);

                if (existingUser != null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Error = new List<string>() { "Email already in use" },
                        Success = false
                    });
                }

                var User = new ApplicationUser() { Email = user.Email, UserName = user.UserName };
                var isCreated = await _userManager.CreateAsync(User, user.Password);

                if (isCreated.Succeeded)
                {
                    var jwtToken = await GenerateJwtToken(User);

                    return Ok(jwtToken);
                }
                else 
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Error = isCreated.Errors.Select(a => a.Description).ToList(),
                        Success = false
                    });
                }
            }

            return BadRequest(new RegistrationResponse()
            {
                Error = new List<string>() { "Invalid payload" },
                Success = false
            });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByNameAsync(user.UserName);
                //var existingUser = await _userManager.FindByEmailAsync(user.Email);

                if (existingUser == null) 
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Error = new List<string>() { "Invalid login request" },
                        Success = false
                    });
                }

                var isCorrect = await _userManager.CheckPasswordAsync(existingUser, user.Password);

                if (!isCorrect) 
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Error = new List<string>() { "Invalid login request" },
                        Success = false
                    });
                }

                var jwtToken = await GenerateJwtToken(existingUser);

                return Ok(jwtToken);
            }

            return BadRequest(new RegistrationResponse()
            {
                Error = new List<string>() { "Invalid payload" },
                Success = false
            });
        }

        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (ModelState.IsValid)
            {
                var result = await VerifyAndGenerateToken(tokenRequest);

                if (result == null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Error = new List<string>() { "Invalid tokens" },
                        Success = false
                    });
                }

                return Ok(result);
            }

            return BadRequest(new RegistrationResponse()
            {
                Error = new List<string>() { "Invalid payload" },
                Success = false
            });
        }

        private async Task<AuthResult> GenerateJwtToken(ApplicationUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            var role = await _userManager.GetRolesAsync(user);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { 
                    new Claim("Id", user.Id),
                    new Claim(ClaimTypes.Role, role.FirstOrDefault()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.Now.AddSeconds(300), // 5-10 minutes
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);
            
            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsUsed = false,
                IsRevorked = false,
                UserId = user.Id,
                AddedDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddMonths(6),
                Token = RandomString(35) + Guid.NewGuid()
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResult() { 
                Token = jwtToken,
                Success = true,
                RefreshToken = refreshToken.Token,
                Role = role.FirstOrDefault(),
                Error = new List<string>() { "Token refreshed" }
            };
        }

        private async Task<AuthResult> VerifyAndGenerateToken(TokenRequest tokenRequest)
        {
            //Validation 1 - Validate JWT token foramt
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            try 
            {
                var tokenInVerification = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validatedToken);

                //Validation 2 - Validate encryption alg
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (result == false) {
                        return null;
                    }
                }

                //Validation 3 - Validate expiry date
                var utcExpiryDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);

                if (expiryDate > DateTime.Now) {
                    return new AuthResult()
                    {
                        Success = false,
                        IsLife = true,
                        Error = new List<string>() { "Token has not yet expired" }
                    };
                }

                //Validation 4 - Validate existance
                var storeToken = await _context.RefreshTokens.FirstOrDefaultAsync(a => a.Token == tokenRequest.RefreshToken);

                if (storeToken == null)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Error = new List<string>() { "Token does not exist" }
                    };
                }

                //Validation 5 - Validate if used
                if (storeToken.IsUsed)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Error = new List<string>() { "Token has been used" }
                    };
                }

                //Validation 6 - Validate if revoked
                if (storeToken.IsRevorked)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Error = new List<string>() { "Token has been revoked" }
                    };
                }

                //Validation 7 - Validate the id
                var jti = tokenInVerification.Claims.FirstOrDefault(a => a.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storeToken.JwtId != jti)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Error = new List<string>() { "Token doesn't match" }
                    };
                }

                //Update current token
                storeToken.IsUsed = true;
                _context.RefreshTokens.Update(storeToken);
                await _context.SaveChangesAsync();

                //Generate a new token
                var dbUser = await _userManager.FindByIdAsync(storeToken.UserId);
                return await GenerateJwtToken(dbUser);
            }
            catch (Exception ex)
            {
                return new AuthResult()
                { 
                    Success = false,
                    Error = new List<string>() { "Refresh error", ex.Message.ToString() }
                };
            }
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTimeVal;
        }

        public string RandomString(int length) 
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(x => x[random.Next(x.Length)]).ToArray());
        }
    }
}
