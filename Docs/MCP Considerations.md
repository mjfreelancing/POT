# MCP Considerations

> Status: early ideas, captured on 2026-11-04 for later conversion to a PRD. The source code is the source of truth; this reflects the code as scanned on branch `v0.1-mcp`. External facts (MCP spec, Azure pricing) were checked on 2026-11-04 from the sources listed at the end, and should be re-verified before planning.

## Goals and constraints

- Run an MCP server **locally for testing** first.
- Evaluate what a **hosted** option would require in development effort and cost before committing to it.
- The hosted option may prove too expensive. It might be offered as a **licensed option** to make it affordable.
- No PRD yet. The documentation is being cleaned up on another branch, and this document is the reference for planning later.

## What the API offers today

- **Domain:** Each site (tenant) has accounts, and each account has a balance and a reserved amount. Incomes and expenses belong to an account and repeat on a schedule: Days, Weeks, Months, Years, OneTime or EndOfMonth, with a count and an optional end date. Expenses can also accrue, which produces accrued and arrears figures. Any item can be excluded from calculations.
- **Projection engine** (`Pot.App/Features/Projections/ProjectionsService.cs`): it simulates every day from today forward. For each account it returns per-day balance, reserved, arrears, unpaid accrual, money in and out, and the individual items behind them. This is the core value, and it's exactly what an LLM can't work out by itself.
- **Renew / accrual status:** the API reports which items are past due and need renewing. Renewing moves an item's next due date forward ("I paid it").
- **Auth:** you log in with a username and password and get a 15-minute JWT. The refresh token is an HTTP-only cookie. Permissions are per resource (`account:view`, `expense:manage` and so on), and updates use etags for optimistic concurrency, which helps when an agent writes.
- **Contract:** `contracts/openapi/v1.json` is regenerated at build and committed, and has 47 endpoints.
- **Hosting** (`Docs/Azure/POT-Azure-Diagram.mmd`): Azure Container Apps (client and API containers), PostgreSQL, Key Vault with a managed identity, and Log Analytics.

## Gaps that would block or weaken an MCP server

