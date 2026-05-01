# 📋 OAuth Configuration Checklist

## Pre-Configuration

- [ ] Project builds successfully (`dotnet build`)
- [ ] Database is running (SQL Server)
- [ ] Connection string is configured
- [ ] Previous migrations applied (`dotnet ef database update`)

---

## Google OAuth Setup

### Google Cloud Console

- [ ] Created Google Cloud project
- [ ] Enabled Google+ API
- [ ] Created OAuth 2.0 credentials (Web Application)
- [ ] Project name: "Inventory Management System"
- [ ] Redirect URIs added:
  - [ ] `https://localhost:5001/signin-google`
  - [ ] `https://localhost:7001/signin-google`
  - [ ] `http://localhost:5000/signin-google`
  - [ ] Production domain (when ready)

### Credentials

- [ ] Copied Client ID
- [ ] Copied Client Secret
- [ ] Credentials file saved securely

### Verification

- [ ] No typos in Client ID
- [ ] No typos in Client Secret
- [ ] Redirect URLs match exactly (including https/http)
- [ ] Consent screen configured

---

## Facebook OAuth Setup

### Facebook Developers

- [ ] Created Facebook app
- [ ] App name: "Inventory Management System"
- [ ] Added Facebook Login product
- [ ] Configured OAuth redirect URIs:
  - [ ] `https://localhost:5001/signin-facebook`
  - [ ] `https://localhost:7001/signin-facebook`
  - [ ] `http://localhost:5000/signin-facebook`
  - [ ] Production domain (when ready)

### Credentials

- [ ] Copied App ID
- [ ] Copied App Secret
- [ ] Credentials file saved securely

### Verification

- [ ] No typos in App ID
- [ ] No typos in App Secret
- [ ] Redirect URLs match exactly
- [ ] App is visible to your test account

### Test Account

- [ ] Created test user in Facebook app
- [ ] Test account can access app

---

## Application Configuration

### appsettings.json

- [ ] Updated Google ClientId
- [ ] Updated Google ClientSecret
- [ ] Updated Facebook ClientId (App ID)
- [ ] Updated Facebook ClientSecret (App Secret)
- [ ] Verified JSON syntax (valid format)
- [ ] Saved file

### appsettings.Development.json (Optional)

- [ ] Created or updated development-specific settings
- [ ] Uses same OAuth values as appsettings.json

### Environment Variables (Optional)

- [ ] Set `AUTH_GOOGLE_CLIENTID` environment variable
- [ ] Set `AUTH_GOOGLE_SECRET` environment variable
- [ ] Set `AUTH_FACEBOOK_CLIENTID` environment variable
- [ ] Set `AUTH_FACEBOOK_SECRET` environment variable
- [ ] Verified variables are accessible (`echo %VAR_NAME%` or `$env:VAR_NAME`)

---

## Database Preparation

### Migration Creation

- [ ] Ran `dotnet ef migrations add AddOAuthFields`
- [ ] Migration file created with OAuth fields
- [ ] Review migration file for correctness:
  - [ ] `Provider` column added
  - [ ] `ProviderUserId` column added
  - [ ] `ProfilePictureUrl` column added
  - [ ] `LastLoginAt` column added
  - [ ] Indexes created

### Migration Application

- [ ] Ran `dotnet ef database update`
- [ ] Migration applied successfully
- [ ] No migration errors
- [ ] Database schema updated

### Database Verification

- [ ] Opened SQL Server Management Studio
- [ ] Navigated to `InventoryManagementSystem` database
- [ ] Checked `AspNetUsers` table has new columns:
  - [ ] `Provider` (nvarchar(50))
  - [ ] `ProviderUserId` (nvarchar(450))
  - [ ] `ProfilePictureUrl` (nvarchar(max))
  - [ ] `LastLoginAt` (datetime2)
- [ ] Verified indexes were created

---

## NuGet Packages

### Verification

- [ ] `Microsoft.AspNetCore.Authentication.Google 8.0.26` installed
- [ ] `Microsoft.AspNetCore.Authentication.Facebook 8.0.26` installed
- [ ] Check with: `dotnet package search authentication`

