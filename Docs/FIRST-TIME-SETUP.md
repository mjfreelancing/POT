# First-Time Configuration

Now that POT is running, you need to create your first user account and configure platform admin access.

## Step 1: Create Your First User Account

1. Open your browser to http://localhost:5175
2. Click **Sign Up** to create a new account

   ![Click Sign Up](images/first-time-setup/step01-01-select-sign-up.png)

3. Fill in the registration form:

   - Username (must be globally unique)
   - Email address (must be valid - you'll receive verification code)
   - Password

   ![Enter account details](images/first-time-setup/step01-02-create-account.png)

4. Click **Send Verification Code**

## Step 2: Verify Your Email

1. Check your email inbox for a verification code

   ![Verification code screen](images/first-time-setup/step02-01-wait-for-email-with-codes.png)

   The email will contain a verification code and a temporary password:

   ![Email with verification code](images/first-time-setup/step02-02-email-verification.png)

   > **Important:** Save the temporary password shown in the email - you'll need it to log in after approval. If you lose it, you'll need to use the "Forgot your password?" flow to reset it.

2. Enter the 6-digit code in the verification screen

   ![Enter verification code](images/first-time-setup/step02-03-enter-verification-code.png)

3. Click **Verify Code**

   ![Account pending approval](images/first-time-setup/step02-04-pending-approval.png)

## Step 3: Wait for Platform Admin Approval

After email verification, your account status will be **PendingApproval**. You cannot log in until a platform admin approves your account.

> **For the first user:** Since there are no platform admins yet, you need to approve yourself manually. Continue to Step 4.

## Step 4: Approve Your First User (Platform Admin Setup)

### 4.1: Get Your User GUID

Connect to your PostgreSQL database and run the following query:

```sql
SELECT "RowId", "Username", "Status" FROM "User";
```

You should see a result similar to:

| RowId                                | Username  | Status   |
| ------------------------------------ | --------- | -------- |
| 7e9a1695-b6ad-45fe-811a-de61d8c68ef9 | demo-user | Approval |

Copy the GUID from the `RowId` column (e.g., `7e9a1695-b6ad-45fe-811a-de61d8c68ef9`).

> **Note:** Since this is the first user, there will only be one row in the result.

### 4.2: Add Yourself as Platform Admin

Add your GUID to the platform admin configuration:

**If using Docker:**

Open `Source/Docker/.env.development` and add your GUID to the `PLATFORM_ADMIN_USERIDS` setting:

```bash
# Platform Admin Configuration
PLATFORM_ADMIN_USERIDS=7e9a1695-b6ad-45fe-811a-de61d8c68ef9
```

**If running locally (Visual Studio):**

Open `Source/Server/Pot.AspNetCore/appsettings.Development.json` and add your GUID to the `PlatformAdmin:UserIds` setting:

```json
"PlatformAdmin": {
  "UserIds": "7e9a1695-b6ad-45fe-811a-de61d8c68ef9"
}
```

> **Note:** Replace the example GUID with your actual GUID from Step 4.1.

### 4.3: Restart the API Server

**If using Docker:**

```bash
docker restart pot-aspnet
```

**If running locally (Visual Studio):**

Stop and restart the API application in Visual Studio (or restart `dotnet run` if using the command line).

### 4.4: Approve Your Account

Connect to your PostgreSQL database and approve your account by updating the status:

```sql
UPDATE "User" SET "Status" = 'Enabled' WHERE "RowId" = '7e9a1695-b6ad-45fe-811a-de61d8c68ef9';
```

> **Note:** Replace the GUID with your actual GUID from Step 4.1.

Verify the update by running:

```sql
SELECT "RowId", "Username", "Status" FROM "User";
```

You should see the status changed to `Enabled`:

| RowId                                | Username  | Status  |
| ------------------------------------ | --------- | ------- |
| 7e9a1695-b6ad-45fe-811a-de61d8c68ef9 | demo-user | Enabled |

## Step 5: Log In and Change Password

1. Return to http://localhost:5175
2. Click **Log In**
3. Enter your username and the temporary password from the email
4. You should now have access to the POT application with platform admin permissions

   ![Logged in dashboard](images/first-time-setup/step05-01-logged-in.png)

5. **Change your password immediately** for security:

   - Click on your username in the top-right corner

     ![Select username](images/first-time-setup/step05-02-select-username.png)

   - Select **Settings** from the dropdown menu

     ![Select Settings](images/first-time-setup/step05-03-select-settings.png)

   - (Optional) Update your **Display Name** in the User Details section if desired, then click **Update User Details**

     ![Update display name](images/first-time-setup/step05-04-change-display-name.png)

   - Expand the **Change Password** section
   - Enter your temporary password in **Current Password**
   - Enter and confirm your new secure password
   - Click **Change Password**

     ![Change password form](images/first-time-setup/step05-05-change-password.png)

   - A success notification will appear confirming the password change

     ![Password changed success](images/first-time-setup/step05-06-success-message.png)

6. **Verify platform admin access:**

   - Navigate to **Users** in the left sidebar (only visible to platform admins)
   - You should see your account listed with "Admin" role and "Enabled" status

     ![User management view](images/first-time-setup/step06-01-users.png)

---

**Congratulations!** 🎉 POT is now fully configured and ready to use.

---

**Return to:** [Getting Started](GETTING-STARTED.md)
