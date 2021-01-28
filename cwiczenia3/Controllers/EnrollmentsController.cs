using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using cwiczenia3.DAL;
using cwiczenia3.dto.request;
using cwiczenia3.dto.response;
using cwiczenia3.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace cwiczenia3.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentsDbService service = new SqlServerDbService();
        public IConfiguration Configuration { get; set; }
        public EnrollmentsController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [HttpPost]
        [Authorize]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            EnrollStudentResponse response;
            try
            {
                response = service.EnrollStudent(request);
                if (response == null) 
                    return BadRequest("Response is null");
            }
            catch (BadRequestException e) { return BadRequest(e.Message); }
            catch (NotFoundException e) { return NotFound(e.Message); }

            return Created($"api/students/enrollments", response);
        }

        [HttpPost("promotions")]
        [Authorize]
        public IActionResult PromoteStudents(PromotionRequest request)
        {
            PromotionResponse response;
            try
            {
                response = service.PromoteStudents(request);
                if (response == null)
                {
                    return BadRequest($"Studies {request.Studies} , {request.Semester} not found.");
                }
            }
            catch (BadRequestException e) { return BadRequest(e.Message); }
            catch (NotFoundException e) { return NotFound(e.Message); }

            return Created($"api/students", response);
        }

        [HttpPost("authorize")]
        public IActionResult Authorize(AuthorizationRequest request)
        {
            string refTkn = service.GenerateRefreshTokenIfValid(request);
            if (string.IsNullOrEmpty(refTkn))
            {
                return Unauthorized("Wrong login data.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, request.Login)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                issuer: "s15035@pjwstk.edu.pl",
                audience: "osoba",
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials
            );

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                refTkn
            });
        }

        [HttpPost("refresh/{refreshToken}")]
        public IActionResult RefreshJwtToken(string refreshToken)
        {
            RefreshTokenResponse refTknDto = service.GenerateRefreshTokenIfValid(refreshToken);
            if (string.IsNullOrEmpty(refTknDto.refreshToken))
            {
                return Unauthorized("Niepoprawny Refresh Token");
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, refTknDto.studIndex)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                issuer: "s15035@pjwstk.edu.pl",
                audience: "osoba",
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials
            );

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                refTknDto.refreshToken
            });
        }
    }
}
