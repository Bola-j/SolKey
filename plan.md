## Plan: SolKey Remaining Backend Completion

Align the backend with PostgreSQL, fix security/validation plumbing, implement real email verification via SMTP, add presigned R2 uploads, complete missing APIs/access rules/admin workflows, and then harden with tests.

**Handoff context**
- Data store is PostgreSQL with Npgsql; spec must match this choice.
- Email verification must be real and production-ready, using SMTP (AWS SES SMTP or similar).
- Uploads should be presigned client uploads to Cloudflare R2.
- API responses use ResponseEnvelope and PagedResponse patterns.
- Validation uses Euphoric.FluentValidation.AspNetCore and existing validators.
- JWT claims use "sub" for user id and "role" for roles.

**Milestones**
M0 Foundation alignment complete (PostgreSQL, auth/validation wiring, Swagger JWT).
M1 Email verification live (token storage, send, verify).
M2 Presigned uploads live (video + payment screenshots).
M3 Content gaps closed (question/session reads, tags, pagination, free-tier limits).
M4 Admin workflows complete (approvals + grants).
M5 Analytics/wallet optional phase complete.
M6 Quality hardening done (soft delete, config-driven expiry, tests).

**Task checklist (handoff-ready with clarifying context)**

M0 - Foundation alignment
1. M0-A Spec alignment to PostgreSQL.
   - Purpose: make the spec match the actual data store choice.
   - Dependencies: none.
   - Do: update Spec-1-SolKey.md to mention PostgreSQL/Npgsql and remove SQL Server references.
   - Done when: spec reflects PostgreSQL everywhere and no SQL Server text remains.
2. M0-B JWT role claim mapping.
   - Purpose: make [Authorize(Roles=...)] and user identity claims work.
   - Dependencies: none.
   - Do: configure JwtBearer RoleClaimType to "role" and NameClaimType to "sub".
   - Done when: role-protected endpoints accept a token with role claim and user id resolves from "sub".
3. M0-C FluentValidation wiring + filters.
   - Purpose: enforce validators on requests.
   - Dependencies: none.
   - Do: register Euphoric.FluentValidation.AspNetCore, ensure ValidationActionFilter/ValidationExceptionFilter exist and are applied.
   - Done when: invalid payloads return validation errors for Auth/Payment/Upload endpoints.
4. M0-D Swagger JWT security.
   - Purpose: allow testing protected endpoints via Swagger UI.
   - Dependencies: none.
   - Do: add bearer security definition and requirement.
   - Done when: Swagger UI can authorize and call protected endpoints.
5. M0-E Soft delete correctness.
   - Purpose: prevent hard deletes and honor query filters.
   - Dependencies: none.
   - Do: when EntityState.Deleted, mark IsDeleted and convert to Modified.
   - Done when: deleting entities keeps rows and sets IsDeleted.
6. M0-F Refresh token expiry uses config.
   - Purpose: centralize token lifetime.
   - Dependencies: none.
   - Do: use JwtOptions.RefreshTokenDays in AuthService instead of hard-coded values.
   - Done when: refresh token expiry matches config.
7. M0-G AutoMapper decision.
   - Purpose: avoid half-adopted mapping.
   - Dependencies: none.
   - Do: either remove AutoMapper references from the plan or add profiles and use it consistently in services.
   - Done when: mapping strategy is explicit and consistent.

M1 - Email verification (SMTP, cheapest scalable option)
8. M1-A Verification token storage.
   - Purpose: persist verification tokens and expiry.
   - Dependencies: M0.
   - Do: add EmailVerificationToken entity/config/migration with TokenHash (not plaintext), ExpiresAt (e.g., 24h), ConsumedAt, Purpose, and indexes on TokenHash and ExpiresAt.
   - Done when: migration applies and tokens can be created/queried; hashes are stored instead of raw tokens.
9. M1-B Send verification email on register.
   - Purpose: deliver verification link to users.
   - Dependencies: M1-A.
   - Do: add SMTP email sender (AWS SES SMTP or similar), configure From address and base URL, and send verification link after registration.
   - Done when: register triggers a real email send and token is stored.
10. M1-C Verify endpoint.
   - Purpose: complete verification.
   - Dependencies: M1-A.
   - Do: implement endpoint to validate token hash, check expiry/consumed, mark user verified, mark token consumed, and return a safe response for already-verified users.
   - Done when: user.IsEmailVerified toggles to true via token and token cannot be reused.
11. M1-D Resend endpoint with rate limit.
   - Purpose: allow retry without abuse.
   - Dependencies: M1-A.
   - Do: add resend endpoint with a throttle (e.g., 1 per hour per user), log attempts, and avoid user enumeration in responses.
   - Done when: resend works and rate limit blocks rapid repeats.

