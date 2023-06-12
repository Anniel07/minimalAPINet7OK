namespace minimalAPINet7OK.Models
{
    public class Rol
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual List<User> Users { get; set; }

        public Rol()
        {
            Users = new List<User>();
        }
    }
}
