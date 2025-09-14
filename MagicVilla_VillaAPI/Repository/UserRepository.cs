using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private string secretkey;

        public UserRepository(ApplicationDbContext db, IMapper mapper,
            IConfiguration _configuration,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _mapper = mapper;
            secretkey = _configuration.GetValue<string>("ApiSettings:Secret")!;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public bool IsUniqeUser(string userName)
        {
            var user = _db.ApplicationUsers.AsNoTracking().FirstOrDefault(x => x.UserName == userName);
            return user == null;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = await _db.ApplicationUsers.AsNoTracking().FirstOrDefaultAsync(x => x.UserName!.ToLower() == loginRequestDTO.UserName.ToLower());
            if (user == null)
            {
                return new LoginResponseDTO
                {
                    Token = "",
                    User = null
                };
            }
            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);

            if(!isValid)
            {
                return new LoginResponseDTO
                {
                    Token = "",
                    User = null
                };
            }
            var roles = await _userManager.GetRolesAsync(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretkey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Email!),
                    new Claim(ClaimTypes.Role, roles!.FirstOrDefault()!)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            LoginResponseDTO loginResponseDTO = new LoginResponseDTO
            {
                Token = tokenHandler.WriteToken(token),
                User = _mapper.Map<UserDTO>(user),
            };
            return loginResponseDTO;
        }

        public async Task<UserDTO?> Register(RegistrationRequestDTO dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.UserName,
                NormalizedEmail = dto.UserName.ToUpper(),
                NormalizedUserName = dto.UserName.ToUpper(),
                Name = dto.Name
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", ErrorMessages(result)));
            }
            if (!await _roleManager.RoleExistsAsync("admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("admin"));
                await _roleManager.CreateAsync(new IdentityRole("customer"));
            }

            await _userManager.AddToRoleAsync(user, "admin");

            return _mapper.Map<UserDTO>(user);
        }

        private static List<string> ErrorMessages(IdentityResult result)
        {
            return result.Errors.Select(e => e.Description).ToList();

            //var errors = new List<string>();
            //foreach(var error in result.Errors)
            //{
            //    errors.Add(error.Description);
            //}
            //return errors;
        }
    }
}
