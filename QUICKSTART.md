# Inventory Management System - Quick Start Guide

## Prerequisites
- .NET 8 SDK installed
- SQL Server Express/LocalDB installed
- Visual Studio 2022 or VS Code

## Step 1: Initial Setup

### 1.1 Open Project
```bash
cd "Inventory Management System"
```

### 1.2 Restore NuGet Packages
```bash
dotnet restore
```

### 1.3 Create Database
```bash
dotnet ef database update
```

This will:
- Create the database `InventoryManagementSystem`
- Create all tables
- Seed default roles (Admin, User)

## Step 2: Run Application

```bash
dotnet run
```

The application will start at: `https://localhost:7000` (or configured port)

## Step 3: Register & Create First Inventory

### 3.1 Register Account
- Click **Register** in top navigation
- Enter email and password
- Submit

### 3.2 Create Inventory
- Click **+ New Inventory**
- Fill in details:
  - **Title**: e.g., "Electronics Collection"
  - **Description**: e.g., "My personal tech gadgets"
  - **Category**: Select from dropdown
  - **Visibility**: Choose Private or Public
  - **Image URL**: (optional) Cloud storage URL
- Click **Create Inventory**

### 3.3 Configure Custom ID (Optional)
- Go to inventory → **Settings** → **Custom ID**
- Click **+ Add Element**
- Add fixed text "INV-" + sequence number
- Click **Save**

### 3.4 Add Custom Fields (Optional)
- Go to inventory → **Settings** → **Custom Fields**
- Click **+ Add Field**
- Example: Add "Brand" as Single Line Text
- Click **Save**

### 3.5 Add Items
- Go to inventory → **Items** tab
- Click **+ Add Item**
- Fill in custom fields
- Click **Save**
- Item gets auto-generated custom ID

## Step 4: Share Inventory (Optional)

### 4.1 Grant Access
- Go to inventory → **Settings** → **Access Control**
- Change visibility to "Private" (if needed)
- Search for username/email
- Click **Grant Access**
- Select access level (View/Edit)

## Step 5: Explore Features

### Browse Inventories
- Click logo or **Inventories** to see all public inventories
- Use search bar to find items

### Discussions
- Go to any inventory
- Click **Discussion** tab
- Post comments (Markdown supported)

### Statistics
- Go to **Statistics** tab
- View auto-generated stats about items

### Dashboard
- Click **Dashboard** to see your inventories
- View owned and accessible inventories

## Database Schema Overview

### Key Tables
- **AspNetUsers**: User accounts
- **Inventories**: Inventory templates
- **Items**: Items in inventories
- **CustomFields**: Field definitions
- **InventoryAccess**: Access control
- **Discussions**: Comments
- **ItemLikes**: Likes on items

## Common Tasks

### Change Inventory Visibility
1. Go to inventory → **Settings** → **Access Control**
2. Select visibility level
3. For Private: manage user access
4. Click **Save Changes**

### Add More Custom Fields
1. **Settings** → **Custom Fields**
2. Click **+ Add Field**
3. Configure field type, name, description
4. Toggle "Visible in table" as needed
5. Save

### Delete Item
1. Go to inventory → **Items**
2. Find item in table
3. Click **Delete** button
4. Confirm deletion

### Search Items
1. In any inventory → **Items** tab
2. Use search box at top
3. Enter search term (searches custom ID and all fields)
4. Results update automatically

## Troubleshooting

### Database Connection Failed
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure LocalDB instance exists

### Migration Error
```bash
# Reset migrations
dotnet ef database drop
dotnet ef database update
```

### Port Already in Use
Edit `Properties/launchSettings.json` and change port number

## Performance Tips

1. **Limit results**: Inventories display 20 items per page
2. **Use search**: Instead of scrolling for large datasets
3. **Archive old items**: Keep active inventory lean

## Next Steps

1. Explore the [Full README](./README.md) for detailed documentation
2. Check the [API Documentation](./API.md) for endpoint details
3. Review [Database Schema](./SCHEMA.md) for technical details
4. Join discussions and explore community features

## Support

- 📖 Documentation: See README.md
- 🐛 Issues: Report in GitHub
- 💡 Features: Request via GitHub Issues

---

**Ready to start managing your inventory? Let's go! 🚀**
