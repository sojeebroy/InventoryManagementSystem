# Inventory Management System

A modern, scalable, web-based inventory management system built with ASP.NET Core Razor Pages and Entity Framework Core, designed for maximum flexibility and efficient data handling.

## 🎯 Features

### Core Functionality
- **Create Customizable Inventories**: Users can create templates with custom configurations
- **Item Management**: Add, edit, and delete items within inventories
- **Table-based UI**: All views use responsive table layouts (no gallery views)
- **Global Search**: Full-text search available on every page
- **Real-time Discussions**: Chronological posts with Markdown support
- **Like System**: Users can like items (one like per user per item)

### 🔑 Key Capabilities

#### Custom ID System
- Flexible ID format configuration with support for:
  - Fixed text
  - Random numbers (6, 9, 20, 32-bit)
  - GUID
  - Date/time stamps
  - Sequence numbers
- Drag-and-drop ordering of ID elements
- Real-time preview
- Automatic uniqueness enforcement within inventory

#### Custom Fields System
- Up to 3 fields of each type:
  - Single-line text
  - Multi-line text
  - Numeric
  - Boolean
  - Link
- Configurable field visibility in tables
- Tooltip descriptions
- Fixed fields (created_by, created_at, custom_id)

#### Access Control
- **Role-based**: Guest, User, Admin, Inventory Owner
- **Visibility**: Public (all authenticated users) or Private (selected users)
- **Access Levels**: View, Edit, Admin
- User search with autocomplete
- Access management interface

#### Statistics
- Auto-generated per inventory:
  - Total items count
  - Average/min/max for numeric fields
  - Most frequent values for string fields
  - Top 10 frequency analysis

### 👥 User Roles & Permissions

| Role | Permissions |
|------|-------------|
| **Guest** | View public inventories and items, search |
| **Authenticated User** | Create inventories, add items, like items, participate in discussions |
| **Inventory Owner** | Full control: edit, access management, field configuration |
| **Admin** | System-wide access, user management |

## 🏗️ Architecture

### Technology Stack
- **.NET 8** - Latest LTS framework
- **ASP.NET Core** - Web framework
- **Entity Framework Core** - ORM
- **SQL Server** - Relational database
- **Bootstrap 5** - UI framework
- **Markdig** - Markdown processing

### Database Design
- **Relational Schema**: Strictly follows database normalization
- **Fixed Columns**: Custom fields use predefined columns (no JSON storage)
- **Cascade Deletion**: Foreign key constraints with cascade delete
- **Optimistic Locking**: Version-based concurrency control
- **Composite Indexes**: For performance (inventory_id + custom_id)

### Project Structure
```
Inventory Management System/
├── Controllers/
│   ├── InventoriesController.cs      # Inventory CRUD
│   ├── ItemsController.cs            # Items API
│   ├── DiscussionsController.cs      # Discussions API
│   └── DashboardController.cs        # User dashboard
├── Models/
│   ├── ApplicationUser.cs            # Identity user
│   ├── Inventory.cs                  # Inventory entity
│   ├── Item.cs                       # Item entity
│   ├── CustomField.cs                # Custom field configuration
│   ├── InventoryAccess.cs            # Access control
│   ├── Discussion.cs                 # Discussion posts
│   └── ItemLike.cs                   # Like functionality
├── Services/
│   ├── InventoryService.cs           # Inventory business logic
│   ├── ItemService.cs                # Item management
│   ├── CustomIdService.cs            # Custom ID generation
│   ├── InventoryAuthorizationService.cs # Authorization
│   ├── DiscussionService.cs          # Discussion management
│   └── StatisticsService.cs          # Statistics calculation
├── Data/
│   └── ApplicationDbContext.cs       # Entity mappings
├── Views/
│   ├── Inventories/
│   │   ├── Index.cshtml              # Browse inventories
│   │   ├── Details.cshtml            # Inventory detail view
│   │   ├── Create.cshtml             # Create inventory
│   │   ├── Edit.cshtml               # Edit inventory
│   │   ├── Items.cshtml              # Items table
│   │   ├── Discussion.cshtml         # Discussion section
│   │   ├── Statistics.cshtml         # Statistics view
│   │   └── Settings.cshtml           # Inventory settings
│   ├── Dashboard/
│   │   └── Index.cshtml              # User dashboard
│   └── Shared/
│       └── _Layout.cshtml            # Master layout
└── Helpers/
    └── MarkdownHelper.cs             # Markdown rendering
```

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or Express)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
```bash
git clone <repository-url>
cd "Inventory Management System"
```

2. **Restore packages**
```bash
dotnet restore
```

3. **Configure database connection**
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InventoryManagementSystem;Trusted_Connection=true;"
  }
}
```

4. **Apply migrations**
```bash
dotnet ef database update
```

5. **Run the application**
```bash
dotnet run
```

Navigate to `https://localhost:7000` (or configured port)

## 📖 Usage Guide

### Creating an Inventory

1. Click **"+ New Inventory"** in the dashboard
2. Fill in:
   - **Title**: Inventory name
   - **Description**: Markdown-supported description
   - **Category**: Predefined category
   - **Visibility**: Public or Private
   - **Image URL**: Cloud storage URL
   - **Tags**: Searchable tags

3. Click **Create Inventory**

### Configuring Custom ID Format

1. Navigate to inventory **Settings** → **Custom ID**
2. Click **+ Add Element** to build your format
3. Available elements:
   - Fixed text (e.g., "INV-")
   - Random numbers (6, 9, 20, or 32-bit)
   - GUID
   - Date/time (configurable format)
   - Sequence number (configurable padding)
