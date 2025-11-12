# Documentation Consolidation TODO

## 🎯 Objective

Create a new, well-organized documentation structure in `/Docs` that consolidates information from all DRAFT sources into clear, focused documents.

## ⚠️ Critical Guidelines for AI

When creating or updating documentation:

1. **ONLY document facts that can be verified by:**

   - Reading the actual source code
   - Asking the user for clarification
   - Referencing existing configuration files

2. **DO NOT include:**

   - Assumptions about how things work
   - Speculative information
   - Best practices unless explicitly in the code/config
   - Guesses about implementation details

3. **When uncertain:**

   - Mark sections with `[TODO: Verify with user]`
   - Ask specific questions before documenting
   - Leave gaps rather than filling with assumptions

4. **NEVER link to DRAFT documentation from public docs:**
   - All files in `/Docs/DRAFT/` are internal reference only
   - Only link to completed, published documentation we've worked through together
   - Azure deployment docs are user's personal reference (will be removed from version control)
   - Wait for user to work through content before adding links to public documentation

## � Documentation Organization Strategy

**Approach:** Create smaller, focused documents that cross-reference each other via hyperlinks. This approach:

- Keeps documents focused and manageable
- Makes maintenance easier
- Allows readers to drill down into specific topics
- Enables better organization through subdirectories

**Naming Convention:** Use descriptive, hyphenated names (e.g., `user-signup-workflow.md`, `api-error-handling.md`)

---

## �📋 Documentation Structure Plan

### 1. Root README.md (New)

**Purpose:** High-level overview and entry point

**Content:**

- Brief project description (2-3 paragraphs)
- Key features (bullet list, no details)
- Quick start (basic steps only)
- Links to detailed documentation
- License information

**Source Material:**

- `/DRAFT/Root/README-Original.md` (extract overview sections)
- User input for tone and content decisions

**Status:** ⏳ Not Started

---

### 2. `/Docs/GETTING-STARTED.md` (New)

**Purpose:** Quick setup guide for new developers

**Content:**

- Prerequisites
- Clone and initial setup
- Running locally (both Docker and manual)
- First-time configuration
- Common issues and solutions

**Source Material:**

- `/DRAFT/Root/README-Original.md` (Quick Start sections)
- `/DRAFT/Docker/Docker-Setup.md` (setup portions)
- `/DRAFT/Frontend/React-Client-Setup.md` (setup sections)

**Status:** ⏳ Not Started

---

### 3. `/Docs/ARCHITECTURE.md` (New)

**Purpose:** High-level technical architecture overview (navigation hub)

**Content:**

- System architecture diagram [TODO: Create or reference]
- Technology stack summary (frontend, backend, database)
- Project structure overview (folder organization)
- Key design patterns summary
- Module/feature organization
- **Links to detailed docs:**
  - → `/Docs/Authentication/overview.md` (auth architecture)
  - → `/Docs/Development/Frontend/architecture.md` (frontend patterns)
  - → `/Docs/Development/Backend/architecture.md` (backend patterns)
  - → `/Docs/Features/` (feature-specific architecture)

**Source Material:**

- `/DRAFT/Root/README-Original.md` (Architecture sections)
- `/DRAFT/AI-Guidelines/Copilot-Instructions.md` (Project Architecture section)
- Verify structure by reading actual source folders

**Status:** ⏳ Not Started

---

### 4. `/Docs/Authentication/` (New Directory Structure)

**Purpose:** Authentication and authorization documentation split into focused documents

**Documents:**

#### `/Docs/Authentication/overview.md`

- Authentication system overview
- Authorization/permissions system overview
- Links to detailed workflow documents

#### `/Docs/Authentication/user-signup-workflow.md`

- User signup process (with sequence diagram)
- Email verification flow (with sequence diagram)
- Form validation and error handling
- Frontend and backend components involved

#### `/Docs/Authentication/approval-workflow.md`

- Platform admin approval process (with sequence diagram)
- Approval/rejection flows
- Email notifications
- Status transitions

#### `/Docs/Authentication/login-flow.md`

- Login authentication flow (with sequence diagram)
- Token generation and validation
- Status-based responses
- Error handling

#### `/Docs/Authentication/permissions.md`

- Permission system architecture
- Role-based permissions
- Permission format (`resource:action`)
- Adding new permissions

#### `/Docs/Authentication/platform-admin-config.md`

- Platform admin setup via environment variables
- Security considerations
- Configuration examples
- Troubleshooting

