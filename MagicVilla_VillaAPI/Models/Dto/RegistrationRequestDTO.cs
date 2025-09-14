namespace MagicVilla_VillaAPI.Models.Dto
{
    public class RegistrationRequestDTO
    {
        public string UserName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
