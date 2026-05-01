# Authentication & Authorization Guide

## Overview

The Inventory Management System implements **OAuth 2.0-based authentication** using Google and Facebook as social login providers. This eliminates the need for password management and provides secure, industry-standard authentication.

---

## Authentication Flow

### User Registration & Login Flow

```
User clicks "Login with Google/Facebook"
    ↓
Redirected to Provider OAuth Page
    ↓
User Grants Permission
    ↓
Provider Returns User Data (email, name, picture)
    ↓
System Checks if User Exists
    ↓
    ├─→ User Exists: Sign In & Update Last Login
    └─→ New User: Create Account & Sign In
    ↓
Redirect to Dashboard
```

---

## User Model

### ApplicationUser Properties

```csharp
// OAuth Provider Information
public string? Provider { get; set; }           // "Google" or "Facebook"
public string? ProviderUserId { get; set; }    // Provider's unique ID
public string? ProfilePictureUrl { get; set; } // User's avatar

// User Profile
public string? FirstName { get; set; }
public string? LastName { get; set; }
public string UserName { get; set; }           // From email prefix
public string Email { get; set; }              // Unique email

// Account Status
public bool IsBlocked { get; set; }
public DateTime CreatedAt { get; set; }
public DateTime? LastLoginAt { get; set; }

// User Preferences
public string? PreferredLanguage { get; set; }  // Default: "en"
public string? Theme { get; set; }              // Default: "light"
```

---

## Role-Based Access Control (RBAC)

### Available Roles

1. **Admin**
   - Full system access
   - Can manage all users
   - Can block/unblock users
   - View all inventories
   - Modify any inventory

2. **User** (default for new users)
   - Create own inventories
   - Add/edit items in owned inventories
   - Access granted inventories
   - Like and comment on items
   - View public inventories

3. **Guest** (unauthenticated)
   - View public inventories only
   - No create/edit permissions

---

## Authorization Rules

| Action | Guest | User | Owner | Admin |
|--------|-------|------|-------|-------|
| View public inventories | ✅ | ✅ | ✅ | ✅ |
| View private inventories | ❌ | ✅* | ✅ | ✅ |
| Create inventory | ❌ | ✅ | ✅ | ✅ |
| Edit inventory | ❌ | ❌ | ✅ | ✅ |
| Delete inventory | ❌ | ❌ | ✅ | ✅ |
| Add items | ❌ | ✅* | ✅ | ✅ |
| Edit items | ❌ | ✅* | ✅ | ✅ |
| Delete items | ❌ | ✅* | ✅ | ✅ |
| Manage access | ❌ | ❌ | ✅ | ✅ |
| Like/comment | ❌ | ✅ | ✅ | ✅ |
| Manage users | ❌ | ❌ | ❌ | ✅ |

*Depends on inventory access level

---

## Access Control at Inventory Level

### Visibility Levels

1. **Public**
   - Anyone can view
   - Guests can see items
   - Only authorized users can edit

2. **Private**
   - Only owner and granted users can access
   - Owner controls who has access
   - Access levels: Viewer, Editor, Admin

### Access Levels (Private Inventories)

- **Viewer**: Read-only access
- **Editor**: Can add/modify items
- **Admin**: Can manage access and settings

---

## Account Security

### Blocked Users

**Behavior:**
- Cannot login
- If logged in, session is invalidated
- Redirected to blocked user page
- Admin can block/unblock from admin panel

**Implementation:**
```csharp
// Check during login
if (user.IsBlocked)
{
    await _signInManager.SignOutAsync();
    return RedirectToAction("BlockedUser");
}

// Middleware checks on every request
if (context.User.Identity?.IsAuthenticated && user.IsBlocked)
{
    await _signInManager.SignOutAsync();
    context.Response.Redirect("/Account/BlockedUser");
}
```

### Last Login Tracking

- Updated on every successful login
- Used for security monitoring
- Helps identify suspicious activity

---

## Session Management

### Session Duration
- **Cookie Expiration**: 30 days (configurable)
- **Sliding Expiration**: Yes (resets on activity)
- **HttpOnly Cookies**: Yes (prevents JavaScript access)
- **Secure Cookies**: Yes (HTTPS only in production)

### Logout
- Clears authentication cookie
- Invalidates session
- Redirects to home page

---

## Implementation Details

### AccountController Endpoints

```
POST /Account/LoginWithProvider
- Initiates OAuth flow with specified provider
- Parameters: provider (Google/Facebook), returnUrl

GET /Account/ExternalLoginCallback
- Handles OAuth provider callback
- Creates/updates user if needed
- Signs in user

GET /Account/LogoutConfirm
- Logs out user (for logout links)

POST /Account/Logout
- Logs out user (for logout form)

GET /Account/BlockedUser
- Displays blocked user message
```

### Authorization Attributes

```csharp
[Authorize]
public class DashboardController : Controller
{
    // Only logged-in users can access
}

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    // Only admins can access
}

[AllowAnonymous]
public IActionResult Login()
{
    // Anyone can access (including logged-in users)
}
```

