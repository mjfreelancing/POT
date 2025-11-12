# First-Time Configuration

> **Note:** This document will be enhanced with screenshots in a future update. The screenshots will be organized in the `Docs/first-time-setup/images/` folder.

Now that POT is running, you need to create your first user account and configure platform admin access.

## Step 1: Create Your First User Account

1. Open your browser to http://localhost:5175
2. Click **Sign Up** to create a new account
3. Fill in the registration form:
   - Username (unique)
   - Email address (must be valid - you'll receive verification code)
   - Password (must meet security requirements)
4. Click **Create Account**

## Step 2: Verify Your Email

1. Check your email inbox for a verification code
2. Enter the 6-digit code in the verification screen
3. Click **Verify**

> **Troubleshooting:** If you don't receive the email:
>
> - Check your spam/junk folder
> - Verify SMTP settings in `.env.development` are correct
> - Check server logs: `docker logs pot-aspnet`
> - Resend the verification code using the "Resend Code" button

## Step 3: Wait for Platform Admin Approval

After email verification, your account status will be **PendingApproval**. You cannot log in until a platform admin approves your account.

> **For the first user:** Since there are no platform admins yet, you need to approve yourself manually. Continue to Step 4.

## Step 4: Approve Your First User (Platform Admin Setup)

### 4.1: Get Your User GUID

Connect to the PostgreSQL database and get your user GUID:

```bash
# Connect to the database
docker exec -it pot-postgres psql -U postgres -d Pot
```

Run this query (replace `your-username` with your actual username):

```sql
SELECT "RowId", "Username", "Status" FROM "User" WHERE "Username" = 'your-username';
```

Copy the GUID from the `RowId` column. It will look like: `8826aa3a-914d-4d04-af05-fbed5b5b621f`

Exit psql:

```sql
\q
```

### 4.2: Add Yourself as Platform Admin

Open `Source/Docker/.env.development` and add your GUID to the `PLATFORM_ADMIN_USERIDS` setting:

```bash
# Platform Admin Configuration
PLATFORM_ADMIN_USERIDS=your-guid-here
```

Example:

```bash
PLATFORM_ADMIN_USERIDS=8826aa3a-914d-4d04-af05-fbed5b5b621f
```

### 4.3: Restart the API Server

```bash
docker restart pot-aspnet
```

### 4.4: Approve Your Account

1. Log back into the database:
   ```bash
   docker exec -it pot-postgres psql -U postgres -d Pot
   ```
2. Approve your account by updating the status:

   ```sql
   UPDATE "User" SET "Status" = 1 WHERE "Username" = 'your-username';
   ```

   > **Note:** Status values: `0` = PendingEmailVerification, `1` = Active, `2` = PendingApproval, `3` = Rejected

3. Exit psql:
   ```sql
   \q
   ```

## Step 5: Log In

1. Return to http://localhost:5175
2. Click **Log In**
3. Enter your username and password
4. You should now have access to the POT application with platform admin permissions

## Step 6: Approve Future Users (Platform Admin)

Once you're logged in as a platform admin, you can approve future users through the UI:

1. Navigate to **User Management** (platform admins will see this menu option)
2. View pending users
3. Approve or reject user accounts

> **For more details on platform admin permissions:** See `/Docs/DRAFT/Root/PLATFORM_ADMIN.md`

---

**Congratulations!** 🎉 POT is now fully configured and ready to use.

---

**Return to:** [Getting Started](GETTING-STARTED.md)
