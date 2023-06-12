using minimalAPINet7OK.Models;

namespace minimalAPINet7OK.Dtos
{
    public class TodoItemDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsComplete { get; set; }

        public TodoItemDTO() { }
        public TodoItemDTO(Todo todoItem) =>
        (Id, Name, IsComplete) = (todoItem.Id, todoItem.Name, todoItem.IsComplete);
    }
}
