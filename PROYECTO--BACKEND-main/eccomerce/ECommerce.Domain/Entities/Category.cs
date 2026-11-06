namespace ECommerce.Domain.Entities;

public class Category
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Category() { }

    public Category(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