---

## Data Protection

### What Data is Stored

**From OAuth Provider:**
- Email (unique identifier)
- Name (first and last)
- Profile picture URL
- Provider ID

**System Generated:**
- User ID (GUID)
- Account creation date
- Last login date
- Blocked status
- User preferences

### What is NOT Stored
- ❌ Passwords
- ❌ Provider authentication tokens (not persisted)
- ❌ Provider refresh tokens
- ❌ Sensitive account information

### Data Privacy
- GDPR compliant (stores only necessary data)
- Users can request data export
- Admins can delete user accounts
- All data transmitted over HTTPS

---

## Multi-Provider Account Linking

### Same Email, Different Providers

**Current Behavior:**
- Separate accounts created if same email
- Email verification needed on duplicate
- System adds suffix (_1, _2) if duplicate

**Future Enhancement:**
```csharp
// Link Facebook account to Google account
var currentUser = await _userManager.GetUserAsync(User);
var facebookInfo = await _signInManager.GetExternalLoginInfoAsync();
await _userManager.AddLoginAsync(currentUser, facebookInfo);
```

---

## Configuration

### Required Settings (appsettings.json)

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_CLIENT_ID.apps.googleusercontent.com",
      "ClientSecret": "YOUR_CLIENT_SECRET"
    },
    "Facebook": {
      "ClientId": "YOUR_APP_ID",
      "ClientSecret": "YOUR_APP_SECRET"
    }
  }
}
```

### Environment Variables (Recommended for Production)

```bash
export AUTH_GOOGLE_CLIENTID="xxx"
export AUTH_GOOGLE_SECRET="xxx"
export AUTH_FACEBOOK_CLIENTID="xxx"
export AUTH_FACEBOOK_SECRET="xxx"
```

---

## Security Best Practices

✅ **Implemented:**
- OAuth 2.0 secure flow
- No password storage
- HTTPS-only cookies
- CSRF protection (anti-forgery tokens)
- Session invalidation for blocked users
- Secure token handling

✅ **Recommendations:**
- Always use HTTPS
- Rotate OAuth secrets regularly
- Monitor failed login attempts
- Implement rate limiting on login
- Use strong CORS policies
- Enable security headers

---

## Troubleshooting

### User Cannot Login

1. **Check OAuth Configuration**
   ```
   - Verify ClientId and ClientSecret are correct
   - Confirm redirect URLs match exactly
   ```

2. **Check User Status**
   ```
   - Is user blocked?
   - Is provider configured?
   - Does user exist in database?
   ```

3. **Check Session**
   ```
   - Clear browser cookies
   - Try incognito/private mode
   - Check server logs
   ```

### "Redirect URI Mismatch"

- OAuth provider rejects redirect URL
- Ensure redirect URLs in provider settings include:
  - Exact protocol (https vs http)
  - Exact domain and port
  - Correct callback path

### User Session Expires Too Quickly

- Check cookie configuration
- Verify sliding expiration is enabled
- Check server clock sync

### Provider Says "App Not Installed"

- User is not authorized to use test app
- Add user as test user in provider settings
- Wait for app review (if in development)

---

## Admin Panel Features

### User Management

```
List All Users
├─ Username
├─ Email
├─ Provider (Google/Facebook)
├─ Created Date
├─ Last Login
├─ Blocked Status
└─ Actions:
   ├─ View Details
   ├─ Block/Unblock
   └─ Delete Account
```

### Blocking/Unblocking Users

```csharp
// Block user
user.IsBlocked = true;
await _userManager.UpdateAsync(user);

// Unblock user
user.IsBlocked = false;
await _userManager.UpdateAsync(user);
```

---

## Login Page

### Design Features

- Clean, minimal interface
- Large, accessible buttons
- Clear OAuth provider icons
- Information box about security
- Professional branding
- Mobile responsive

### User Experience

- No password field (simplifies form)
- Single click to login
- Automatic user creation
- Fast redirection to dashboard
- Remember login preference

---

## Session Security

### Cookies

- **Name**: `.AspNetCore.Identity.Application`
- **HttpOnly**: Yes (immune to XSS)
- **Secure**: Yes in production (HTTPS only)
- **SameSite**: Strict (CSRF protection)
- **Expiration**: 30 days

### CSRF Protection

- Anti-forgery tokens on all POST forms
- Verified on server before execution
- Protects against cross-site attacks

---

## References

- [Microsoft OAuth Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/)
- [Google OAuth Setup](OAUTH_SETUP.md)
- [OWASP OAuth Security](https://owasp.org/www-community/attacks/oauth)
- [RFC 6749 - OAuth 2.0](https://tools.ietf.org/html/rfc6749)

---

## Next Steps

1. ✅ Configure Google OAuth credentials
2. ✅ Configure Facebook OAuth credentials
3. ✅ Update appsettings.json
4. ✅ Run database migration
5. ✅ Test login flow
6. ✅ Verify user creation in database
7. ✅ Test logout functionality
8. ✅ Test blocked user behavior
