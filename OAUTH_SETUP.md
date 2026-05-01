# OAuth Authentication Setup Guide

## Overview

This Inventory Management System uses **OAuth 2.0** for secure authentication with Google and Facebook social login providers. Users authenticate without passwords, relying entirely on their existing social accounts.

---

## Prerequisites

- ASP.NET Core 8.0
- SQL Server (with database already created)
- Google Developer Account
- Facebook Developer Account
- localhost running on HTTPS (https://localhost:5001 or similar)

---

## Step 1: Google OAuth Setup

### 1.1 Create Google Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Click **Create Project**
3. Enter project name: `Inventory Management System`
4. Click **Create**
5. Wait for project to be created

### 1.2 Enable Google+ API

1. In the search bar, type `Google+ API`
2. Click on it and press **Enable**
3. This may take a few minutes

### 1.3 Create OAuth 2.0 Credentials

1. Go to **Credentials** (left sidebar)
2. Click **+ Create Credentials** → **OAuth Client ID**
3. If prompted to create a consent screen first:
   - Choose **External** user type
   - Click **Create**
   - Fill in:
     - **App name**: Inventory Management System
     - **User support email**: your-email@gmail.com
     - **Developer contact**: your-email@gmail.com
   - Click **Save and Continue**
   - Skip scopes (continue)
   - Skip test users (continue)
   - Review and go back

4. Now create OAuth Client ID:
   - Application type: **Web application**
   - Name: `Local Development`
   - **Authorized redirect URIs** - Add these:
     ```
     https://localhost:5001/signin-google
     https://localhost:7001/signin-google
     http://localhost:5000/signin-google
     ```
   - Click **Create**

5. Copy the **Client ID** and **Client Secret**

### 1.4 Add to appsettings.json

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID_HERE",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET_HERE"
    }
  }
}
```

---

## Step 2: Facebook OAuth Setup

### 2.1 Create Facebook App

1. Go to [Facebook Developers](https://developers.facebook.com/)
2. Click **My Apps** → **Create App**
3. Choose type: **Consumer**
4. Fill in:
   - **App Name**: Inventory Management System
   - **App Contact Email**: your-email@facebook.com
   - **App Purpose**: Select appropriate category
5. Click **Create App**

### 2.2 Add Facebook Login Product

1. In your app dashboard, click **+ Add Product**
2. Search for **Facebook Login**
3. Click **Set Up**
4. Choose platform: **Web**
5. Skip the Quick Start

### 2.3 Configure Facebook Login Settings

1. Go to **Settings** → **Basic**
   - Copy **App ID** and **App Secret**

2. Go to **Facebook Login** → **Settings**
   - **Valid OAuth Redirect URIs** - Add these:
     ```
     https://localhost:5001/signin-facebook
     https://localhost:7001/signin-facebook
     http://localhost:5000/signin-facebook
     ```
   - Click **Save Changes**

3. Go to **Settings** → **App Roles** and create a test user

### 2.4 Add to appsettings.json

```json
{
  "Authentication": {
    "Facebook": {
      "ClientId": "YOUR_FACEBOOK_CLIENT_ID_HERE",
      "ClientSecret": "YOUR_FACEBOOK_CLIENT_SECRET_HERE"
    }
  }
}
```

---

## Step 3: Application Configuration

### 3.1 Update appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SOJEEB\\SQLEXPRESS;Database=InventoryManagementSystem;Trusted_Connection=true;TrustServerCertificate=true;Encrypt=false;"
  },
  "Authentication": {
    "Google": {
      "ClientId": "xxxxx-xxxxx.apps.googleusercontent.com",
      "ClientSecret": "GOCSPX-xxxxxxxxxxxxx"
    },
    "Facebook": {
      "ClientId": "123456789012345",
      "ClientSecret": "abcdef1234567890abcdef"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 3.2 Update Redirect URLs for Production

When deploying to production, add your domain:
- Google: `https://yourdomain.com/signin-google`
- Facebook: `https://yourdomain.com/signin-facebook`

---

## Step 4: Database Migration