1. **No non-interactive credential.** The cookie-based refresh flow suits browsers, not agents. MCP needs either personal access tokens (scoped and revocable, with a read-only option) or OAuth 2.1, which is what the MCP spec expects for remote servers. This is the biggest prerequisite for the hosted option. It is not needed for a local test (see [Running locally for testing](#running-locally-for-testing)).
2. **Enums come through empty in OpenAPI.** `Frequency`, `AccrualPolicy` and `RenewalMode` are `EnrichedEnum` types and appear in the contract with no values. A model or a code generator can't tell what's valid. A schema transformer would fix this cheaply.
3. **What-if changes persist.** `toggleExclude` writes to the database. An LLM exploring "what if I skip this bill" would change the user's real data. Scenarios need a projection call that takes temporary changes and saves nothing.
4. **Projection output is large for an LLM.** It's a daily array per account with item lists, so 180 days × 4 accounts is a lot of tokens to answer "am I OK?" A summary shape would be far more efficient: lowest balance and its date, first breach of reserved or zero, and totals in and out.
5. **No filtering on list endpoints.** `GET /expenses` has no "due within N days" or "for account X" filter. That's fine at personal scale, but worth noting.
6. **No record of who made a change.** If an agent writes, you'll want to know a change came from MCP, and ideally be able to preview or undo it.

## Ideas for features an LLM could support

### Answering questions (read-only, high value, low risk)

- "Am I OK for the next 3 months?": shortfall warnings, the first date an account dips below reserved, and the lowest point.
- "Why does Everyday drop on the 14th?": explain a balance change from the day's income and expense items.
- "What's due this week, and from which account?" as an upcoming-bills digest.
- "What do my recurring commitments cost per year?" This still looks forward, so it fits POT's "not a budgeting app" stance.

### Scenarios and affordability (needs the save-nothing scenario endpoint)

- "Can I afford $1,800 on 20 March?": run the projection with the purchase added and compare it to the baseline.
- Safe-to-spend: the largest one-off amount you can spend on a date without any account breaching its reserve over the horizon. The server should calculate this, not the LLM.
- Delay or shift what-ifs: an income arriving a week late, moving a bill to another account, or a contract ending early.
- Suggested fixes: "move $X from Savings to Bills before the 12th". POT has no first-class transfers, so this is also a product question.

### Guided upkeep (guarded writes)

- Renewal walkthrough: "these 4 items are past due. Did you pay them?" Confirmed items then get renewed.
- Balance reconciliation: "my bank says $4,210". The agent updates the balance and points out how far it differs from what POT expected.
- Natural-language entry: "Add Netflix $22.99 monthly from Everyday starting the 5th." The schedule rules stay on the server, and etags stop the agent overwriting newer changes.
- Bulk setup from a pasted bill list or statement. The parsing happens in the user's own LLM, so it fits POT's privacy and no-bank-integration position.
- Data hygiene: items whose end date has passed, one-off items left in the past, likely duplicates, and accounts with nothing linked to them.

### MCP-specific pieces beyond tools

- **Resources:** an account list, the user's settings, and a short "how POT models accrual and renewal" explainer so the LLM reasons correctly.
- **Prompts:** templates for a weekly check-in, an affordability check, and first-time onboarding.

### Deliberately excluded

User and role admin, approvals, site or account deletion, import, and password and signup flows. They carry high risk and gain little from an LLM.

## Where the MCP server could live

1. **Separate MCP server calling the REST API.** The API keeps doing all validation, authorization and tenancy, and the MCP server holds only task-level tools. It could use the official C# MCP SDK, which matches the existing stack. This suits a **local test or spike**. For the **hosted** option it runs into the MCP spec's token passthrough rule (see below): the MCP server may not forward the user's token to a different API, so it would need token exchange (RFC 8693), which adds complexity.
2. **An `/mcp` endpoint inside `Pot.AspNetCore` calling the app services directly.** This avoids the extra network hop and the auth relay. Because the MCP endpoint and the API are the same resource, the token passthrough problem doesn't arise. The trade-off is that MCP changes are tied to API releases and the API host gets bigger. This is **probably the simpler production shape for the hosted option**.
3. **Generate tools automatically from OpenAPI.** It's the quickest start but gives poor results: 47 low-level tools including auth and admin, and the model has to chain them. Useful at most as a prototype.

How the server runs is a separate choice: locally over stdio for testing and self-hosted Docker users, or remotely over HTTP (Streamable HTTP transport) for the Azure deployment, which needs OAuth.

**The bridge between them:** personal access tokens would let the in-process `/mcp` endpoint (option 2) be tested locally, because Claude Code can send a bearer token header to an HTTP MCP server. That means the production shape can be exercised before OAuth exists.

## Local vs hosted: effort and cost

Local testing and the hosted option share most of the work. The difference is almost entirely authentication and operations.

### Running locally for testing

A local MCP server can start with **no API changes**:

- It runs as a console process that Claude Code or Claude Desktop launches over stdio.
- It calls the existing `/api/auth/login` with a username and password held in an environment variable, keeps the refresh cookie, and calls `/api/auth/refresh` when the 15-minute JWT expires. The API sees it as just another client.
- The MCP Inspector tool lets you exercise the tools without an LLM in the loop.

The catches are acceptable for testing, not for anything wider:

- It stores a password.
- It gets every permission that user has.
- A buggy refresh loop could trip the authentication lockout policy (PRD-008).

### The hosted option: the cost is mostly development, not infrastructure

**Authentication is the big item.** For Claude.ai or ChatGPT to connect to a hosted POT, the current MCP authorization spec (reportedly the 2026-07-28 revision) requires OAuth 2.1:

- POT publishes **Protected Resource Metadata** (RFC 9728) so clients can discover the authorization server, through a `resource_metadata` URL in the 401 `WWW-Authenticate` header or a well-known path.
- Login uses **authorization code with PKCE**, plus a consent screen.
- Clients send the **resource indicator** (RFC 8707) so tokens are bound to the MCP server as their audience.
- Clients register through a **Client ID Metadata Document (CIMD)**, which the latest revision reportedly prefers over Dynamic Client Registration (DCR). The reported order of preference is a pre-registered client ID, then CIMD, then DCR.
- **Token passthrough is forbidden.** The MCP server must not accept a token that wasn't issued for it and forward it downstream. This is what makes option 2 in [Where the MCP server could live](#where-the-mcp-server-could-live) attractive for hosting.

These details come from secondary write-ups, not the spec itself, so check them against modelcontextprotocol.io before relying on them.

There are two ways to provide the authorization server:

- **Self-hosted with OpenIddict:** no licence fee, but you build, secure and maintain it.
- **A managed identity provider:** less to build, but possibly per-user charges. Compare free tiers and pricing at planning time.

**Infrastructure cost looks small.**

- It's one more route on the existing Container Apps setup (option 2), or one small extra container (option 1).
- The Container Apps consumption plan includes a free grant of **180,000 vCPU-seconds, 360,000 GiB-seconds and 2 million requests per subscription per calendar month**. Only requests from outside the Container Apps environment are billable, and health probes are excluded. A third-party guide quoted about $0.000024 per active vCPU-second and $0.40 per million requests (East US, September 2026). Check the live Azure pricing page for current rates in your region.
- **Trap:** long-lived streaming connections can stop the app scaling to zero. Run the MCP endpoint stateless.

**LLM token costs aren't yours.** The user's Claude or ChatGPT subscription pays for the model. That changes only if POT itself ever calls an LLM, for example an in-app chat. That is the point where per-use costs land on you and licensing becomes necessary rather than optional.

**What does cost you:**

- Building the authentication.
- Keeping up with the MCP spec, which has been revised several times.
- A security review, since this is financial data with a new way in.
- Abuse and rate limiting for agent traffic.
- Testing compatibility with each client (Claude, ChatGPT, VS Code and others).
- Updating the privacy policy, because users' financial data would now flow to third-party LLM providers. That cuts against POT's current privacy pitch, so it needs careful wording and explicit user opt-in.

### Licensing

- There's **no entitlement or tier concept in the code today**.
- The minimum would be a per-site "MCP enabled" flag, checked when issuing tokens and at the `/mcp` endpoint. That part is small.
- The expensive part is **billing integration**.
- Someone has to grant licences, which overlaps with **PRD-027 (platform administration, currently blocked)**.

### Rough relative sizing

| Work item | Local test | Hosted | Size |
|---|---|---|---|
| MCP tools (read-only, curated) | ✓ | ✓ | M |
| Projection summary endpoint | optional at first | ✓ | S |
| Enum values in OpenAPI | – | ✓ | S |
| Save-nothing scenario projection | later | later | M–L |
| Login-based stdio wrapper | ✓ | – | S |
| Personal access tokens | – | ✓ (bridge, power users) | M |
| OAuth 2.1 to MCP spec | – | ✓ | **L** (largest) |
| Deployment, rate limits, monitoring | – | ✓ | S–M |
| Agent attribution / audit | – | ✓ (before writes) | M |
| Entitlement flag / billing | – | if licensed | S / L |

These are relative sizes, not estimates.

## A possible order

0. **Spike (throwaway):** see [Suggested first step](#suggested-first-step-a-throwaway-spike). This decides whether the rest is worth doing.
1. **API prerequisites:** enum values in OpenAPI, better field descriptions, a projection summary endpoint, and personal access tokens.
2. **Read-only MCP:** an in-process `/mcp` endpoint with projection summary, upcoming bills, renewals due, and the account, income and expense lists, plus resources and prompts. Tested locally with a personal access token.
3. **Scenario engine:** save-nothing what-if projections and safe-to-spend.
4. **Hosted:** OAuth 2.1 to the MCP spec, deployment, rate limiting, privacy policy update, and an entitlement flag if it is licensed.
5. **Guarded writes:** preview then confirm, agent attribution, and an undo for recent agent changes.
6. **Later:** link to the settlement ledger in PRD-024 (budget vs actual), and add proactive briefings alongside the existing budget reminder email.

## Suggested first step: a throwaway spike

Build a local stdio MCP server that uses the existing login and offers 3–4 read-only tools:

- list accounts
- upcoming bills
- renewals due
- a projection summary, worked out inside the spike from the existing `/api/projections` endpoint

Then use it from Claude for a week. That tells you cheaply whether the conversational experience is worth the hosted investment, before you commit to OAuth. The spike code is labelled throwaway and is not the production shape.

## Open questions

- **Who is the hosted option for?** Local testing is decided. Whether hosted goes ahead depends on development effort and cost, and the spike should inform that.
- **Licensing:** free, included in a paid tier, or a separate add-on? If billing is needed, what's the mechanism, and does it wait on PRD-027?
- **Authorization server:** self-hosted with OpenIddict, or a managed identity provider?
- **Privacy position:** how to word explicit opt-in for sending financial data to third-party LLM providers.
- **Transfers:** should POT model transfers between accounts as a first-class concept, given the "suggested fixes" idea?
- **In-app LLM:** is POT ever likely to call an LLM itself? If so, per-use costs make licensing necessary.

## Sources

Checked 2026-11-04. The MCP authorization details come from secondary sources and need verifying against the official spec at modelcontextprotocol.io.

- [MCP OAuth: how MCP authentication works (Scalar)](https://scalar.com/learn/mcp/mcp-oauth)
- [MCP authorization OAuth (Tyk)](https://tyk.io/learning-center/mcp-authorization-oauth/)
- [MCP authentication (NeuralTrust)](https://neuraltrust.ai/blog/mcp-authentication)
- [MCP Authentication: 2026-07-28 Authorization Spec (SSOJet)](https://ssojet.com/blog/mcp-authentication-oauth-tokens-security-ai-connections.md)
- [CIMD support for MCP client registration (Scalekit)](https://www.scalekit.com/blog/mcp-authorization-guide)
- [Azure Container Apps pricing](https://azure.microsoft.com/en-au/pricing/details/container-apps/)
- [Azure Container Apps billing (Microsoft docs)](https://docs.azure.cn/en-us/container-apps/billing)
- [Azure Container Apps pricing guide (DevZero)](https://www.devzero.io/blog/azure-container-apps-pricing)
