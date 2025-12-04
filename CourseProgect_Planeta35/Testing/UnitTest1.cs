using Xunit;
using System.Collections.Generic;
using CourseProgect_Planeta35.Models;

public class InventoryFilterServiceTests
{
    private List<InventoryItem> CreateTestData()
    {
        var cat1 = new AssetCategory { Name = "IT" };
        var cat2 = new AssetCategory { Name = "Мебель" };

        return new List<InventoryItem>
        {
            new InventoryItem { Asset = new Asset { Name = "Ноутбук", Status = "В эксплуатации", Category = cat1 } },
            new InventoryItem { Asset = new Asset { Name = "Стол", Status = "Списано", Category = cat2 } },
            new InventoryItem { Asset = new Asset { Name = "Монитор", Status = "В эксплуатации", Category = cat1 } },
            new InventoryItem { Asset = new Asset { Name = "Стул", Status = "На обслуживании", Category = cat2 } },
        };
    }

    [Fact]
    public void Filter_ByCategory_ShouldReturnCorrectItems()
    {
        var items = CreateTestData();
        var result = InventoryFilterService.Filter(items, "", "IT", "Все");

        Assert.Equal(2, result.Count);
        Assert.All(result, i => Assert.Equal("IT", i.Asset.Category.Name));
    }

    [Fact]
    public void Filter_ByStatus_ShouldReturnCorrectItems()
    {
        var items = CreateTestData();
        var result = InventoryFilterService.Filter(items, "", "Все", "Списано");

        Assert.Single(result);
        Assert.Equal("Списано", result[0].Asset.Status);
    }

    [Fact]
    public void Filter_BySearchText_ShouldReturnCorrectItems()
    {
        var items = CreateTestData();
        var result = InventoryFilterService.Filter(items, "ст", "Все", "Все"); // ищем "ст" → Стол и Стул

        Assert.Equal(2, result.Count);
        Assert.Contains(result, i => i.Asset.Name == "Стол");
        Assert.Contains(result, i => i.Asset.Name == "Стул");
    }
}
