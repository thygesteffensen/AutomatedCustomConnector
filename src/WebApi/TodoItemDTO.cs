namespace WebApi;

public record Base
{
    public int Id { get; set; }
}

public record TodoItemDto : Base
{
    public string? Name { get; set; }
    public bool IsComplete { get; set; }

    public TodoItemDto()
    {
    }

    public TodoItemDto(Todo todoItem) =>
        (Id, Name, IsComplete) = (todoItem.Id, todoItem.Name, todoItem.IsComplete);
}