#### `/Docs/Authentication/jwt-tokens.md`

- Token structure and lifecycle
- Refresh token mechanism
- Token validation
- Security considerations

**Source Material:**

- `/DRAFT/Root/README-Original.md` (Auth sections)
- `/DRAFT/Root/README-AuthUpdates.md` (Approval workflow)
- `/DRAFT/Root/PLATFORM_ADMIN.md` (Platform admin details)
- Verify by reading auth source code (both frontend `/features/auth` and backend `Features/Auth`)

**Status:** ⏳ Not Started

---

### 5. `/Docs/Features/` (New Directory Structure)

**Purpose:** Feature documentation split by feature area

**Structure:**

#### `/Docs/Features/overview.md`

- Feature list and descriptions
- Links to individual feature docs

#### `/Docs/Features/dashboard.md`

- Dashboard overview and widgets

#### `/Docs/Features/financial-projections.md`

- Projections feature
- Chart components and data flow

#### `/Docs/Features/accounts.md`

- Account management
- Account filtering patterns

#### `/Docs/Features/expenses.md`

- Expense management
- Recurring expense logic

#### `/Docs/Features/income.md`

- Income management
- Income tracking

#### `/Docs/Features/data-import-export.md`

- Import/export functionality
- File formats and validation

#### `/Docs/Features/user-management.md`

- User invitation and management
- Role assignment

#### `/Docs/Features/settings.md`

- User settings
- Site settings

**Note:** Each feature doc should be concise (1-3 pages) and link to relevant technical docs in `/Development` folder.

**Source Material:**

- `/DRAFT/Root/README-Original.md` (Features section - lines ~140-2400)
- Verify feature details by examining source code in `/src/features`

**Status:** ⏳ Not Started

---

### 6. `/Docs/Development/DOCKER.md` (New)

**Purpose:** Docker setup and deployment

**Content:**

- Docker configurations (server-only, full-stack)
- Building and running containers
- Environment variables
- Image versioning system
- Troubleshooting

**Source Material:**

- `/DRAFT/Docker/Docker-Setup.md` (entire file)
- Verify by reading docker-compose files

**Status:** ⏳ Not Started

---

### 7. `/Docs/Development/DATABASE.md` (New)

**Purpose:** Database and Entity Framework documentation

**Content:**

- Database setup
- Creating migrations
- Applying migrations
- Entity patterns and conventions
- Relationship configuration
- Sequence management

**Source Material:**

- `/DRAFT/Backend/Database-Migrations.md` (entire file)
- Verify by reading Pot.Data project files

**Status:** ⏳ Not Started

---

### 8. `/Docs/Development/Frontend/` (New Directory Structure)

**Purpose:** Frontend development documentation split into focused topics

**Documents:**

#### `/Docs/Development/Frontend/setup.md`

- React setup and installation
- Prerequisites
- Environment configuration
- Available scripts

#### `/Docs/Development/Frontend/architecture.md`

- Project structure (`/features`, `/components`, `/api`)
- Feature-based organization
- Module boundaries

#### `/Docs/Development/Frontend/state-management.md`

- Zustand for global state
- React Query for server state
- React Context for local state
- When to use each

#### `/Docs/Development/Frontend/api-integration.md`

- API hook patterns (useGet, usePost, etc.)
- Result type system
- Error handling
- Cache invalidation strategy
- Special cases (import/export)

#### `/Docs/Development/Frontend/components.md`

- Component library overview
- Reusable components
- DataTable component (link to detailed doc)
- Form components
- shadcn/ui usage

#### `/Docs/Development/Frontend/components/data-table.md`

- Detailed DataTable documentation
- API reference
- Usage examples

#### `/Docs/Development/Frontend/coding-standards.md`

- TypeScript conventions
- React patterns
- File organization
- Import/export patterns

#### `/Docs/Development/Frontend/forms-validation.md`

- React Hook Form integration
- Zod validation schemas
- Form patterns

#### `/Docs/Development/Frontend/permissions-ui.md`

- PermissionGuard component
- WithPermission component
- Permission-based UI patterns

#### `/Docs/Development/Frontend/routing.md`

- React Router setup
- Protected routes
- Navigation patterns

**Source Material:**

- `/DRAFT/Frontend/React-Client-Setup.md`
- `/DRAFT/Frontend/Environment-Configuration.md`
- `/DRAFT/Frontend/API/Import-Export-Hooks.md`
- `/DRAFT/Frontend/Components/DataTable-Component.md`
- `/DRAFT/Root/README-Original.md` (Frontend sections)
- `/DRAFT/AI-Guidelines/Copilot-Instructions.md`
- Verify by reading source code

