using minimalAPINet7OK.Models;

namespace minimalAPINet7OK.Dtos
{
    public class CreateEditUserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string? Password { get; set; }

        public List<int>? Roles { get; set; }
    }
}
