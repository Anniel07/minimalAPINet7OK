using minimalAPINet7OK.Models;

namespace minimalAPINet7OK.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public List<RolDto> Roles { get; set; }

        public UserDto(User u) 
        {
            Id = u.Id;
            UserName = u.UserName;
            Password = u.Password;
            Roles = new();
            foreach (var rol in u.Roles)
            {
                Roles.Add(new RolDto(rol.Id, rol.Name));
            }
        }

        
    }
    public record RolDto(int Id, string Name);
}
