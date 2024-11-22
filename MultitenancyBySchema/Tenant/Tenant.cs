using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MultitenancyBySchema.Tenant;

[Index(nameof(Name), IsUnique = true)]
public class Tenant
{
    [Key] 
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string GetSchema()
    {
        return Name;
    }
}