Update the database to include OAuth fields:

```powershell
dotnet ef migrations add AddOAuthFields
dotnet ef database update
```

This migration adds:
- `Provider` - "Google" or "Facebook"
- `ProviderUserId` - Unique ID from provider
- `ProfilePictureUrl` - User's profile picture
- `LastLoginAt` - Track login history

---

## Step 5: Testing

### 5.1 Run Application

```powershell
dotnet run
```

### 5.2 Test Login Flow

1. Navigate to `https://localhost:5001/Account/Login`
2. Click **Login with Google**
   - Select a Google account
   - Grant permissions
   - Should redirect back and create user
3. Click **Login with Facebook**
   - Select a Facebook account
   - Grant permissions
   - Should redirect back and create/login user

### 5.3 Verify User Creation

Users should appear in database with:
- Email from provider
- Name from provider
- Provider name (Google/Facebook)
- ProviderUserId from provider
- ProfilePictureUrl

---

## Security Features Implemented

✅ **OAuth 2.0 Secure Flow**
- No passwords stored
- Provider validates credentials
- Secure token exchange

✅ **Session Management**
- Secure cookie-based sessions
- Automatic logout
- HTTPS-only cookies in production

✅ **User Blocking**
- Admins can block users
- Blocked users cannot login
- Automatic session invalidation

✅ **Account Security**
- Unique email requirement
- External login validation
- Provider user ID verification

---

## Troubleshooting

### "Redirect URI mismatch"
- **Issue**: OAuth provider doesn't recognize redirect URL
- **Solution**: Ensure redirect URLs in provider settings match exactly (including https/http)

### "Application not installed" (Facebook)
- **Issue**: App is in development and user isn't a tester
- **Solution**: Add user as Test User in Facebook app roles

### "Error loading external login information"
- **Issue**: Session expired during OAuth flow
- **Solution**: User should try logging in again

### User email conflicts
- **Issue**: Same email exists for Google and Facebook users
- **Solution**: System automatically adds suffix to make unique (user@example.com_1)

---

## User Data Collected

From **Google**:
- Email
- Given Name (First Name)
- Family Name (Last Name)
- Picture URL

From **Facebook**:
- Email
- Name
- Picture URL

This data is used to:
- Create user profile
- Display user name in interface
- Show user avatar
- Track login history

---

## Advanced: Linking Accounts

To implement account linking (merge Google and Facebook accounts for same user):

```csharp
// Example: Link Facebook account to Google-authenticated user
var currentUser = await _userManager.GetUserAsync(User);
var facebookInfo = await _signInManager.GetExternalLoginInfoAsync();
await _userManager.AddLoginAsync(currentUser, facebookInfo);
```

---

## Production Deployment

### Before Going Live:

1. ✅ Update redirect URLs to production domain
2. ✅ Use environment variables for secrets (not appsettings.json)
3. ✅ Enable HTTPS everywhere
4. ✅ Set secure cookies (HttpOnly, SameSite=Strict)
5. ✅ Configure CORS properly
6. ✅ Set up logging/monitoring
7. ✅ Test with production OAuth apps

### Environment Variables (Recommended)

```bash
# Linux/Mac
export AUTH_GOOGLE_CLIENTID="xxxxx"
export AUTH_GOOGLE_SECRET="xxxxx"
export AUTH_FACEBOOK_CLIENTID="xxxxx"
export AUTH_FACEBOOK_SECRET="xxxxx"

# Windows PowerShell
$env:AUTH_GOOGLE_CLIENTID="xxxxx"
$env:AUTH_GOOGLE_SECRET="xxxxx"
```

Then read in code:

```csharp
var googleClientId = builder.Configuration["AUTH_GOOGLE_CLIENTID"];
```

---

## References

- [Microsoft OAuth Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/)
- [Google OAuth Documentation](https://developers.google.com/identity/protocols/oauth2)
- [Facebook Login Documentation](https://developers.facebook.com/docs/facebook-login/)
- [OWASP OAuth Security](https://owasp.org/www-community/attacks/oauth)
