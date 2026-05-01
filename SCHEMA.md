# Database Schema & API Documentation

## 📊 Database Schema

### Entity Relationship Diagram

```
ApplicationUser
├── OwnedInventories (1:N) → Inventory
├── InventoryAccesses (1:N) → InventoryAccess
├── CreatedItems (1:N) → Item
├── Likes (1:N) → ItemLike
└── Discussions (1:N) → Discussion

Inventory
├── Tags (1:N) → InventoryTag
├── CustomFields (1:N) → CustomField
├── Items (1:N) → Item
├── AccessControls (1:N) → InventoryAccess
└── Discussions (1:N) → Discussion

Item
├── Likes (1:N) → ItemLike
└── CreatedBy → ApplicationUser

InventoryAccess
├── Inventory → Inventory
└── User → ApplicationUser

ItemLike
├── Item → Item
└── User → ApplicationUser

Discussion
├── Inventory → Inventory
└── User → ApplicationUser
```

### Table Definitions

#### ApplicationUser
```sql
CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) PRIMARY KEY,
    [UserName] nvarchar(256),
    [Email] nvarchar(256),
    [FirstName] nvarchar(max),
    [LastName] nvarchar(max),
    [IsBlocked] bit NOT NULL DEFAULT 0,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [PreferredLanguage] nvarchar(10) DEFAULT 'en',
    [Theme] nvarchar(20) DEFAULT 'light'
);
```

#### Inventory
```sql
CREATE TABLE [Inventories] (
    [Id] int PRIMARY KEY IDENTITY(1,1),
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max),
    [Category] nvarchar(max),
    [ImageUrl] nvarchar(max),
    [Visibility] int NOT NULL DEFAULT 1, -- 0=Public, 1=Private
    [OwnerId] nvarchar(450) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [Version] int NOT NULL DEFAULT 1,
    [CustomIdFormat] nvarchar(max), -- JSON serialized
    FOREIGN KEY ([OwnerId]) REFERENCES [AspNetUsers]([Id]),
    INDEX [IX_OwnerId] ([OwnerId]),
    INDEX [IX_CreatedAt] ([CreatedAt])
);
```

#### InventoryTag
```sql
CREATE TABLE [InventoryTags] (
    [Id] int PRIMARY KEY IDENTITY(1,1),
    [InventoryId] int NOT NULL,
    [Tag] nvarchar(max) NOT NULL,
    FOREIGN KEY ([InventoryId]) REFERENCES [Inventories]([Id]) ON DELETE CASCADE,
    UNIQUE INDEX [IX_InventoryTag] ([InventoryId], [Tag])
);
```

#### CustomField
```sql
CREATE TABLE [CustomFields] (
    [Id] int PRIMARY KEY IDENTITY(1,1),
    [InventoryId] int NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max),
    [FieldType] int NOT NULL, -- 0=Text, 1=MultiText, 2=Numeric, 3=Bool, 4=Link
    [DisplayOrder] int NOT NULL,
    [IsVisibleInTable] bit NOT NULL DEFAULT 1,
    [FieldName] nvarchar(max),
    FOREIGN KEY ([InventoryId]) REFERENCES [Inventories]([Id]) ON DELETE CASCADE,
    INDEX [IX_InventoryOrder] ([InventoryId], [DisplayOrder])
);
```