**Status:** ⏳ Not Started

---

### 9. `/Docs/Development/Backend/` (New Directory Structure)

**Purpose:** Backend development documentation split into focused topics

**Documents:**

#### `/Docs/Development/Backend/architecture.md`

- ASP.NET Core project organization
- Layer architecture (AspNetCore → App → Data)
- Features and concerns organization
- Shared/DTO project structure

#### `/Docs/Development/Backend/services-layer.md`

- Services layer pattern
- Dependency injection setup
- Service organization

#### `/Docs/Development/Backend/features-concerns.md`

- Feature-based organization
- Concerns (cross-cutting functionality)
- Feature structure conventions

#### `/Docs/Development/Backend/api-endpoints.md`

- Minimal API endpoints
- Request/response patterns
- Endpoint organization

#### `/Docs/Development/Backend/validation.md`

- FluentValidation setup
- Validation patterns
- Custom validation rules

#### `/Docs/Development/Backend/error-handling.md`

- Problem details (RFC 7807)
- Error response patterns
- Exception handling

#### `/Docs/Development/Backend/entity-framework.md`

- EF Core patterns
- Query patterns
- Transaction management
- Link to `/Docs/Development/database.md` for migrations

#### `/Docs/Development/Backend/email-system.md`

- Email sending architecture
- Email templates
- Email services

**Source Material:**

- `/DRAFT/Backend/Database-Migrations.md` (EF Core patterns)
- `/DRAFT/Root/README-Original.md` (Backend sections)
- `/DRAFT/AI-Guidelines/Copilot-Instructions.md` (Backend project structure)
- Verify by reading server project files

**Status:** ⏳ Not Started

---

### 10. `/Docs/Development/TESTING.md` (New)

**Purpose:** Testing guidelines and setup

**Content:**

- [TODO: Verify testing setup with user]
- **Note:** Limited testing currently implemented. This document will be completed when comprehensive testing is added.
- Test structure
- Running tests
- Writing tests
- Coverage requirements

**Source Material:**

- `/DRAFT/Root/README-Original.md` (Testing sections)
- `/DRAFT/Frontend/React-Client-Setup.md` (Test scripts)
- Verify by reading test configuration files and example tests

**Status:** ⏳ Deferred (awaiting comprehensive testing implementation)

---

### 11. `/Docs/DEPLOYMENT.md` (New)

**Purpose:** Production deployment guide

**Content:**

- Docker deployment (local production setup)
- Environment configuration
- Database migrations in production
- Monitoring and health checks
- **Note:** Azure-specific deployment will remain in `/Docs/Azure/` for user reference only (not for public repo due to sensitive settings)

**Source Material:**

- `/DRAFT/Docker/Docker-Setup.md` (Production sections)
- `/DRAFT/Root/README-Original.md` (Deployment sections)
- Verify by reading docker-compose files and deployment scripts

**Status:** ⏳ Not Started (Low priority - may be minimal or excluded)

---

### 12. `/Docs/Azure/` (Existing - User Reference Only)