M2 - Presigned uploads to Cloudflare R2
12. M2-A Presigned upload endpoints.
   - Purpose: upload large files without API bottleneck.
   - Dependencies: M0.
   - Do: add endpoints to create presigned PUT/POST URLs for videos and payment screenshots; enforce size and content-type; generate key prefix (userId/purpose/date/guid-filename).
   - Done when: client can upload directly to R2 and receives blob path for saving.
13. M2-B Enforce server-generated blob paths.
   - Purpose: prevent arbitrary path injection.
   - Dependencies: M2-A.
   - Do: store issued upload tickets (blobPath + expiry + purpose) and require UploadVideo/CreatePayment to validate the ticket before saving.
   - Done when: endpoints reject arbitrary paths and accept only issued paths.
14. M2-C Require verified teacher for uploads.
   - Purpose: align with spec rules for teacher verification.
   - Dependencies: M0.
   - Do: ensure Video upload checks IsVerifiedTeacher in addition to role.
   - Done when: unverified teachers are blocked from uploads.

M3 - Content gaps and access rules
15. M3-A Missing read endpoints.
   - Purpose: complete spec APIs.
   - Dependencies: M0.
   - Do: add GET question by id and GET session by id endpoints with required access checks.
   - Done when: endpoints exist and return ResponseEnvelope with expected data.
16. M3-B Free-tier limits.
   - Purpose: enforce spec limits.
   - Dependencies: M0.
   - Do: add daily usage counters (table-based per user per day) for question views and asks; enforce for non-subscribers.
   - Done when: limits trigger and reset by day.
17. M3-C Tags CRUD + filtering with pagination.
   - Purpose: enable tags + search.
   - Dependencies: M0.
   - Do: add tag CRUD endpoints and tag-based filtering for questions/videos using PagedResponse.
   - Done when: filtering returns paged results and tags are manageable by admin.

M4 - Admin workflows and access grants
18. M4-A Session approval.
   - Purpose: allow admin moderation.
   - Dependencies: M0.
   - Do: add admin approve session endpoint; ensure student list endpoints return only approved sessions.
   - Done when: unapproved sessions are hidden from students.
19. M4-B Payment approval grants access (idempotent).
   - Purpose: payment drives access.
   - Dependencies: M2.
   - Do: on approve payment, create subscription or session purchase based on PaymentType, check for existing access to avoid double-grants, and set access windows.
   - Done when: approval grants access exactly once.
20. M4-C Admin management endpoints.
   - Purpose: admin CRUD over subscriptions and tags.
   - Dependencies: M4-B.
   - Do: add endpoints to list/update subscriptions and tags as needed by MVP.
   - Done when: admin can manage these via API.

M5 - Analytics and wallet (optional/should-have)
21. M5-A Wallet and transactions.
   - Purpose: track earnings.
   - Dependencies: M0.
   - Do: add TeacherWallet and WalletTransaction entities/config and services.
   - Done when: wallet balance updates from approved payments or views.
22. M5-B Analytics endpoints.
   - Purpose: teacher/admin insights.
   - Dependencies: M5-A.
   - Do: add endpoints for counts, revenue summaries, and content trends.
   - Done when: endpoints return aggregated metrics.

M6 - Quality and hardening
23. M6-A Build and smoke verification.
   - Purpose: ensure stability.
   - Dependencies: all applicable prior tasks.
   - Do: dotnet build SolKey.slnx and manual smoke for auth/session/payment/upload/email verification flows.
   - Done when: build passes and smoke test checklist is green.
24. M6-B Access control regression.
   - Purpose: prevent regressions in gating.
   - Dependencies: M0-M4.
   - Do: verify role-based and subscription gating across endpoints.
   - Done when: role and subscription rules hold across endpoints.

**Relevant files**
- src/SolKey.API/Program.cs
- src/SolKey.API/Spec-1-SolKey.md
- src/SolKey.Infrastructure/Persistence/SolKeyDbContext.cs
- src/SolKey.Infrastructure/Identity/JwtTokenService.cs
- src/SolKey.Infrastructure/Services/AuthService.cs
- src/SolKey.Infrastructure/Services/VideoService.cs
- src/SolKey.Infrastructure/Services/SubscriptionService.cs
- src/SolKey.API/Controllers/AuthController.cs
- src/SolKey.API/Controllers/VideosController.cs
- src/SolKey.API/Controllers/SubscriptionController.cs
- src/SolKey.API/Controllers/QuestionsController.cs
- src/SolKey.API/Controllers/AdminController.cs
- src/SolKey.Application/Validators/RegisterRequestValidator.cs

**Verification**
1. dotnet build SolKey.slnx
2. Manual smoke: register, verify email, login, refresh, presigned upload, create session, purchase, secure video URL
3. Validate role-protected endpoints via Swagger

**Decisions**
- Use PostgreSQL (keep Npgsql) and update spec/docs to match.
- Use SMTP email sending via AWS SES (or equivalent low-cost provider) for verification.
- Use presigned client uploads to Cloudflare R2 for videos and payment screenshots.