#### Item
```sql
CREATE TABLE [Items] (
    [Id] int PRIMARY KEY IDENTITY(1,1),
    [InventoryId] int NOT NULL,
    [CustomId] nvarchar(max) NOT NULL,
    [CreatedById] nvarchar(450) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [Version] int NOT NULL DEFAULT 1,

    -- String fields (single-line text, up to 3)
    [CustomString1Value] nvarchar(max),
    [CustomString2Value] nvarchar(max),
    [CustomString3Value] nvarchar(max),

    -- Text fields (multi-line, up to 3)
    [CustomText1Value] nvarchar(max),
    [CustomText2Value] nvarchar(max),
    [CustomText3Value] nvarchar(max),

    -- Numeric fields (up to 3)
    [CustomNumber1Value] decimal(18,2),
    [CustomNumber2Value] decimal(18,2),
    [CustomNumber3Value] decimal(18,2),

    -- Boolean fields (up to 3)
    [CustomBool1Value] bit,
    [CustomBool2Value] bit,
    [CustomBool3Value] bit,

    -- Link fields (up to 3)
    [CustomLink1Value] nvarchar(max),
    [CustomLink2Value] nvarchar(max),
    [CustomLink3Value] nvarchar(max),

    FOREIGN KEY ([InventoryId]) REFERENCES [Inventories]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([CreatedById]) REFERENCES [AspNetUsers]([Id]),
    UNIQUE INDEX [IX_InventoryCustomId] ([InventoryId], [CustomId]),
    INDEX [IX_CreatedById] ([CreatedById])
);
```

#### ItemLike
```sql
CREATE TABLE [ItemLikes] (
    [Id] int PRIMARY KEY IDENTITY(1,1),
    [ItemId] int NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [LikedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY ([ItemId]) REFERENCES [Items]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]),
    UNIQUE INDEX [IX_ItemUser] ([ItemId], [UserId])
);
```

#### InventoryAccess
```sql
CREATE TABLE [InventoryAccesses] (
    [Id] int PRIMARY KEY IDENTITY(1,1),
    [InventoryId] int NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [AccessLevel] int NOT NULL DEFAULT 0, -- 0=View, 1=Edit, 2=Admin
    [GrantedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY ([InventoryId]) REFERENCES [Inventories]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]),
    UNIQUE INDEX [IX_InventoryUser] ([InventoryId], [UserId])
);
```

#### Discussion
```sql
CREATE TABLE [Discussions] (
    [Id] int PRIMARY KEY IDENTITY(1,1),
    [InventoryId] int NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY ([InventoryId]) REFERENCES [Inventories]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]),
    INDEX [IX_InventoryId] ([InventoryId]),
    INDEX [IX_UserId] ([UserId])
);
```

## 🔌 REST API Documentation

### Base URL
```
https://localhost:7000/api
```

### Authentication
- All endpoints except GET public items require authentication
- Use Bearer token in Authorization header
- Login returns JWT token

### Items Endpoints

#### Create Item
```http
POST /api/items
Content-Type: application/json
Authorization: Bearer {token}

{
    "inventoryId": 1,
    "customString1Value": "Samsung",
    "customNumber1Value": 999.99,
    "customBool1Value": true,
    "customLink1Value": "https://example.com"
}

Response: 201 Created
{
    "id": 1,
    "customId": "ITEM-000001",
    "customString1Value": "Samsung",
    ...
}
```

#### Get Item
```http
GET /api/items/1
Authorization: Bearer {token}

Response: 200 OK
{
    "id": 1,
    "customId": "ITEM-000001",
    ...
}
```

#### Update Item
```http
PUT /api/items/1
Content-Type: application/json
Authorization: Bearer {token}

{
    "id": 1,
    "customString1Value": "LG",
    "version": 1
}

Response: 200 OK
```

#### Delete Item
```http
DELETE /api/items/1
Authorization: Bearer {token}

Response: 204 No Content
```

### Like Endpoints

#### Toggle Like
```http
POST /api/items/1/like
Authorization: Bearer {token}

Response: 200 OK
{
    "liked": true,
    "likeCount": 5
}
```

#### Get Likes
```http
GET /api/items/1/likes

Response: 200 OK
{
    "count": 5,
    "isLiked": true
}
```

### Discussions Endpoints

#### Create Discussion
```http
POST /api/discussions
Content-Type: application/json
Authorization: Bearer {token}

{
    "inventoryId": 1,
    "content": "Great inventory! **Bold text** supported."
}

Response: 201 Created
{
    "id": 1,
    "inventoryId": 1,
    "userId": "user-id",
    "content": "Great inventory! **Bold text** supported.",
    "createdAt": "2024-01-15T10:30:00Z"
}
```

