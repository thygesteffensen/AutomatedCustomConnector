using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WebApi;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Register OpenAPI 2.0 document for Custom Connector
builder.Services.AddOpenApi("cc", options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi2_0;
    options.AddDocumentTransformer((document, _, _) =>
    {
        PopulateOperationIds(document);

        return Task.CompletedTask;
    });
});

// Default OpenAPI 3.0 document
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        PopulateOperationIds(document);

        return Task.CompletedTask;
    });
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // app.UseSwaggerUI(options => // Needs Swashbuckle.AspNetCore
    // {
    //     options.SwaggerEndpoint("/openapi/v1.json", "v1");
    // });
}

var todoItems = app
    .MapGroup("/todoItems")
    .WithOpenApi(operation =>
    {
        operation.OperationId = "GetExample";
        return operation;
    });

todoItems
    .MapGet("/",
        async Task<Results<Ok<TodoItemDto[]>, NoContent>> (TodoDb db) =>
            TypedResults.Ok(await db.Todos.Select(x => new TodoItemDto(x)).ToArrayAsync()))
    .WithDescription("Get all todo items")
    .WithSummary("Get all todo items");

todoItems
    .MapGet("/complete",
        async Task<Results<Ok<List<TodoItemDto>>, NoContent>> (TodoDb db) =>
            TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).Select(x => new TodoItemDto(x)).ToListAsync()))
    .WithDescription("Get all complete todo items")
    .WithSummary("Get all complete todo items");

todoItems
    .MapGet("/{id:int}", async Task<Results<Ok<TodoItemDto>, NotFound>> (TodoDb db, int id) =>
        await db.Todos.FindAsync(id) is { } todo ? TypedResults.Ok(new TodoItemDto(todo)) : TypedResults.NotFound())
    .WithDescription("Get a todo item by id")
    .WithSummary("Get a todo item by id");

todoItems
    .MapPost("/", async Task<Results<Created<TodoItemDto>, NoContent>> (TodoDb db, TodoItemDto todoItemDto) =>
    {
        var todoItem = new Todo
        {
            IsComplete = todoItemDto.IsComplete,
            Name = todoItemDto.Name
        };

        db.Todos.Add(todoItem);
        await db.SaveChangesAsync();

        todoItemDto = new TodoItemDto(todoItem);

        return TypedResults.Created($"/todoItems/{todoItem.Id}", todoItemDto);
    })
    .WithDescription("Create a todo item")
    .WithSummary("Create a todo item");

todoItems
    .MapPut("/{id:int}", async Task<Results<NoContent, NotFound>> (TodoDb db, int id, TodoItemDto todoItemDto) =>
    {
        var todo = await db.Todos.FindAsync(id);

        if (todo is null) return TypedResults.NotFound();

        todo.Name = todoItemDto.Name;
        todo.IsComplete = todoItemDto.IsComplete;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    })
    .WithDescription("Update a todo item")
    .WithSummary("Update a todo item");

todoItems.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (TodoDb db, int id) =>
    {
        if (await db.Todos.FindAsync(id) is not { } todo) return TypedResults.NotFound();

        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    })
    .WithDescription("Delete a todo item")
    .WithSummary("Delete a todo item");

app.Run();
return;

void PopulateOperationIds(OpenApiDocument openApiDocument)
{
    foreach (var (openApiPathItemKey, openApiPathItem) in openApiDocument.Paths)
    {
        foreach (var (openApiOperationKey, openApiOperation) in openApiPathItem.Operations)
        {
            var version = $"V{openApiDocument.Info.Version.AsSpan()[0]}";
            var path = openApiPathItemKey;
            var paths = openApiOperation.Parameters ?? [];
            path = Regex.Replace(path, @"\{[^}]*\}", "");
            path = path.Remove(0, 1);
            path = path.Split('/').Aggregate("", (acc, e) => acc + FirstCharToUpper(e));
            if (paths.Any())
                path = $"{path}By{string.Join("And", paths.Select(x => FirstCharToUpper(x.Name)))}";

            openApiOperation.OperationId =
                $"{FirstCharToUpper(version)}{FirstCharToUpper(openApiOperationKey.ToString())}{path}";
            continue;

            string FirstCharToUpper(string input) =>
                string.IsNullOrWhiteSpace(input)
                    ? ""
                    : input.First().ToString().ToUpper() + input.AsSpan(1).ToString();
        }
    }
}