### Build

- [ ] Ran `dotnet restore`
- [ ] Ran `dotnet build`
- [ ] Build successful (no errors)
- [ ] Build warnings reviewed

---

## Code Files

### New Files Created

- [ ] `Controllers/AccountController.cs` exists
- [ ] `Views/Account/Login.cshtml` exists
- [ ] `Views/Account/BlockedUser.cshtml` exists
- [ ] `Middleware/BlockedUserMiddleware.cs` exists
- [ ] `wwwroot/css/login.css` exists

### Modified Files Updated

- [ ] `Models/ApplicationUser.cs` has OAuth fields
- [ ] `Program.cs` has OAuth configuration
- [ ] `appsettings.json` has Authentication section
- [ ] `.csproj` has OAuth NuGet packages

### Verification

- [ ] No compilation errors
- [ ] No compilation warnings
- [ ] Intellisense working properly
- [ ] All using statements present

---

## Application Launch

### Pre-Launch

- [ ] SQL Server Express is running
- [ ] Database is accessible
- [ ] No other applications using port 5001
- [ ] HTTPS certificate trusted (localhost)

### Launch

- [ ] Ran `dotnet run`
- [ ] Application started successfully
- [ ] No runtime errors
- [ ] Console shows "Now listening on: https://localhost:5001"

### Post-Launch

- [ ] Application accessible at `https://localhost:5001`
- [ ] Navigation to `/Account/Login` works
- [ ] Login page displays correctly
- [ ] Google button visible and styled
- [ ] Facebook button visible and styled
- [ ] Mobile responsive (test with dev tools)

---

## Login Testing

### Google Login Test

- [ ] Navigate to login page
- [ ] Click "Login with Google"
- [ ] Redirected to Google
- [ ] Selected Google account
- [ ] Granted permissions
- [ ] Redirected back to application
- [ ] Logged in successfully
- [ ] Dashboard displayed
- [ ] User data visible in database

### Facebook Login Test

- [ ] Logged out from previous test
- [ ] Navigate to login page
- [ ] Click "Login with Facebook"
- [ ] Redirected to Facebook
- [ ] Selected Facebook account
- [ ] Granted permissions
- [ ] Redirected back to application
- [ ] Logged in successfully
- [ ] Dashboard displayed
- [ ] User data visible in database

### Logout Test

- [ ] Click logout button
- [ ] Redirected to home page
- [ ] Session cleared
- [ ] Cannot access protected pages
- [ ] Must login again

### Returning User Test

- [ ] Logout
- [ ] Login with same provider
- [ ] User recognized
- [ ] `LastLoginAt` updated
- [ ] All data preserved

---

## Database Verification After Testing

### User Data

- [ ] New users created in `AspNetUsers` table
- [ ] `Provider` field populated ("Google" or "Facebook")
- [ ] `ProviderUserId` field populated
- [ ] `Email` field populated
- [ ] `FirstName` and `LastName` populated
- [ ] `ProfilePictureUrl` populated
- [ ] `CreatedAt` timestamp correct
- [ ] `LastLoginAt` updated

### User Roles

- [ ] New users assigned "User" role
- [ ] Check `AspNetUserRoles` table
- [ ] Entry exists for each user

### External Logins

- [ ] Check `AspNetUserLogins` table
- [ ] Entry exists with Provider and ProviderKey
- [ ] Matches `ProviderUserId` in ApplicationUser

---

## Security Verification

### HTTPS

- [ ] Application uses HTTPS
- [ ] Login page is HTTPS
- [ ] OAuth redirects are HTTPS
- [ ] Certificate warning appears only in dev

### Cookies

- [ ] Authentication cookie created after login
- [ ] Cookie name contains "Identity"
- [ ] Cookie is HttpOnly (not accessible from JS)
- [ ] Cookie is Secure (HTTPS only in production)

### Session

- [ ] Session persists after page refresh
- [ ] Session expires after timeout
- [ ] Logout clears session
- [ ] Can't access protected routes after logout

### CSRF Protection

- [ ] Forms have anti-forgery tokens
- [ ] POST requests require tokens
- [ ] Invalid tokens rejected