#### Get Discussions
```http
GET /api/discussions/inventory/1

Response: 200 OK
[
    {
        "id": 1,
        "inventoryId": 1,
        "user": {
            "id": "user-id",
            "userName": "john_doe"
        },
        "content": "Great inventory!",
        "createdAt": "2024-01-15T10:30:00Z"
    }
]
```

#### Delete Discussion
```http
DELETE /api/discussions/1
Authorization: Bearer {token}

Response: 204 No Content
```

## 🔐 HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200  | OK - Request successful |
| 201  | Created - Resource created |
| 204  | No Content - Successful deletion |
| 400  | Bad Request - Invalid input |
| 401  | Unauthorized - Authentication required |
| 403  | Forbidden - No permission |
| 404  | Not Found - Resource doesn't exist |
| 409  | Conflict - Concurrency issue |
| 500  | Server Error |

## 📋 Query Parameters

### Search
```
GET /inventories?searchTerm=electronics
```

### Pagination
```
GET /inventories/items/1?page=2
```

### Filtering
```
GET /dashboard?sortBy=date&filterBy=all
```

## 🎯 DTOs (Data Transfer Objects)

### CreateInventoryDto
```csharp
public class CreateInventoryDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string ImageUrl { get; set; }
    public string VisibilityType { get; set; }
    public List<string> Tags { get; set; }
}
```

### CreateItemDto
```csharp
public class CreateItemDto
{
    public int InventoryId { get; set; }
    public string CustomString1Value { get; set; }
    public string CustomString2Value { get; set; }
    public string CustomString3Value { get; set; }
    public string CustomText1Value { get; set; }
    public string CustomText2Value { get; set; }
    public string CustomText3Value { get; set; }
    public decimal? CustomNumber1Value { get; set; }
    public decimal? CustomNumber2Value { get; set; }
    public decimal? CustomNumber3Value { get; set; }
    public bool? CustomBool1Value { get; set; }
    public bool? CustomBool2Value { get; set; }
    public bool? CustomBool3Value { get; set; }
    public string CustomLink1Value { get; set; }
    public string CustomLink2Value { get; set; }
    public string CustomLink3Value { get; set; }
}
```

### CreateDiscussionDto
```csharp
public class CreateDiscussionDto
{
    public int InventoryId { get; set; }
    public string Content { get; set; }
}
```

## 🔍 Query Examples

### Find All Items in Inventory
```sql
SELECT * FROM Items WHERE InventoryId = 1 ORDER BY CreatedAt DESC;
```

### Get Popular Items (Most Liked)
```sql
SELECT i.*, COUNT(il.Id) as LikeCount 
FROM Items i
LEFT JOIN ItemLikes il ON i.Id = il.ItemId
WHERE i.InventoryId = 1
GROUP BY i.Id
ORDER BY LikeCount DESC;
```

### Get User's Accessible Inventories
```sql
SELECT DISTINCT i.* 
FROM Inventories i
LEFT JOIN InventoryAccesses ia ON i.Id = ia.InventoryId
WHERE i.OwnerId = 'user-id' 
   OR i.Visibility = 0 
   OR ia.UserId = 'user-id'
ORDER BY i.UpdatedAt DESC;
```

### Search Items by Custom Fields
```sql
SELECT i.* FROM Items i
WHERE i.InventoryId = 1
AND (i.CustomId LIKE '%search%'
     OR i.CustomString1Value LIKE '%search%'
     OR i.CustomString2Value LIKE '%search%'
     OR i.CustomString3Value LIKE '%search%')
LIMIT 50;
```

## ⚙️ Configuration Options

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InventoryManagementSystem;Trusted_Connection=true;"
  },
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;...",
    "ContainerName": "inventory-images"
  }
}
```

## 📚 Enumerations

### VisibilityType
- `0` - Public
- `1` - Private

### AccessLevel
- `0` - View
- `1` - Edit
- `2` - Admin

### CustomFieldType
- `0` - SingleLineText
- `1` - MultiLineText
- `2` - Numeric
- `3` - Boolean
- `4` - Link

---

**Last Updated**: 2024
**Version**: 1.0
