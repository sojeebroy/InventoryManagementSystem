using Inventory_Management_System.Data;
using Inventory_Management_System.Models;
using Inventory_Management_System.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_System.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var viewModel = new HomePageViewModel();

            viewModel.LatestInventories = await _context.Inventories
                .Where(i => i.Visibility == VisibilityType.Public)
                .Include(i => i.Owner)
                .Include(i => i.Tags)
                .OrderByDescending(i => i.CreatedAt)
                .Take(10)
                .Select(i => new InventoryCardDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description.Length > 100 ? i.Description.Substring(0, 100) + "..." : i.Description,
                    ImageUrl = i.ImageUrl,
                    OwnerName = i.Owner != null ? $"{i.Owner.FirstName} {i.Owner.LastName}".Trim() : "Unknown",
                    CreatedAt = i.CreatedAt,
                    ItemCount = i.Items.Count,
                    Tags = i.Tags.Select(t => t.Tag).Take(3).ToList()
                })
                .ToListAsync();

            viewModel.TopPopularInventories = await _context.Inventories
                .Where(i => i.Visibility == VisibilityType.Public)
                .Include(i => i.Owner)
                .Include(i => i.Tags)
                .Include(i => i.Items)
                .OrderByDescending(i => i.Items.Count)
                .Take(5)
                .Select(i => new InventoryCardDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description.Length > 100 ? i.Description.Substring(0, 100) + "..." : i.Description,
                    ImageUrl = i.ImageUrl,
                    OwnerName = i.Owner != null ? $"{i.Owner.FirstName} {i.Owner.LastName}".Trim() : "Unknown",
                    CreatedAt = i.CreatedAt,
                    ItemCount = i.Items.Count,
                    Tags = i.Tags.Select(t => t.Tag).Take(3).ToList()
                })
                .ToListAsync();

            viewModel.TagCloud = await _context.InventoryTags
                .Where(t => t.Inventory!.Visibility == VisibilityType.Public)
                .GroupBy(t => t.Tag)
                .Select(g => new TagCloudDto
                {
                    TagName = g.Key,
                    Frequency = g.Count(),
                    Size = CalculateTagSize(g.Count()) 
                })
                .OrderByDescending(t => t.Frequency)
                .Take(50)
                .ToListAsync();

            _logger.LogInformation("Home page loaded successfully");
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading home page");
            return StatusCode(500, "Error loading home page");
        }
    }

[HttpGet]
    public async Task<IActionResult> Search(string query, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(query))
            return RedirectToAction(nameof(Index));

        try
        {
            const int pageSize = 20;
            var normalizedQuery = query.Trim().ToLower();

            var results = await _context.Inventories
                .Where(i => i.Visibility == VisibilityType.Public &&
                    (EF.Functions.Like(i.Title, $"%{normalizedQuery}%") ||
                     EF.Functions.Like(i.Description, $"%{normalizedQuery}%") ||
                     i.Tags.Any(t => EF.Functions.Like(t.Tag, $"%{normalizedQuery}%"))))
                .Include(i => i.Owner)
                .Include(i => i.Tags)
                .Include(i => i.Items)
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize + 1)
                .Select(i => new InventoryCardDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description.Length > 100 ? i.Description.Substring(0, 100) + "..." : i.Description,
                    ImageUrl = i.ImageUrl,
                    OwnerName = i.Owner != null ? $"{i.Owner.FirstName} {i.Owner.LastName}".Trim() : "Unknown",
                    CreatedAt = i.CreatedAt,
                    ItemCount = i.Items.Count,
                    Tags = i.Tags.Select(t => t.Tag).Take(3).ToList()
                })
                .ToListAsync();

            var hasNextPage = results.Count > pageSize;
            var viewModel = new SearchResultsViewModel
            {
                Query = query,
                Results = results.Take(pageSize).ToList(),
                CurrentPage = page,
                HasNextPage = hasNextPage,
                TotalResults = results.Count
            };

            _logger.LogInformation("Search executed for query: {Query}", query);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing search for query: {Query}", query);
            return StatusCode(500, "Error executing search");
        }
    }

    [HttpGet]
    public async Task<IActionResult> FilterByTag(string tag, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return RedirectToAction(nameof(Index));

        try
        {
            const int pageSize = 20;

            var inventoryIds = await _context.InventoryTags
                .Where(t => t.Tag.ToLower() == tag.ToLower() &&
                    t.Inventory!.Visibility == VisibilityType.Public)
                .Select(t => t.InventoryId)
                .Distinct()
                .OrderByDescending(id => _context.Inventories.Where(i => i.Id == id).Select(i => i.CreatedAt).FirstOrDefault())
                .Skip((page - 1) * pageSize)
                .Take(pageSize + 1)
                .ToListAsync();

            var results = await _context.Inventories
                .Where(i => inventoryIds.Contains(i.Id))
                .Include(i => i.Owner)
                .Include(i => i.Tags)
                .Include(i => i.Items)
                .Select(i => new InventoryCardDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description.Length > 100 ? i.Description.Substring(0, 100) + "..." : i.Description,
                    ImageUrl = i.ImageUrl,
                    OwnerName = i.Owner != null ? $"{i.Owner.FirstName} {i.Owner.LastName}".Trim() : "Unknown",
                    CreatedAt = i.CreatedAt,
                    ItemCount = i.Items.Count,
                    Tags = i.Tags.Select(t => t.Tag).Take(3).ToList()
                })
                .ToListAsync();

            var hasNextPage = inventoryIds.Count > pageSize;
            var viewModel = new TagFilterViewModel
            {
                Tag = tag,
                Results = results,
                CurrentPage = page,
                HasNextPage = hasNextPage
            };

            _logger.LogInformation("Tag filter applied: {Tag}", tag);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering by tag: {Tag}", tag);
            return StatusCode(500, "Error filtering by tag");
        }
    }

    private static string CalculateTagSize(int frequency)
    {
        return frequency switch
        {
            >= 20 => "ExtraLarge",
            >= 10 => "Large",
            >= 5 => "Medium",
            _ => "Small"
        };
    }
}


