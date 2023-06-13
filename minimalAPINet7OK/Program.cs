using Microsoft.EntityFrameworkCore;
using minimalAPINet7OK.DataContext;
using minimalAPINet7OK.Dtos;
using minimalAPINet7OK.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "First api doc", Version = "v1" });
});

//builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDbContext<TodoDb>(options =>
{
    var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    options.UseSqlite($"Data Source={Path.Join(path, "MyDB1.db")}");
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetService<TodoDb>();
db?.Database.MigrateAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));
}

//app.UseHttpsRedirection();

var todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/", GetAllTodos).WithName("GetWeatherForecast").WithOpenApi();
todoItems.MapGet("/complete", GetCompleteTodos);
todoItems.MapGet("/{id}", GetTodo);
todoItems.MapPost("/", CreateTodo);
todoItems.MapPut("/{id}", UpdateTodo);
todoItems.MapDelete("/{id}", DeleteTodo);


var roles = app.MapGroup("/roles");

roles.MapGet("/", GetAllRoles);
roles.MapGet("/{id}", GetRol);
roles.MapPost("/", CreateRol);
roles.MapPut("/{id}", UpdateRol);
roles.MapDelete("/{id}", DeleteRol);


var users = app.MapGroup("/usuarios");

users.MapGet("/", GetAllUsers);
users.MapGet("/{id}", GetUser);
users.MapPost("/", CreateUser);
users.MapPut("/{id}", UpdateUser);
users.MapDelete("/{id}", DeleteUser);

app.Run();

#region Todo
//define route handler methods
static async Task<IResult> GetAllTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Select(x => new TodoItemDTO(x)).ToArrayAsync());
}

static async Task<IResult> GetCompleteTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).Select(x => new TodoItemDTO(x)).ToListAsync());
}

static async Task<IResult> GetTodo(int id, TodoDb db)
{
    return await db.Todos.FindAsync(id)
        is Todo todo
            ? TypedResults.Ok(new TodoItemDTO(todo))
            : TypedResults.NotFound();
}

static async Task<IResult> CreateTodo(TodoItemDTO todoItemDTO, TodoDb db)
{
    var todoItem = new Todo
    {
        IsComplete = todoItemDTO.IsComplete,
        Name = todoItemDTO.Name
    };

    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();

    todoItemDTO = new TodoItemDTO(todoItem);

    return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
}

static async Task<IResult> UpdateTodo(int id, TodoItemDTO todoItemDTO, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return TypedResults.NotFound();

    todo.Name = todoItemDTO.Name;
    todo.IsComplete = todoItemDTO.IsComplete;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteTodo(int id, TodoDb db)
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();

        TodoItemDTO todoItemDTO = new TodoItemDTO(todo);

        return TypedResults.Ok(todoItemDTO);
    }

    return TypedResults.NotFound();
}

#endregion


#region Rol
//define route handler methods
static async Task<IResult> GetAllRoles(TodoDb db)
{
    return TypedResults.Ok(await db.Roles.ToArrayAsync());
}

static async Task<IResult> GetRol(int id, TodoDb db)
{
    return await db.Roles.FindAsync(id)
        is Rol rol
            ? TypedResults.Ok(rol)
            : TypedResults.NotFound();
}

static async Task<IResult> CreateRol(Rol rol, TodoDb db)
{

    db.Roles.Add(rol);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/roles/{rol.Id}", rol);
}

static async Task<IResult> UpdateRol(int id, Rol rolInp, TodoDb db)
{
    var rol = await db.Roles.FindAsync(id);

    if (rol is null) return TypedResults.NotFound();

    rol.Name = rolInp.Name;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteRol(int id, TodoDb db)
{
    if (await db.Roles.FindAsync(id) is Rol rol)
    {
        db.Roles.Remove(rol);
        await db.SaveChangesAsync();

        return TypedResults.Ok(rol);
    }

    return TypedResults.NotFound();
}

#endregion


#region User
//define route handler methods
static async Task<IResult> GetAllUsers(TodoDb db)
{
    var users = await db.Users.Include(u => u.Roles).Select(u => new UserDto(u)).ToArrayAsync();
    
    return TypedResults.Ok(users);
}

static async Task<IResult> GetUser(int id, TodoDb db)
{
    return await db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id)
        is User user
            ? TypedResults.Ok(new UserDto(user))
            : TypedResults.NotFound();
}

static async Task<IResult> CreateUser(CreateEditUserDto userDto, TodoDb db)
{
    var user = new User
    {
        UserName = userDto.UserName,
        Password = userDto.Password ?? "1234", //hash in future use
    };
    //append roles to user, note los roles deben existir para ser adicionados
    if (userDto.Roles != null && userDto.Roles.Count > 0)
    {
        foreach (var rolId in userDto.Roles)
        {
            var rol = await db.Roles.FindAsync(rolId);
            if (rol != null)
            {
                user.Roles.Add(rol);
            }
        }
    }

    db.Users.Add(user);
    await db.SaveChangesAsync();

    //todoItemDTO = new TodoItemDTO(todoItem);

    return TypedResults.Created($"/todoitems/{user.Id}", user.UserName + " " + user.Password);
}

static async Task<IResult> UpdateUser(int id, CreateEditUserDto editDto, TodoDb db)
{
    var user = await db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);

    if (user is null) return TypedResults.NotFound(); //keep tracked

    user.UserName = editDto.UserName;
    if (editDto.Password != null)
        user.Password = editDto.Password;

    // la interseccion de ambos conjuntos se mantiene 
    // los q vienen en el dto y no estan en la entity se adicionan
    IEnumerable<int> addIds = new List<int>();

    //los q estan en el entity y no estan en el dto se eliminan
    IEnumerable<int> removeIds = new List<int>();

    if (editDto.Roles != null) {
        var userRolesIds = user.Roles.Select(r => r.Id);
        addIds = editDto.Roles.Except(userRolesIds);
        removeIds = userRolesIds.Except(editDto.Roles);
    }
        

    foreach (var addId in addIds)
    {
        var rol = await db.Roles.FindAsync(addId);
        if (rol != null)
        {
            user.Roles.Add(rol);
        }
    }

    user.Roles.RemoveAll(x => removeIds.Contains(x.Id));

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteUser(int id, TodoDb db)
{
    if (await db.Users.FindAsync(id) is User user)
    {
        db.Users.Remove(user);
        await db.SaveChangesAsync();

        return TypedResults.Ok(user);
    }

    return TypedResults.NotFound();
}

#endregion