# PRD.md — Video Link Vault (Capstone)

_Drafted Sep 10 2026. Today's date and deadline both anchor this doc: work begins Sep 10, full capstone due **Mon Oct 5 2026** (25 days). Supersedes the mini-project `PRD.md` this repo was seeded from — that document is preserved in git history and remains the accurate as-built record of the mini project itself; this one describes the target state of the capstone build. Where a capstone decision reopens or changes a mini-project decision, that's called out explicitly rather than silently overwritten — see `memory.md` for the full reasoning behind each one._

## Purpose

A cloud-native, multi-user version of Video Link Vault: users sign in with their own account (no more hardcoded demo credentials), save/browse/filter/delete video links from TikTok, YouTube, Instagram, and Facebook, and organize them by category and free-form tags — same product as the mini project, now backed by a real database, real identity, and a real Azure deployment, with a subscription tier and an admin view layered on top. Built to demonstrate production Azure patterns (managed identity, Key Vault, API gateway, CI/CD) to recruiters, not just CRUD.

## Language, Framework, Version, App Type

- **Backend:** C#, .NET 10 (LTS), ASP.NET Core Web API (controller-based)
- **Frontend:** JavaScript/React 19 (Vite), deployed to Azure Static Web Apps
- **Mobile (stretch):** React Native (Expo), separate codebase, same backend
- **Database:** Azure Database for PostgreSQL – Flexible Server, EF Core + Npgsql
- **Identity:** Microsoft Entra External ID (external tenant)
- **App type:** Cloud-hosted web application (REST API + SPA client), single Azure region, with an optional mobile client

## Architecture (target state)

    React Web App (Azure Static Web Apps)   \
                                               -->  API Management (Consumption)  -->  ASP.NET Core API (Azure App Service)  -->  Azure Database for PostgreSQL
    React Native mobile app — stretch goal  /              ^                                    |         ^
                                                             |                                    |         |
                                                    validate-jwt / rate-limit                Managed Identity   Managed Identity
                                                    (Entra External ID tokens)             (no secrets)     (token as password)
                                                                                                    |
                                                                                              Azure Key Vault
                                                                                          (Stripe keys, webhook secret)

**Identity — Microsoft Entra External ID, not roll-your-own.** Azure AD B2C has been closed to new customers since May 1, 2025, so it was never an option for a project starting now. Entra External ID is the direct replacement: a separate external tenant, self-service email signup, first 50,000 monthly active users free. This **replaces** the mini project's `AuthController`, `UserStore`'s credential role, `IPasswordHasher<User>`, and the `PasswordHash`/`LoginRequest`/`RegisterRequest` DTOs — those are removed, not extended. Identity for a request comes from a validated JWT's `oid` claim, not a session built on a hashed password.

A local `users` table is kept, but its job changes: it's a **profile row keyed by the Entra object ID** (email, display name, Stripe customer ID, subscription status, admin flag), not a credential store. The first authenticated request from an `oid` with no matching row creates one — just-in-time provisioning. `links.user_id` continues to point at this table's `Id`.

One direct, positive side effect: `{userId}` disappears from the API routes. `GET/POST /api/links`, `DELETE /api/links/{id}` — the owner comes from the validated token, not a route parameter a client could substitute. This closes the mini project's documented, accepted limitation ("any client that knows a GUID can read or delete that account's links").

**Database access — passwordless, via managed identity.** App Service gets a system-assigned managed identity. `Azure.Identity`'s `DefaultAzureCredential` requests a token for scope `https://ossrdbms-aad.database.windows.net/.default`; Npgsql's `UsePeriodicPasswordProvider` on the `NpgsqlDataSourceBuilder` uses that token as the connection password and handles refresh/caching automatically. There is no database password anywhere — not in `appsettings.json`, not in Key Vault, not in a GitHub secret. Local development keeps a real password (Docker Postgres / .NET user-secrets); the code attaches the token provider only when the configured connection string has no password, so one codebase serves both environments without an `if (isProduction)` branch scattered through it. The Postgres side needs a database role created for the managed identity with the right grants — a manual, portal-driven step to do once, deliberately, not on deploy day.

**Secrets — Key Vault holds only what's actually secret.** With the database password eliminated, Key Vault's job is the Stripe secret key and the Stripe webhook signing secret. App Service reads them via its managed identity (`DefaultAzureCredential` again, same mechanism, different scope) — no secret ever sits in App Service configuration in plaintext.

