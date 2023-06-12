namespace minimalAPINet7OK.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public virtual List<Rol> Roles { get; set; }

        public User()
        {
            Roles = new List<Rol>();
        }
    }
}