---

## Blocking Feature Test

### Admin Blocking

- [ ] Login as admin user
- [ ] Access admin panel
- [ ] Find test user
- [ ] Block user
- [ ] `IsBlocked` field set to true

### Blocked User Behavior

- [ ] Logout blocked user
- [ ] Try to login again
- [ ] Login appears to work
- [ ] Redirected to blocked page
- [ ] Cannot access any protected routes
- [ ] Session invalidated

### Unblocking

- [ ] Admin unblocks user
- [ ] `IsBlocked` field set to false
- [ ] User can login again
- [ ] Can access all features

---

## Error Handling

### Missing Credentials

- [ ] Comment out OAuth credentials
- [ ] Application fails to start with helpful error message
- [ ] Error message specifies which credential is missing

### Invalid Redirect URI

- [ ] Test with wrong redirect URI
- [ ] OAuth provider rejects
- [ ] Error message displayed in browser

### Provider Down

- [ ] If provider is temporarily down
- [ ] User sees error message
- [ ] Error is logged
- [ ] Application doesn't crash

---

## Performance Testing

### Login Speed

- [ ] Google login: < 5 seconds
- [ ] Facebook login: < 5 seconds
- [ ] Session validation: < 100ms per request

### Database Performance

- [ ] User lookup: < 10ms
- [ ] User creation: < 50ms
- [ ] No N+1 queries

### Page Load

- [ ] Login page: < 2 seconds
- [ ] Dashboard: < 2 seconds on first load
- [ ] Mobile: acceptable on 4G

---

## Mobile Testing

### Responsive Design

- [ ] Login page responsive on mobile
- [ ] Buttons are touch-friendly (44px minimum)
- [ ] Text is readable without zooming
- [ ] Forms are usable on small screens

### Mobile Browsers

- [ ] iOS Safari works
- [ ] Chrome Android works
- [ ] Firefox Android works
- [ ] Samsung Internet works

---

## Production Preparation

### Before Deployment

- [ ] Updated production domain in OAuth provider settings
- [ ] Removed development redirect URIs
- [ ] Added production redirect URIs
- [ ] HTTPS certificate configured for production domain
- [ ] Environment variables set on production server
- [ ] Credentials NOT hardcoded in production config
- [ ] Security headers configured
- [ ] CORS policies configured

### Production OAuth URLs

- [ ] Google: `https://yourdomain.com/signin-google`
- [ ] Facebook: `https://yourdomain.com/signin-facebook`
- [ ] Exact domain verified in provider settings
- [ ] Tested against production domain

### Security Headers

- [ ] Content-Security-Policy configured
- [ ] X-Frame-Options set
- [ ] X-Content-Type-Options set
- [ ] Strict-Transport-Security set

---

## Documentation

### Verify Documentation

- [ ] [OAUTH_SETUP.md](OAUTH_SETUP.md) complete
- [ ] [AUTHENTICATION.md](AUTHENTICATION.md) complete
- [ ] [OAUTH_IMPLEMENTATION.md](OAUTH_IMPLEMENTATION.md) complete
- [ ] [OAUTH_READY.md](OAUTH_READY.md) complete
- [ ] [README.md](README.md) updated with OAuth info
- [ ] [QUICKSTART.md](QUICKSTART.md) updated

### Documentation Quality

- [ ] Clear instructions provided
- [ ] Troubleshooting section complete
- [ ] Code examples included
- [ ] Security best practices documented
- [ ] References provided

---

## Final Sign-Off

### Overall Status

- [ ] All tests passed
- [ ] No critical issues
- [ ] Security verified
- [ ] Performance acceptable
- [ ] Documentation complete

### Ready for Deployment?

- [ ] YES, all items checked
- [ ] NO, items remain (list below):

---

## Notes

Use this space for any additional notes or issues encountered:

```
[Your notes here]
```

---

## Sign-Off

- **Configuration Date**: _______________
- **Tester Name**: _______________
- **Status**: ⬜ Not Started | 🟡 In Progress | ✅ Complete
- **Issues Found**: None / [List below]

---

**Remember**: Keep this checklist as a record of your OAuth setup!