**Gateway — API Management (Consumption tier), not Front Door.** APIM is the single front door for both the web app and (eventually) the mobile app: `validate-jwt` verifies the Entra token's signature and issuer before a request ever reaches the API, and `rate-limit-by-key`/`quota-by-key` policies provide Layer-7 abuse protection. Consumption tier includes the first million operations/month at no fixed cost; the tradeoff is a multi-second cold start on the first request after idle, which needs a warm-up call before any live demo.

**Front Door — deliberately out of the MVP, considered and rejected for this shape, not just deferred.** Azure's infrastructure-level DDoS protection (Layer 3/4) is already on by default and free for every public IP, App Service and APIM included — no action needed. Azure's paid DDoS tiers (Network Protection, ~$2,944/mo; IP Protection, ~$199/mo per IP) attach to a standalone Public IP resource inside a VNet — a shape neither App Service nor APIM Consumption/Basic has without a networking change out of scope here, so they don't actually apply to this architecture regardless of cost. Front Door's real value on top of APIM would be Layer-7 flood mitigation and a managed WAF, but the managed rule sets and bot protection live in the Premium tier ($330/mo) — the free default protection plus APIM's own rate-limiting policy covers this project's actual risk profile at $0. **Front Door Standard is the one item on the original wishlist explicitly cut, on security-architecture grounds, not budget grounds** — it would be redundant in front of APIM for a single-region app with no real attacker profile. Revisit only if a real production traffic pattern ever justifies it.

**CI/CD — GitHub Actions with OIDC federated credentials**, not a stored publish profile or service-principal secret — the same no-plaintext-secrets posture as the database and Key Vault access, extended into the pipeline. EF Core migrations are generated as an idempotent SQL script in CI and applied as an explicit deploy step, not via `Database.Migrate()` at app startup — decouples "the app started" from "the schema changed" and is the habit worth building now. **Known gotcha, planned for in advance:** if the Postgres firewall is locked down, the GitHub-hosted runner's IP needs to be allowed (or a self-hosted runner used) for migrations to reach the database during deploy.

## Design of Custom Data Types (changes from the mini project)

**Models**

