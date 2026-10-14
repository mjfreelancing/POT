# Security

POT keeps each site's financial data private to that site, keeps your site administrable, and protects your account. Everything below is in place already - there is nothing to switch on or configure.

- **Isolation.** Each site's users, accounts, expenses, income and settings are separate. You cannot read or change another site's data, and a request for something outside your site is answered as though it did not exist, so nobody can use the system to find out which rows exist elsewhere. No one in another site can rename your site, change your users' roles or status, or reset their credentials.
- **Availability.** Your site cannot be left without an administrator, so there is always somebody who can manage it. The last enabled administrator cannot be disabled or demoted. Nobody outside your site can disable your users at all, and platform-level administration is limited to the administrators configured for the installation.
- **Accounts and sessions.** Passwords are stored only as salted one-way hashes, which cannot be read back. Changing your password ends every other session that is signed in. A disabled account cannot sign in. Invitations, sign-ups and password resets use single-use codes that expire and accept only a limited number of attempts. Repeated requests are throttled, and when you are throttled you are told how long to wait before trying again.
- **Not covered.** Repeated failed sign-in attempts are throttled, but an account is never locked out after a fixed number of failures.

---

[Back to User Guide](README.md)
