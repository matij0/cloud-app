using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudBackend.Data;
using CloudBackend.Models;
using CloudBackend.DTOs;

namespace CloudBackend.Controllers;
[ApiController]
[Route("api/[controller]")] // Adres: http://localhost:8081/api/tasks
public class TasksController : ControllerBase 
{
    private readonly AppDbContext _context;
    // Wstrzykiwanie zależności (Dependency Injection) kontekstu bazy danych
    public TasksController(AppDbContext context) 
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskReadDto>>> GetAll()
    {
        // Pobieramy encje z bazy danych
        var tasks = await _context.Tasks.ToListAsync();
        // Mapujemy każdą encję na obiekt DTO
        var tasksDto = tasks.Select(t => new TaskReadDto
        {
            Id = t.Id,
            Name = t.Name,
            IsCompleted = t.IsCompleted
        });
        return Ok(tasksDto);
    }
 
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskReadDto>> GetById(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();  // Zwracamy DTO zamiast czystej encji
        return Ok(new TaskReadDto 
        { 
            Id = task.Id, 
            Name = task.Name, 
            IsCompleted = task.IsCompleted 
        });
    }

    [HttpPost]
    public async Task<ActionResult<TaskReadDto>> Create(TaskCreateDto taskDto)
    {
        // 1. Mapowanie DTO -> Entity
        // Przekształcamy to, co przyszło z sieci, na model bazy danych
        var newTask = new CloudTask
        {
            Name = taskDto.Name,
            IsCompleted = false // Domyślnie nowe zadanie nie jest gotowe
        };

        // 2. Zapis do bazy danych
        _context.Tasks.Add(newTask);
        await _context.SaveChangesAsync();

        // 3. Mapowanie Entity -> DTO (Zwrotka)
        // Zwracamy TaskReadDto, który zawiera już nadane przez bazę Id
        var readDto = new TaskReadDto
        {
            Id = newTask.Id,
            Name = newTask.Name,
            IsCompleted = newTask.IsCompleted
        };

        return CreatedAtAction(nameof(GetById), new { id = readDto.Id }, readDto);
    }
 

    [HttpPut("{id}")] // 4. Edytuj (UPDATE)
    public async Task<ActionResult> Update(int id, CloudTask task) 
    {
        if (id != task.Id) return BadRequest("ID mismatch");   
        _context.Entry(task).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent(); // Status 204 - operacja udana, brak danych do odesłania
    }
    
    [HttpDelete("{id}")] // 5. Usuń (DELETE)
    public async Task<ActionResult> Delete(int id) 
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();   
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
