# TeslaStarter Authentication Setup

## Configuration

1. **Update Descope Project ID**: 
   - Edit `wwwroot/config.js`
   - Replace `YOUR_DESCOPE_PROJECT_ID` with your actual Descope Project ID

2. **Update API Configuration**:
   - Edit `appsettings.json` or `appsettings.local.json`
   - Add your Descope configuration:
   ```json
   {
     "Descope": {
       "ProjectId": "YOUR_DESCOPE_PROJECT_ID",
       "ManagementKey": "YOUR_MANAGEMENT_KEY"
     }
   }
   ```

## Testing the Authentication Flow

1. **Start the API**:
   ```bash
   dotnet run
   ```

2. **Access the Test Page**:
   - Navigate to `https://localhost:5001/` (or your configured port)
   - You'll see the authentication test page

3. **Sign In**:
   - Use the Descope sign-in flow to authenticate
   - After successful authentication, you'll see the authenticated view

4. **Test Authenticated Endpoints**:
   - Click "Get Current User" to fetch your user profile
   - Click "Get All Users" to test the authenticated endpoint

## API Endpoints

- `GET /api/auth/me` - Get current authenticated user
- `GET /api/auth/users` - Get all users (requires authentication)
- `GET /api/auth/tesla/authorize` - Initialize Tesla OAuth flow
- `POST /api/auth/tesla/callback` - Handle Tesla OAuth callback
- `POST /api/auth/tesla/refresh` - Refresh Tesla tokens
- `DELETE /api/auth/tesla/unlink` - Unlink Tesla account

## Authentication Flow

1. User authenticates with Descope
2. Descope returns a session token
3. Frontend includes the token in the Authorization header: `Bearer <token>`
4. API validates the token with Descope
5. User is automatically created/synced in the database on first authentication