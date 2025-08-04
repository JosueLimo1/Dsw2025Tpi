namespace Dsw2025Tpi.Domain.Entities;

public abstract class EntityBase
{
    public Guid Id { get; set; } //Codigo unico alfanumerico
    protected EntityBase()
    {
        Id = Guid.NewGuid();
    }
}