4. View preview in real-time
5. Click **Save**

### Adding Custom Fields

1. Go to **Settings** → **Custom Fields**
2. Click **+ Add Field**
3. Configure:
   - **Field Name**: Display name
   - **Field Type**: Text/Multi-line/Numeric/Boolean/Link
   - **Description**: Tooltip text
   - **Visibility**: Show in table view
4. Click **Save**

### Managing Access

1. Navigate to **Settings** → **Access Control**
2. Set visibility (Public/Private)
3. If Private:
   - Search for users by username/email
   - Click **Grant Access**
   - Assign access level (View/Edit/Admin)

### Adding Items

1. Go to inventory **Items** tab
2. Click **+ Add Item**
3. Fill in custom fields
4. Auto-generated Custom ID is assigned
5. Click **Save**

### Searching

- Use the **global search bar** in the header
- Searches across:
  - Inventory titles and descriptions
  - Item custom IDs and field values
  - Tags
- Limited to public inventories and user-accessible inventories

### Discussions

1. Navigate to inventory **Discussion** tab
2. Write your comment (Markdown supported)
3. Click **Post**
4. Posts are chronological, showing user and timestamp

### Liking Items

1. View items in the table
2. Click the ❤️ icon
3. One like per user per item
4. Like count displays automatically

## ⚡ Performance Considerations

### Query Optimization
- ✅ No `SELECT *` queries
- ✅ Explicit column selection with `AsNoTracking()`
- ✅ Strategic use of `.Include()` for eager loading
- ✅ No N+1 query problems
- ✅ Batch operations where applicable

### Database Efficiency
- ✅ Composite indexes on foreign keys
- ✅ Index on frequently searched columns
- ✅ Fixed column design (no JSON columns)
- ✅ Proper cascade deletion at DB level

### Application-Level Optimization
- ✅ Pagination (20 items per page)
- ✅ Caching opportunities for statistics
- ✅ Async/await throughout
- ✅ No large data transfers

## 🔐 Security Features

- **Identity & Authorization**: ASP.NET Core Identity
- **Role-based Access Control**: Via ClaimsPrincipal
- **HTTPS Only**: Enforced in production
- **CSRF Protection**: Built-in with tag helpers
- **Input Validation**: Server-side validation on all inputs
- **SQL Injection Prevention**: Entity Framework parameterized queries
- **XSS Protection**: Automatic HTML encoding in Razor
- **Concurrency Control**: Optimistic locking with version fields

## 🔄 Concurrency & Auto-save

### Optimistic Locking
- Each Inventory and Item has a `Version` field
- Modified during updates
- `DbUpdateConcurrencyException` caught and reported

### Auto-save (Implementation ready)
- Every 7-10 seconds for drafts
- Full version control
- Graceful conflict resolution

## 🌐 Localization

- **English**: Complete UI localization
- **Extensible**: Easy to add additional languages
- **Stored per user**: User preference saved

## 🎨 Theming

- **Light Mode**: Default professional theme
- **Dark Mode**: Alternative dark theme
- **Preferences**: Saved per user in database

## 📊 Statistics Engine

Automatically calculates and displays:
- **Numeric Fields**: Average, min, max
- **String Fields**: Top 10 most frequent values with counts
- **Inventory Stats**: Total items, creation date, last modified

*Note: Statistics calculated on-demand for performance*

## 🧪 Testing

Run tests:
```bash
dotnet test
```

## 📝 API Endpoints

### Items API
- `POST /api/items` - Create item
- `GET /api/items/{id}` - Get item
- `PUT /api/items/{id}` - Update item
- `DELETE /api/items/{id}` - Delete item
- `POST /api/items/{itemId}/like` - Toggle like
- `GET /api/items/{itemId}/likes` - Get like count

### Discussions API
- `POST /api/discussions` - Create discussion
- `GET /api/discussions/inventory/{inventoryId}` - Get discussions
- `DELETE /api/discussions/{id}` - Delete discussion

## 🛠️ Development

### Adding a New Feature

1. Create model in `/Models`
2. Add DbSet to `ApplicationDbContext`
3. Create migration: `dotnet ef migrations add FeatureName`
4. Create service in `/Services`
5. Register in `Program.cs`
6. Create controller/endpoint
7. Create views as needed

### Database Migrations

```bash
# Create new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Revert last migration
dotnet ef database update PreviousMigrationName
```

## 📦 Deployment

### Azure App Service
1. Configure `appsettings.Production.json`
2. Set up Azure SQL Database
3. Deploy via GitHub Actions or Visual Studio Publish

### Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY . .
RUN dotnet build
ENTRYPOINT ["dotnet", "Inventory Management System.dll"]
```

## 🐛 Troubleshooting

### Database Connection Issues
- Verify connection string in `appsettings.json`
- Ensure SQL Server instance is running
- Check LocalDB: `SqlLocalDB info`

### Migration Errors
- Remove recent migrations: `dotnet ef migrations remove`
- Check pending migrations: `dotnet ef migrations list`

### Performance Issues
- Check query logs in SQL Server Management Studio
- Use `AsNoTracking()` for read-only queries
- Enable query caching for statistics

## 📄 License

MIT License - See LICENSE file

## 👥 Contributing

Contributions are welcome! Please:
1. Fork repository
2. Create feature branch
3. Commit changes
4. Push to branch
5. Create Pull Request

## 📞 Support

For issues and questions:
- GitHub Issues: [Report here]
- Email: support@example.com

---

**Built with ❤️ using .NET 8 and modern web technologies**