- `VideoLink` — unchanged shape (`Id`, `UserId`, `Url`, `Platform`, `Title`, `ThumbnailUrl`, `Category`, `Tags`, `CreatedAtUtc`), but now an EF Core entity. `Platform` requires `HasConversion<string>()` in the EF configuration or it persists as an `int` — unreadable rows, and a future enum reorder would silently corrupt existing data. `Tags` maps to Postgres `text[]` via Npgsql's native array support (decided over a join table — reopen only if tags ever need renaming/merging/their own counts).
- `User` — repurposed as a profile, not a credential holder. Drops `PasswordHash` entirely. Gains: `EntraObjectId` (the `oid` claim, unique, the real identity key), `StripeCustomerId` (nullable until a subscription starts), `SubscriptionStatus` (free / active / past_due / canceled), `IsAdmin` (bool, or superseded by an Entra app role — see Admin section). The existing hand-written `Email` property (private backing field, blank/format/immutability validation) is kept — it's still a good encapsulation example, and email still needs storing even though it no longer authenticates anything.
- **`VideoMetadataService`** (new) — `GetMetadataAsync(url)` switches on `DetectPlatform(url)`. Two real branches: **YouTube** (free, no network call — the existing `getYouTubeThumbnail()` ID-extraction logic moves server-side unchanged) and **TikTok** (oEmbed, open, no key required, the one platform where going server-side genuinely unlocks a thumbnail the client couldn't produce alone). Two fallback branches: **Instagram and Facebook always return a placeholder.** This is not a gap to close later — Meta's oEmbed responses stopped including `thumbnail_url` for Facebook posts/videos and Instagram posts effective Nov 3 2025 (verified directly against Meta's developer blog, logged in `memory.md`), and the replacement path requires a Meta developer app plus a `meta_oembed_read` token this project has no reason to obtain. `IHttpClientFactory` (never `new HttpClient()`), a ~3s timeout, and a try/catch that falls back to `null` — a missing thumbnail is a gray placeholder card, a failed HTTP call must never fail the save.

**Subscriptions (new)**

- `SubscriptionTier` (enum: `Free`, `Paid`) drives a **free-tier cap of 25 links**; `Paid` accounts are uncapped. This is the smallest paywall that touches the product in exactly one place (the add-link path checks the count before inserting) rather than gating features throughout the UI.
- Stripe integration runs in **test mode** for the whole project — no real charges, no business entity required, same Checkout/webhook flow as production. `POST /api/billing/checkout` creates a Stripe Checkout Session for the demo product/price; a webhook endpoint (`POST /api/billing/webhook`) verifies the Stripe signature using the webhook secret from Key Vault and updates `SubscriptionStatus`/`StripeCustomerId` on the `users` row. The webhook, not the redirect-back page, is the source of truth for subscription state — a browser tab closing mid-checkout must never leave the account in a stale state.

**Admin (new)**

- Modeled as an **Entra app role** (`Admin`) assigned in the External ID tenant, surfaced as a `roles` claim on the token and checked via `[Authorize(Roles = "Admin")]` — not a hand-rolled `IsAdmin` boolean on the `User` row, which would be one more piece of custom auth logic in a project that just finished removing custom auth logic. One concrete screen: an admin-only page/endpoint listing all users with their link count and subscription status (`GET /api/admin/users`) — enough for the role to do something visible, not a stub.

## Solution Structure (target)

Monorepo, cloned outside OneDrive (same reasoning as the mini project — background sync can corrupt `.git`). One new top-level project (`VideoLinkVault.Core`) shared between the API and, eventually, nothing on the frontend side (React/React Native don't reference a .NET class library — Core exists for the API and any future .NET client, not for code-sharing with JS):

    VideoLinkVault/
    ├── docker-compose.yml                            NEW — local Postgres 16 container for dev; VideoLinkVaultDbContext points here by default
    ├── src/
    │   ├── VideoLinkVault.Core/                     shared class library — no DB, no web dependencies
    │   │   ├── Models/
    │   │   │   ├── Platform.cs                      unchanged from the mini project
    │   │   │   ├── VideoLink.cs                     unchanged shape, now an EF entity (mapped in Api/Data)
    │   │   │   └── User.cs                          REPURPOSED: drops PasswordHash, gains EntraObjectId/StripeCustomerId/SubscriptionStatus
    │   │   └── Interfaces/
    │   │       ├── IVideoLinkRepository.cs          NEW — the seam the mini project deliberately deferred (Deviation #4)
    │   │       └── IVideoMetadataService.cs         NEW
    │   │
    │   ├── VideoLinkVault.Api/                      ASP.NET Core Web API, EF Core + Npgsql — the only project with a DB connection
    │   │   ├── Data/
    │   │   │   ├── VideoLinkVaultDbContext.cs        NEW
    │   │   │   ├── Configurations/
    │   │   │   │   ├── VideoLinkConfiguration.cs     NEW — Platform.HasConversion<string>(), Tags as Postgres text[]
    │   │   │   │   └── UserConfiguration.cs          NEW — unique index on EntraObjectId
    │   │   │   └── Migrations/                       EF Core migrations; applied via CI as an idempotent SQL script, not Database.Migrate()
    │   │   │
    │   │   │       ↳ target database — NOT a folder in this repo, an external resource the connection string/managed identity point at:
    │   │   │           local dev  → `docker-compose.yml`'s Postgres 16 container (root of the repo, password auth, disposable)
    │   │   │           deployed   → Azure Database for PostgreSQL – Flexible Server (B1ms), auth via App Service's managed identity — no password, no connection string secret anywhere
    │   │   ├── DTO/
    │   │   │   ├── CreateVideoLinkRequest.cs         unchanged from the mini project
    │   │   │   ├── UserResponse.cs                   trimmed — no password-adjacent fields ever existed to remove, but drops nothing new
    │   │   │   ├── CheckoutRequest.cs                NEW — Stripe Checkout session request
    │   │   │   └── AdminUserSummaryResponse.cs       NEW — id, email, link count, subscription status
    │   │   ├── Services/
    │   │   │   ├── EfVideoLinkRepository.cs          NEW — implements IVideoLinkRepository; replaces VideoLinkStore
    │   │   │   ├── VideoMetadataService.cs           NEW — implements IVideoMetadataService; YouTube + TikTok real, Meta placeholder
    │   │   │   └── StripeService.cs                  NEW — Checkout session creation, webhook signature verification
    │   │   ├── Controllers/
    │   │   │   ├── LinksController.cs                REPLACES VideoLinksController — {userId} removed, owner from token
    │   │   │   ├── BillingController.cs              NEW — POST /api/billing/checkout, POST /api/billing/webhook
    │   │   │   └── AdminController.cs                NEW — GET /api/admin/users, [Authorize(Roles = "Admin")]
    │   │   ├── appsettings.json                      Entra External ID authority/audience, Key Vault URI — no secrets
    │   │   └── Program.cs                            DI, EF Core + managed-identity Npgsql provider, JWT bearer auth (Entra), Key Vault config source, CORS
    │   │
    │   │   ── REMOVED from the mini project's Backend_Link_Vault, not carried forward ──
    │   │       Controllers/AuthController.cs         · DTO/LoginRequest.cs · DTO/RegisterRequest.cs
    │   │       Services/UserStore.cs (credential role) · Services/VideoLinkStore.cs (superseded by EfVideoLinkRepository)
    │   │       IPasswordHasher<User> registration in Program.cs
    │   │
    │   └── VideoLinkVault.Web/                      React 19 + Vite
    │       └── src/
    │           ├── App.jsx                           owns links, categories, filters; auth state now comes from MSAL context, not local state
    │           ├── config.js                         API base URL (now the APIM gateway, not the API directly) + Entra External ID tenant/client IDs
    │           ├── auth/
    │           │   └── msalConfig.js                 NEW — MSAL instance, Entra External ID authority, token scopes
    │           └── components/                       each with a co-located .css, same convention as the mini project
    │               ├── LinkCard.jsx                  unchanged
    │               ├── CategorySidebar.jsx            unchanged
    │               ├── SearchBar.jsx                 unchanged
    │               ├── AddLinkDialog.jsx              loses client-side platform override as the primary source of truth once VideoMetadataService lands — becomes an optional override sent to the server, same relationship pattern the mini project already used for Title
    │               ├── SignInButton.jsx               NEW — replaces AuthDialog's mock-credential form with MSAL's redirect/popup sign-in
    │               ├── UpgradeDialog.jsx              NEW — triggers POST /api/billing/checkout, shows free-tier cap state
    │               └── AdminDashboard.jsx             NEW — admin-only, rendered only when the roles claim includes Admin
    │
    │           ── REMOVED from the mini project's Frontend_Link_Vault, not carried forward ──
    │               components/AuthDialog.jsx + .css   (MOCK_ACCOUNT hardcoded credential, client-side-only validation)
    │
    ├── mobile/ (stretch — not started until steps 1-6 are done)
    │   └── VideoLinkVault.Mobile/                    React Native (Expo), separate codebase, no shared frontend code with Web
    │       ├── App.js
    │       ├── auth/msalConfig.js                    MSAL React Native, same Entra External ID tenant as Web
    │       └── screens/
    │           ├── SignInScreen.js
    │           └── LinksListScreen.js                read-only for this deadline — no AddLink/Delete screens planned
    │
    └── .github/
        └── workflows/
            ├── build-and-test.yml                    dotnet build/test + npm build on every PR
            └── deploy.yml                            OIDC login to Azure, generate + apply idempotent EF migration script, deploy Api to App Service, deploy Web to Static Web Apps

**Endpoints (target)**

| Method | Route                   | Purpose                                            | Auth                                 |
| ------ | ----------------------- | -------------------------------------------------- | ------------------------------------ |
| GET    | `/api/links`            | List the caller's links                            | Bearer token required                |
| POST   | `/api/links`            | Add a link (blocked if free tier + at cap)         | Bearer token required                |
| DELETE | `/api/links/{id}`       | Delete a link                                      | Bearer token required                |
| POST   | `/api/billing/checkout` | Create a Stripe Checkout session                   | Bearer token required                |
| POST   | `/api/billing/webhook`  | Stripe webhook receiver                            | Stripe signature, not a bearer token |
| GET    | `/api/admin/users`      | List all users + link counts + subscription status | Bearer token, `Admin` role           |

## Build Order (dependency-ordered, not date-boxed — see Timeline for how this maps to Oct 5)

1. Solution restructure + EF Core against local Docker Postgres; port existing CRUD; finally build the `IVideoLinkRepository`/EF-backed-repository seam the mini project deliberately deferred (PRD.md's mini-project Deviation #4).
2. Entra External ID: tenant provisioning, MSAL on the frontend, JWT validation on the API, JIT user provisioning, routes lose `{userId}`.
3. Azure deploy: App Service + Azure Postgres + managed-identity DB auth + Key Vault; GitHub Actions CI/CD via OIDC with idempotent-script migrations; APIM Consumption with `validate-jwt` + `rate-limit-by-key`. **A real, live, single-user-working deployment exists at the end of this step** — the checkpoint that matters if nothing after it finishes in time.
4. `VideoMetadataService` — YouTube + TikTok server-side, Meta placeholders.
5. Stripe (test mode) + free-tier cap + secrets in Key Vault.
6. Admin app role + `/api/admin/users` + a minimal admin screen.
7. React Native (Expo) read-only client — sign in, list links.
8. Front Door Standard — explicitly cut from MVP (see Architecture); only reconsidered if everything else lands early.

## External Resources Required

- Azure subscription: **MSSA/sponsored** (confirm the spending limit directly with the program if one exists; budgeting below assumes none is hit)
- Microsoft Entra External ID tenant (separate from the main Azure AD tenant)
- Azure Database for PostgreSQL – Flexible Server (Burstable B1ms or similar)
- Azure App Service (Linux, B1 or similar)
- Azure API Management (Consumption tier)
- Azure Key Vault
- Stripe account (test mode — no business verification needed)
- GitHub Actions (OIDC federated credential configured against the Azure subscription, no stored secrets)
- TikTok oEmbed (open, no key). YouTube thumbnail URL pattern (no API key, no account). Instagram/Facebook: **not obtained** — Meta developer app + `meta_oembed_read` deliberately out of scope (see VideoMetadataService above).

## Estimated Monthly Cost (lean stack, MSSA subscription)

| Resource                                        | Approx. cost                                               |
| ----------------------------------------------- | ---------------------------------------------------------- |
| App Service (B1)                                | ~$13                                                       |
| Azure Postgres Flexible Server (B1ms + storage) | ~$15–20                                                    |
| Static Web Apps                                 | free tier                                                  |
| Entra External ID                               | free (< 50,000 MAU)                                        |
| Key Vault                                       | pennies                                                    |
| API Management (Consumption)                    | ~free at this volume                                       |
| Front Door                                      | $0 — cut from MVP                                          |
| DDoS Network/IP Protection                      | $0 — doesn't apply to this architecture (see Architecture) |
| **Total**                                       | **~$30–35/month**                                          |

Check whether the MSSA subscription carries Azure-for-Students-style free credits for Postgres; if so this drops further. App Service and Postgres figures are estimates from public pricing guidance current as of Sep 2026 — verify against the Azure Pricing Calculator before committing.

## Known Limitations (accepted, not defects, going into the capstone build)

- **Instagram and Facebook links never get a real thumbnail.** Placeholder cards only, permanently, for reasons outside this project's control (Meta's Nov 2025 oEmbed changes). Documented as an accepted limitation, not a TODO.
- **APIM Consumption has a cold-start penalty.** A demo must include a warm-up request before anything is shown live.
- **Stripe runs in test mode only.** No real payment processing; this is a deliberate scope boundary, not a placeholder for "wire up real billing later."
- **React Native, if it ships at all, is read-only** (sign in + list). Add/delete on mobile is out of scope for this deadline.

## Deviations from the Mini Project (each deliberate, reasoning in `memory.md`)

1. **Auth is fully replaced, not extended.** Entra External ID replaces the mini project's hashed-password auth outright; `AuthController`/`UserStore`'s credential logic/`PasswordHash` DTOs are removed.
2. **`{userId}` leaves the routes.** Ownership now comes from the validated token, closing the mini project's accepted "anyone with a GUID can access that vault" limitation.
3. **A real relational database replaces in-memory storage**, per the plan since the original v3 design doc — not a new decision, just finally executed.
4. **The `IVideoLinkRepository` interface seam**, deliberately skipped in the mini project, gets built now that EF Core/Postgres makes it worth having.
5. **Video metadata generation moves server-side**, closing the "client can lie about platform/thumbnail" trust gap flagged since the mini project's VideoMetadataService discussion.
6. **The product gains a subscription tier and an admin role**, neither present nor planned in the mini project's original scope.

---

_This PRD reflects decisions made through Sep 10 2026. It will be revised if scope changes before Oct 5 — see `memory.md`'s running log for anything decided after this date._