**Purpose:** Azure-specific deployment documentation (user's personal reference)

**Content:**

- Azure deployment architecture
- Container Apps configuration
- Database setup
- **Note:** Contains sensitive Azure settings - for user reference only, not for public repository
- **Future consideration:** May create bicep files as learning experience (testing approach TBD)

**Source Material:**

- Existing Azure documentation
- User will maintain separately

**Status:** 🔄 Deferred (User's private reference, not part of public docs)

---

### 13. `/Docs/AI-CODING-GUIDE.md` (New)

**Purpose:** Guidelines for AI coding assistants

**Content:**

- Project architecture summary
- Coding conventions
- Patterns to follow/avoid
- Common pitfalls
- Tool usage guidelines

**Source Material:**

- `/DRAFT/AI-Guidelines/Copilot-Instructions.md` (entire file)
- `/DRAFT/AI-Guidelines/Copilot-Prompt.md` (review process)
- Consolidate and organize by topic

**Status:** ⏳ Not Started

---

## 📝 Writing Tone Guidelines

**Style:** Tutorial style (default), with technical detail as appropriate

**Key Principles:**

- Clear and direct
- Start high-level, add detail progressively
- Work in small, reviewable chunks
- Tutorial style for user-facing docs
- Technical style for developer-focused docs
- Let subject matter guide the tone

**Review Process:**

- Create small sections for review
- User provides feedback
- Adjust tone/style as needed
- Continue or pivot based on feedback

---

## 🔄 Review Process

1. ✅ **Create initial structure** (this TODO file)
2. ✅ **Discuss with user:** Confirm document list and organization
3. ✅ **Get user answers** to questions
4. ⏳ **Create root README.md** (small section first)
   - Start with high-level overview
   - Submit for review in small chunks
   - Iterate based on feedback
5. **Continue with remaining documents** (priority order TBD)
6. **Cross-reference and link documents**
7. **Create mermaid diagrams** as needed
8. **Final review and cleanup**

**Working Approach:**

- Small, incremental changes
- Frequent review checkpoints
- Easy to adjust direction
- Less rewriting if changes needed

---

## ✅ Answers from User

1. **Document Priority:** Start with root README.md - high-level overview to help shape the plan
2. **Root README:** Start very high-level, avoid overwhelming with content upfront. Can revisit and expand as we proceed.
3. **Sequence Diagrams:**
   - Use Mermaid (`.mmd` files)
   - Generate high-resolution PNG from mermaid
   - Use PNG in documentation (not mermaid code)
   - Store in `/Docs/mermaid/` subdirectory (both .mmd and .png files)
4. **Cross-linking Strategy:** Use relative paths (will load correctly anywhere)
5. **Testing:** None currently. Testing documentation deferred - tests need to be written first.
6. **Deployment:**
   - Azure docs are for user reference, not final repo (contains sensitive Azure settings)
   - May create bicep files as learning experience (but unsure how to test without risking deployed resources)
   - Deployment docs likely minimal or excluded from public documentation
7. **Tone:** Tutorial style or technical, depending on subject
   - Start with tutorial style
   - **Work in SMALL chunks for easier review and less rewriting**
   - Can adjust style as we proceed
8. **Document Length:** Not worried about length
   - Break into multiple docs if it helps keep context small/manageable
   - Separate files allow readers to print specific topics easily

---

## 📊 Progress Tracking

### Document Status

- Total Document Areas: 13
- Estimated Individual Documents: ~45 (many split into focused sub-documents)
- Completed: 4
  - Root README.md (Section 1 complete - awaiting expansion)
  - GETTING-STARTED.md (Prerequisites, Clone, Configuration complete - links to other docs)
  - DOCKER-SETUP.md (Complete - VS Code tasks and Docker Compose CLI commands)
  - FIRST-TIME-SETUP.md (Complete - awaiting screenshots to be added later)
- In Progress: 1
  - LOCAL-SETUP.md (Title and overview placeholder created, focus: API and React locally, PostgreSQL flexible)
- Not Started: ~40
- Deferred: 2
  - Testing (awaiting comprehensive testing implementation)
  - Azure docs (user will handle separately)

**Note:** The modular approach creates more documents but each is smaller, focused, and easier to maintain.

### Current Work: Documentation Reorganization Complete

**Completed:**

- ✅ GETTING-STARTED.md - Prerequisites, Clone, Configuration sections complete
- ✅ DOCKER-SETUP.md - Full Docker setup (VS Code tasks and Docker Compose CLI)
- ✅ FIRST-TIME-SETUP.md - User signup, email verification, platform admin setup
  - **Note:** Placeholder for screenshots - images will be added to `Docs/first-time-setup/images/` folder
- ✅ LOCAL-SETUP.md - Created with title placeholder (renamed from MANUAL-SETUP.md for clarity)

**Documentation Structure:**

1. **GETTING-STARTED.md** → Entry point with Prerequisites & Configuration
2. **DOCKER-SETUP.md** → Docker setup instructions (recommended path)
3. **LOCAL-SETUP.md** → Local setup without Docker (API and React run locally, PostgreSQL flexible)
4. **FIRST-TIME-SETUP.md** → First user configuration and platform admin setup

**Next Chunks:**

1. Complete LOCAL-SETUP.md content:
   - PostgreSQL setup (local installation OR Docker container - user's choice)
   - Backend (.NET) local setup and configuration
   - Frontend (React) local setup and configuration
   - Environment configuration for local development
2. Add screenshots to FIRST-TIME-SETUP.md (organize in `Docs/first-time-setup/images/`)
3. Common issues & troubleshooting (may be separate doc or added to each)

---

**Last Updated:** November 12, 2025
