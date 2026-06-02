# SolKey API - System Actors & Comprehensive User Stories

This document provides a detailed software architecture and business analysis of the **SolKey** .NET 10 Web API project. It maps the system's actors, roles, permissions, business logic constraints, and generates user stories (happy paths and edge cases) for all 43 endpoints.

---

## 1. System Actors & Roles Identification

### UserRole Enum Extraction
As defined in `src/Domain/Enums/UserRole.cs`:
1. `Student = 1`
2. `Teacher = 2`
3. `Admin = 3`

### Role-to-Feature Permission Matrix

| Endpoint | HTTP Method | Auth Requirement | Allowed Roles | Description |
| :--- | :--- | :--- | :--- | :--- |
| **Authentication & Identity** | | | | |
| `/api/auth/register` | POST | None | Public | Register a new Student or Teacher account |
| `/api/auth/login` | POST | None | Public | Login, start a session, get JWT & Refresh Token |
| `/api/auth/refresh` | POST | None | Public | Exchange an unexpired refresh token for new JWT |
| `/api/auth/logout` | POST | None | Public | Invalidate refresh token and end user session |
| `/api/auth/verify-email` | GET | None | Public | Verify user's email via link token |
| `/api/auth/resend-verification`| POST | JWT | Any Auth User | Resend email verification (60m cooldown) |
| `/api/auth/test-auth` | GET | JWT | Any Auth User | Dev-only connection check |
| `/api/auth/whoami` | GET | JWT | Any Auth User | Dev-only claims details check |
| **User Profile** | | | | |
| `/api/user/profile` | GET | JWT | Any Auth User | Retrieve user's own profile details |
| `/api/user/profile` | PUT | JWT | Any Auth User | Update user's own profile details |
| **Books & Curriculum** | | | | |
| `/api/books` | GET | None | Public | List books (paginated) |
| `/api/books/{id}` | GET | None | Public | Get details of a book |
| `/api/books` | POST | JWT + Role | Admin | Create a new book |
| `/api/books/{id}` | PUT | JWT + Role | Admin | Update a book |
| `/api/books/{id}` | DELETE | JWT + Role | Admin | Soft delete a book |
| `/api/chapters/book/{bookId}` | GET | None | Public | List chapters of a book (paginated) |
| `/api/chapters/{id}` | GET | None | Public | Get chapter details |
| `/api/chapters` | POST | JWT + Role | Admin | Create a new chapter |
| `/api/chapters/{id}` | PUT | JWT + Role | Admin | Update a chapter |
| `/api/chapters/{id}` | DELETE | JWT + Role | Admin | Soft delete a chapter |
| `/api/lessons/chapter/{chapterId}`| GET | None | Public | List lessons of a chapter (paginated) |
| `/api/lessons/{id}` | GET | None | Public | Get lesson details |
| `/api/lessons` | POST | JWT + Role | Admin | Create a new lesson |
| `/api/lessons/{id}` | PUT | JWT + Role | Admin | Update a lesson |
| `/api/lessons/{id}` | DELETE | JWT + Role | Admin | Soft delete a lesson |
| **Questions & Answers** | | | | |
| `/api/questions/ask` | POST | JWT | Student | Ask a question on a lesson (must be verified student) |
| `/api/questions/answer` | POST | JWT + Role | Teacher | Answer a student question (must be verified teacher) |
| `/api/questions/vote` | POST | JWT | Any Auth User | Upvote or downvote an answer (30s cooldown, persists only aggregate vote totals/rate; voter identity is not stored) |
| **Explanation Sessions** | | | | |
| `/api/sessions` | GET | None | Public | List approved explanation sessions (paginated) |
| `/api/sessions` | POST | JWT + Role | Teacher | Create explanation session (starts unapproved) |
| `/api/sessions/purchase` | POST | JWT + Role | Student | Purchase an explanation session |
| **Subscriptions & Payments** | | | | |
| `/api/subscription/status` | GET | JWT + Role | Student | Check student's QA subscription status |
| `/api/subscription/payment` | POST | JWT + Role | Student | Submit proof of QA subscription or session payment screenshot; session payments include `SessionId` |
| **Tags Management** | | | | |
| `/api/tags` | GET | None | Public | Query/List tags (paginated) |
| `/api/tags` | POST | JWT + Role | Admin | Create a new tag |
| `/api/tags/{id}` | PUT | JWT + Role | Admin | Update tag name |
| `/api/tags/{id}` | DELETE | JWT + Role | Admin | Soft delete a tag |
| **Storage & Uploads** | | | | |
| `/api/uploads/presign` | POST | JWT | Any Auth User | Get S3 presigned URL for upload ticket |
| `/api/videos/upload` | POST | JWT + Role | Teacher | Register a video upload (verified teacher only) |
| `/api/videos/{id}/secure-url` | GET | JWT | Any Auth User | Get secure S3 signed read URL with watermark |
| **Platform Administration** | | | | |
| `/api/admin/verify-teacher/{id}` | POST | JWT + Role | Admin | Approve teacher verification request |
| `/api/admin/approve-video/{id}` | POST | JWT + Role | Admin | Approve teacher uploaded video |
| `/api/admin/approve-payment/{id}` | POST | JWT + Role | Admin | Temporarily approve manual payment while payment provider integration is pending; approval automatically grants/extends QA subscription or grants session access |

---

## 2. Comprehensive User Stories by Epic

---
## Epic: Authentication & Identity

### US-01: User Registration (Happy Path)
**As a** visitor
**I want to** register an account as a Student or Teacher
**So that** I can access the system's authenticated features

**Acceptance Criteria:**
- [ ] Submitting valid details registers the user and creates a new account.
- [ ] Users registering as teachers are required to submit a non-empty bio.
- [ ] Triggers an email verification token creation and sends a verification link via email.
- [ ] Automatically logs in the user and returns an access token and refresh token.

**Technical Notes:**
- Endpoint: `POST /api/auth/register`
- Auth: None
- Related entities: `User`, `UserSession`, `RefreshToken`, `EmailVerificationToken`

---
### US-02: Register Duplicate Email (Edge Case)
**As a** visitor
**I want to** be prevented from registering with an existing email
**So that** my account identity remains unique

**Acceptance Criteria:**
- [ ] Submitting a registration request with an email already in the database throws an error.
- [ ] The system returns a user-friendly error message indicating that the email already exists.
- [ ] No database changes are committed and no verification email is sent.

**Technical Notes:**
- Endpoint: `POST /api/auth/register`
- Auth: None
- Related entities: `User`

---
### US-03: Teacher Register Without Bio (Edge Case)
**As a** visitor registering as a Teacher
**I want to** receive a validation warning if I omit my professional biography
**So that** my teacher profile meets platform quality standards before registration

**Acceptance Criteria:**
- [ ] Registering with `IsTeacher = true` and an empty or whitespace-only `Bio` field returns a validation error.
- [ ] Account is not created.
- [ ] Error response contains a message: "Teachers must provide a bio."

**Technical Notes:**
- Endpoint: `POST /api/auth/register`
- Auth: None
- Related entities: `User`

---
### US-04: User Login (Happy Path)
**As an** registered user
**I want to** log in with my email and password
**So that** I can retrieve authorization tokens for my device

**Acceptance Criteria:**
- [ ] Authenticating with correct credentials creates a new active `UserSession` and `RefreshToken`.
- [ ] Returns JWT access token (with claims for email, role, teacher verification status, and email verification status) and a refresh token.
- [ ] If active sessions exceed the user's role-based limit (Admin/Teacher: 3, Subscribed Student: 2, Unsubscribed Student: 1), the oldest active session is automatically deactivated.

**Technical Notes:**
- Endpoint: `POST /api/auth/login`
- Auth: None
- Related entities: `User`, `UserSession`, `RefreshToken`

---
### US-05: Login with Invalid Credentials (Edge Case)
**As a** visitor
**I want to** receive a secure generic error when logging in with incorrect credentials
**So that** malicious actors cannot enumerate valid emails or guess passwords

**Acceptance Criteria:**
- [ ] Submitting an unregistered email or incorrect password throws an exception.
- [ ] The API returns a `400 BadRequest` with a generic message: "Invalid credentials."
- [ ] No tokens are issued and no session is created.

**Technical Notes:**
- Endpoint: `POST /api/auth/login`
- Auth: None
- Related entities: `User`

---
### US-06: Refresh Token (Happy Path)
**As an** authenticated client
**I want to** exchange my refresh token for a new access token
**So that** I can maintain my session without prompting the user to re-enter credentials

**Acceptance Criteria:**
- [ ] Submitting a valid, unexpired, and unrevoked refresh token along with the correct device ID revokes the current refresh token.
- [ ] Creates and returns a new access token, a new refresh token, and updates the session's active status.

**Technical Notes:**
- Endpoint: `POST /api/auth/refresh`
- Auth: None
- Related entities: `User`, `UserSession`, `RefreshToken`

---
### US-07: Refresh with Expired/Revoked Token (Edge Case)
**As an** authenticated client
**I want to** be forced to re-login if my refresh token is invalid or expired
**So that** stolen or compromised refresh tokens cannot be used indefinitely

**Acceptance Criteria:**
- [ ] Requesting a refresh with a revoked or expired token returns `400 BadRequest` or `401 Unauthorized` stating "Refresh token invalid."
- [ ] Requesting a refresh with mismatched device details or inactive session returns "Session invalid."
- [ ] No new tokens are issued.

**Technical Notes:**
- Endpoint: `POST /api/auth/refresh`
- Auth: None
- Related entities: `RefreshToken`, `UserSession`

---
### US-08: User Logout (Happy Path)
**As a** logged-in user
**I want to** log out of my current session
**So that** my refresh token is invalidated and my session deactivated

**Acceptance Criteria:**
- [ ] Submitting a logout request revokes the refresh token and sets the session's `IsActive` to false.
- [ ] Cleanly logs out without error.

**Technical Notes:**
- Endpoint: `POST /api/auth/logout`
- Auth: None
- Related entities: `RefreshToken`, `UserSession`

---
### US-09: Logout with Invalid Token (Edge Case)
**As a** client
**I want** logout requests for non-existent refresh tokens to terminate gracefully
**So that** the client application can clean up local storage without getting stuck in error loops

**Acceptance Criteria:**
- [ ] Submitting a logout request with a non-existent or null refresh token does not throw an exception.
- [ ] The API returns an HTTP 200 success response envelope.

**Technical Notes:**
- Endpoint: `POST /api/auth/logout`
- Auth: None
- Related entities: `RefreshToken`

---
### US-10: Verify Email (Happy Path)
**As a** registered user
**I want to** verify my email by clicking the verification link sent to me
**So that** I can unlock student or teacher-specific actions on the platform

**Acceptance Criteria:**
- [ ] Submitting a valid verification token hashes it and verifies it against the database record.
- [ ] Updates the user's `IsEmailVerified` flag to `true`.
- [ ] Marks the token as consumed.

**Technical Notes:**
- Endpoint: `GET /api/auth/verify-email`
- Auth: None
- Related entities: `User`, `EmailVerificationToken`

---
### US-11: Verify Email with Invalid Token (Edge Case)
**As a** visitor
**I want to** receive an error if I verify my email with an invalid or expired token
**So that** invalid verification attempts do not erroneously verify user accounts

**Acceptance Criteria:**
- [ ] If the token is null, expired, already consumed, or has a mismatched hash, it throws an error.
- [ ] The user's email verified status remains false.
- [ ] Returns `400 BadRequest` with "Invalid or expired verification token."

**Technical Notes:**
- Endpoint: `GET /api/auth/verify-email`
- Auth: None
- Related entities: `EmailVerificationToken`

---
### US-12: Resend Verification Email (Happy Path)
**As an** authenticated user with an unverified email
**I want to** request a new verification email
**So that** I can verify my account if the previous email was lost or expired

**Acceptance Criteria:**
- [ ] The API creates a new verification token, invalidates any outstanding active tokens for this purpose, and triggers a new email.
- [ ] Returns HTTP 200 Success.

**Technical Notes:**
- Endpoint: `POST /api/auth/resend-verification`
- Auth: JWT
- Related entities: `User`, `EmailVerificationToken`

---
### US-13: Resend Verification Cooldown Active (Edge Case)
**As a** user requesting a resend of the verification link
**I want to** be restricted by a rate limit cooldown
**So that** the platform is protected from email spam abuse

**Acceptance Criteria:**
- [ ] If a verification email was already sent within the configured cooldown window (default: 60 minutes), the request returns success but *does not* send a new email (silently ignored to prevent spam).
- [ ] No new database records are added.

**Technical Notes:**
- Endpoint: `POST /api/auth/resend-verification`
- Auth: JWT
- Related entities: `EmailVerificationToken`

---
### US-14: Test Authentication - Development Mode (Happy Path)
**As a** developer
**I want to** hit a simple test endpoint in development mode
**So that** I can quickly verify that my bearer token authentication headers are wired correctly

**Acceptance Criteria:**
- [ ] When the environment is Development, the endpoint returns HTTP 200 with the username.
- [ ] Requires a valid JWT token.

**Technical Notes:**
- Endpoint: `GET /api/auth/test-auth`
- Auth: JWT
- Related entities: `User`

---
### US-15: Test Authentication - Production Mode (Edge Case)
**As a** system administrator
**I want** developer-only testing endpoints to be blocked in production
**So that** platform attack surfaces are minimized

**Acceptance Criteria:**
- [ ] When the environment is not Development, requesting the endpoint returns HTTP 404.
- [ ] The claims or auth validation logic is not executed.

**Technical Notes:**
- Endpoint: `GET /api/auth/test-auth`
- Auth: JWT
- Related entities: None

---
### US-16: Dev WhoAmI Claims Check (Happy Path)
**As a** developer
**I want to** inspect the parsed claims of my token in development
**So that** I can debug authorization policies and token issues

**Acceptance Criteria:**
- [ ] When in Development environment, returns HTTP 200 with the list of user claims and a boolean checking the existence of the Authorization header.
- [ ] Requires JWT.

**Technical Notes:**
- Endpoint: `GET /api/auth/whoami`
- Auth: JWT
- Related entities: None

---
### US-17: Dev WhoAmI Claims Check - Production (Edge Case)
**As a** security engineer
**I want** user claims inspector endpoints to return 404 in production
**So that** user claim payloads cannot be queried by unauthorized parties

**Acceptance Criteria:**
- [ ] In non-Development environments, calling this endpoint returns HTTP 404.
- [ ] No claim data is leaked.

**Technical Notes:**
- Endpoint: `GET /api/auth/whoami`
- Auth: JWT
- Related entities: None

---
## Epic: User Profile

---
### US-18: Get Profile (Happy Path)
**As an** authenticated user
**I want to** view my profile details
**So that** I can verify my account information and credentials

**Acceptance Criteria:**
- [ ] Accessing this endpoint returns user properties: FirstName, LastName, Email, PhoneNumber, PhotoUrl, Bio, IsEmailVerified, and IsVerifiedTeacher.
- [ ] Requires valid JWT.

**Technical Notes:**
- Endpoint: `GET /api/user/profile`
- Auth: JWT
- Related entities: `User`

---
### US-19: Get Profile - Expired JWT (Edge Case)
**As an** expired session user
**I want to** be blocked from accessing my profile
**So that** my data is secured when I leave my terminal or my session expires

**Acceptance Criteria:**
- [ ] Requesting the profile with an expired or missing JWT returns HTTP 401 Unauthorized.
- [ ] The user profile details are not returned.

**Technical Notes:**
- Endpoint: `GET /api/user/profile`
- Auth: JWT
- Related entities: None

---
### US-20: Update Profile (Happy Path)
**As a** logged-in user
**I want to** update my profile information
**So that** my personal details (name, photo, bio, phone) remain accurate and up-to-date

**Acceptance Criteria:**
- [ ] Submitting profile update details successfully overwrites FirstName, LastName, PhoneNumber, PhotoUrl, and Bio.
- [ ] Returns the updated profile response envelope.
- [ ] Auditing fields (`ModifiedAt`, `ModifiedBy`) are correctly updated in the database.

**Technical Notes:**
- Endpoint: `PUT /api/user/profile`
- Auth: JWT
- Related entities: `User`

---
### US-21: Update Profile - User Not Found (Edge Case)
**As a** user with a deleted account
**I want** my updates to fail if my database record has been deleted
**So that** orphans are not created or updated in the system

**Acceptance Criteria:**
- [ ] If the authenticated user's ID matches a record that does not exist or has been deleted in the database, the operation fails.
- [ ] Returns HTTP 404 NotFound with the message "User not found."

**Technical Notes:**
- Endpoint: `PUT /api/user/profile`
- Auth: JWT
- Related entities: `User`

---
## Epic: Books & Curriculum

---
### US-22: List Books (Happy Path)
**As a** visitor
**I want to** view a list of books
**So that** I can browse the subjects and modules available on the platform

**Acceptance Criteria:**
- [ ] Returns a paginated list of books sorted alphabetically by Title.
- [ ] Includes total count, page number, and page size in the envelope response.
- [ ] Accessible without authentication.

**Technical Notes:**
- Endpoint: `GET /api/books`
- Auth: None
- Related entities: `Book`

---
### US-23: List Books Pagination Clamping (Edge Case)
**As a** visitor
**I want** invalid pagination arguments to be clamped to reasonable limits
**So that** the request doesn't throw a server error and page sizes don't crash database memory

**Acceptance Criteria:**
- [ ] Supplying a `page` less than 1 defaults to 1.
- [ ] Supplying a `pageSize` less than 1 defaults to 20.
- [ ] Supplying a `pageSize` greater than 100 clamps it to 100.
- [ ] Pagination logic remains successful.

**Technical Notes:**
- Endpoint: `GET /api/books`
- Auth: None
- Related entities: `Book`

---
### US-24: Get Book By ID (Happy Path)
**As a** visitor
**I want to** retrieve the details of a specific book
**So that** I can learn more about its content and preview its cover

**Acceptance Criteria:**
- [ ] Retrieves a single book's ID, Title, Description, and CoverImage URL.
- [ ] Returns HTTP 200 with the success envelope.
- [ ] Accessible without authentication.

**Technical Notes:**
- Endpoint: `GET /api/books/{id}`
- Auth: None
- Related entities: `Book`

---
### US-25: Get Book Not Found (Edge Case)
**As a** visitor
**I want to** receive a 404 error when querying a non-existent book ID
**So that** I know the book does not exist

**Acceptance Criteria:**
- [ ] Querying a non-existent or invalid Guid returns HTTP 404 NotFound.
- [ ] The error message contains "Book not found."

**Technical Notes:**
- Endpoint: `GET /api/books/{id}`
- Auth: None
- Related entities: `Book`

---
### US-26: Create Book (Happy Path)
**As an** Administrator
**I want to** create a new book
**So that** teachers and students can select it for curriculum learning paths

**Acceptance Criteria:**
- [ ] Creates a book with Title, Description, and CoverImage URL.
- [ ] Returns HTTP 200 with the newly created book.
- [ ] Restricts creation to users with the Admin role.

**Technical Notes:**
- Endpoint: `POST /api/books`
- Auth: JWT + Role (Admin)
- Related entities: `Book`

---
### US-27: Create Book - Unauthorized Role (Edge Case)
**As a** non-admin user
**I want** my book creation requests to be rejected
**So that** regular users cannot modify platform curriculum resources

**Acceptance Criteria:**
- [ ] An authenticated Student or Teacher attempting to post to `/api/books` receives a `403 Forbidden` response.
- [ ] The book is not added to the database.

**Technical Notes:**
- Endpoint: `POST /api/books`
- Auth: JWT + Role (Admin)
- Related entities: `Book`

---
### US-28: Update Book (Happy Path)
**As an** Administrator
**I want to** update a book's metadata
**So that** corrections, typos, and cover graphics can be maintained

**Acceptance Criteria:**
- [ ] Overwrites Title, Description, and CoverImage for the matching book ID.
- [ ] Updates audit columns (`ModifiedAt`, `ModifiedBy`).
- [ ] Requires Admin role.

**Technical Notes:**
- Endpoint: `PUT /api/books/{id}`
- Auth: JWT + Role (Admin)
- Related entities: `Book`

---
### US-29: Update Book Not Found (Edge Case)
**As an** Administrator
**I want** updates to non-existent books to fail cleanly
**So that** I do not write orphaned edits or trigger database exceptions

**Acceptance Criteria:**
- [ ] Sending a PUT request for a book Guid that does not exist returns HTTP 404 NotFound.
- [ ] No data changes are made.

**Technical Notes:**
- Endpoint: `PUT /api/books/{id}`
- Auth: JWT + Role (Admin)
- Related entities: `Book`

---
### US-30: Delete Book (Happy Path)
**As an** Administrator
**I want to** delete a book
**So that** it is removed from the platform catalog and no longer appears in results

**Acceptance Criteria:**
- [ ] Deleting a book soft-deletes the record in the database (sets `IsDeleted` to true).
- [ ] Updates audit metadata.
- [ ] Only accessible by Admins.

**Technical Notes:**
- Endpoint: `DELETE /api/books/{id}`
- Auth: JWT + Role (Admin)
- Related entities: `Book`

---
### US-31: Delete Book Not Found (Edge Case)
**As an** Administrator
**I want** book deletion to report an error if the book does not exist
**So that** I am informed of concurrency issues or invalid IDs

**Acceptance Criteria:**
- [ ] Sending a DELETE request for a non-existent book Guid returns HTTP 404 NotFound stating "Book not found."
- [ ] No DB update is performed.

**Technical Notes:**
- Endpoint: `DELETE /api/books/{id}`
- Auth: JWT + Role (Admin)
- Related entities: `Book`

---
### US-32: List Chapters of a Book (Happy Path)
**As a** visitor
**I want to** view a list of chapters inside a book
**So that** I can browse its pedagogical outline

**Acceptance Criteria:**
- [ ] Returns a paginated list of chapters associated with the given book ID, sorted by their `Order` column.
- [ ] Accessible publicly without authentication.

**Technical Notes:**
- Endpoint: `GET /api/chapters/book/{bookId}`
- Auth: None
- Related entities: `Book`, `Chapter`

---
### US-33: List Chapters for Invalid Book (Edge Case)
**As a** visitor
**I want to** receive an empty paginated result if I query chapters for a non-existent Book ID
**So that** the client application can gracefully display an empty chapter tree instead of throwing errors

**Acceptance Criteria:**
- [ ] Querying chapters for a non-existent or arbitrary Guid returns a paginated response with an empty list and total count of 0.
- [ ] Returns HTTP 200.

**Technical Notes:**
- Endpoint: `GET /api/chapters/book/{bookId}`
- Auth: None
- Related entities: `Book`, `Chapter`

---
### US-34: Get Chapter by ID (Happy Path)
**As a** visitor
**I want to** view details of a specific chapter
**So that** I can verify its contents and associated book index

**Acceptance Criteria:**
- [ ] Retrieves chapter ID, BookId, Title, and Order.
- [ ] Accessible without authentication.

**Technical Notes:**
- Endpoint: `GET /api/chapters/{id}`
- Auth: None
- Related entities: `Chapter`

---
### US-35: Get Chapter Not Found (Edge Case)
**As a** visitor
**I want** a 404 error when retrieving a non-existent chapter
**So that** I know the chapter does not exist

**Acceptance Criteria:**
- [ ] Querying a non-existent Guid for a chapter returns HTTP 404 NotFound.
- [ ] Message returned in the envelope is "Chapter not found."

**Technical Notes:**
- Endpoint: `GET /api/chapters/{id}`
- Auth: None
- Related entities: `Chapter`

---
### US-36: Create Chapter (Happy Path)
**As an** Administrator
**I want to** add a chapter to a book
**So that** I can categorize lessons and assignments appropriately

**Acceptance Criteria:**
- [ ] Creates a new chapter using BookId, Title, and Order index.
- [ ] Only allows Admins to perform the action.
- [ ] Returns the created Chapter details.

**Technical Notes:**
- Endpoint: `POST /api/chapters`
- Auth: JWT + Role (Admin)
- Related entities: `Book`, `Chapter`

---
### US-37: Create Chapter with Missing Book (Edge Case)
**As an** Administrator
**I want** chapter creation to fail if I supply an invalid Book ID
**So that** chapters cannot be orphaned or assigned to non-existent books

**Acceptance Criteria:**
- [ ] Submitting a chapter with a non-existent BookId fails database foreign key constraints (or is caught beforehand by validation).
- [ ] Returns standard error handling response.

**Technical Notes:**
- Endpoint: `POST /api/chapters`
- Auth: JWT + Role (Admin)
- Related entities: `Book`, `Chapter`

---
### US-38: Update Chapter (Happy Path)
**As an** Administrator
**I want to** edit chapter details
**So that** I can adjust their ordering or correct chapter titles

**Acceptance Criteria:**
- [ ] Updates BookId, Title, and Order of a chapter.
- [ ] Restricted to Admins.
- [ ] Auditing fields are updated correctly.

**Technical Notes:**
- Endpoint: `PUT /api/chapters/{id}`
- Auth: JWT + Role (Admin)
- Related entities: `Chapter`

---
### US-39: Update Chapter Not Found (Edge Case)
**As an** Administrator
**I want to** receive a 404 error when trying to update a non-existent chapter
**So that** I am alerted that the resource does not exist

**Acceptance Criteria:**
- [ ] Updating a non-existent Guid returns HTTP 404 NotFound stating "Chapter not found."

**Technical Notes:**
- Endpoint: `PUT /api/chapters/{id}`
- Auth: JWT + Role (Admin)
- Related entities: `Chapter`

---
### US-40: Delete Chapter (Happy Path)
**As an** Administrator
**I want to** delete a chapter
**So that** it is removed from the catalog along with its hierarchy

**Acceptance Criteria:**
- [ ] Soft-deletes the chapter by changing its `IsDeleted` flag.
- [ ] Restricted to Admins.

**Technical Notes:**
- Endpoint: `DELETE /api/chapters/{id}`
- Auth: JWT + Role (Admin)
- Related entities: `Chapter`

---
### US-41: Delete Chapter Not Found (Edge Case)
**As an** Administrator
**I want** deletion of a non-existent chapter to return 404
**So that** I can detect duplicate deletions or race conditions

**Acceptance Criteria:**
- [ ] Deleting an invalid or missing chapter ID returns HTTP 404 NotFound.

**Technical Notes:**
- Endpoint: `DELETE /api/chapters/{id}`
- Auth: JWT + Role (Admin)
- Related entities: `Chapter`

---
### US-42: List Lessons of a Chapter (Happy Path)
**As a** visitor
**I want to** view a list of lessons under a chapter
**So that** I can select a specific topic to study

**Acceptance Criteria:**
- [ ] Returns a paginated list of lessons matching the ChapterId, sorted by their `Order` index.
- [ ] Publicly accessible.

**Technical Notes:**
- Endpoint: `GET /api/lessons/chapter/{chapterId}`
- Auth: None
- Related entities: `Chapter`, `Lesson`

---
### US-43: List Lessons Chapter ID Empty/Invalid (Edge Case)
**As a** visitor
**I want** lesson list queries for invalid chapters to fail gracefully with an empty list
**So that** client page views don't break with errors

**Acceptance Criteria:**
- [ ] Querying lesson lists for a non-existent chapter Guid returns HTTP 200 with an empty list.

**Technical Notes:**
- Endpoint: `GET /api/lessons/chapter/{chapterId}`
- Auth: None
- Related entities: `Lesson`

---
### US-44: Get Lesson by ID (Happy Path)
**As a** visitor
**I want to** read details of a specific lesson
**So that** I can access the curriculum content

**Acceptance Criteria:**
- [ ] Retrieves lesson details (Id, ChapterId, Title, Order).
- [ ] Publicly accessible.

**Technical Notes:**
- Endpoint: `GET /api/lessons/{id}`
- Auth: None
- Related entities: `Lesson`

---
### US-45: Get Lesson Not Found (Edge Case)
**As a** visitor
**I want to** receive a 404 error when querying a lesson that doesn't exist
**So that** the client knows the lesson is unavailable

**Acceptance Criteria:**
- [ ] Requesting a non-existent lesson Guid returns HTTP 404 NotFound with "Lesson not found."

**Technical Notes:**
- Endpoint: `GET /api/lessons/{id}`
- Auth: None
- Related entities: `Lesson`

---
### US-46: Create Lesson (Happy Path)
**As an** Administrator
**I want to** create a new lesson in a chapter
**So that** students have new curriculum materials to view

**Acceptance Criteria:**
- [ ] Creates a lesson with ChapterId, Title, and Order.
- [ ] Restricted to Admin accounts.
- [ ] Returns the created lesson details.

**Technical Notes:**
- Endpoint: `POST /api/lessons`
- Auth: JWT + Role (Admin)
- Related entities: `Chapter`, `Lesson`

---
### US-47: Create Lesson - Non-Admin Access Denied (Edge Case)
**As a** Teacher
**I want** my attempt to create lessons to be rejected with 403 Forbidden
**So that** curriculum changes are restricted to authorized admins

**Acceptance Criteria:**
- [ ] Calling this endpoint with a Teacher role returns HTTP 403 Forbidden.
- [ ] The lesson is not created.

**Technical Notes:**
- Endpoint: `POST /api/lessons`
- Auth: JWT + Role (Admin)
- Related entities: `Lesson`

---
### US-48: Update Lesson (Happy Path)
**As an** Administrator
**I want to** update lesson details
**So that** I can adjust curriculum hierarchy and naming

**Acceptance Criteria:**
- [ ] Overwrites ChapterId, Title, and Order of the lesson.
- [ ] Restricted to Admins.

**Technical Notes:**
- Endpoint: `PUT /api/lessons/{id}`
- Auth: JWT + Role (Admin)
- Related entities: `Lesson`

---
### US-49: Update Lesson Not Found (Edge Case)
**As an** Administrator
**I want** updating a non-existent lesson to return 404
**So that** I am alerted of obsolete or incorrect lesson references

**Acceptance Criteria:**
- [ ] Submitting updates to a Guid that does not exist returns HTTP 404 NotFound stating "Lesson not found."

**Technical Notes:**
- Endpoint: `PUT /api/lessons/{id}`
- Auth: JWT + Role (Admin)
- Related entities: `Lesson`

---
### US-50: Delete Lesson (Happy Path)
**As an** Administrator
**I want to** soft delete a lesson
**So that** it is retired from active view without breaking historical links

**Acceptance Criteria:**
- [ ] Updates the lesson's `IsDeleted` flag to true.
- [ ] Only accessible by Admins.

**Technical Notes:**
- Endpoint: `DELETE /api/lessons/{id}`
- Auth: JWT + Role (Admin)
- Related entities: `Lesson`

---
### US-51: Delete Lesson Not Found (Edge Case)
**As an** Administrator
**I want** deleting a non-existent lesson to return a 404 error
**So that** I am informed if the deletion command had an incorrect ID

**Acceptance Criteria:**
- [ ] Calling DELETE with a non-existent lesson Guid returns HTTP 404 NotFound with "Lesson not found."

**Technical Notes:**
- Endpoint: `DELETE /api/lessons/{id}`
- Auth: JWT + Role (Admin)
- Related entities: `Lesson`

---
## Epic: Questions & Answers

---
### US-52: Ask a Question (Happy Path)
**As a** verified Student
**I want to** submit a question on a specific lesson with descriptive tags
**So that** teachers can view and answer my query

**Acceptance Criteria:**
- [ ] Creates a new Question record in the database linked to the student and lesson.
- [ ] Converts tags list into a comma-separated string for persistence.
- [ ] Requires user to have the Student role and verified email (`IsEmailVerified = true`).

**Technical Notes:**
- Endpoint: `POST /api/questions/ask`
- Auth: JWT (Student role verified by Business Logic)
- Related entities: `User`, `Lesson`, `Question`

---
### US-53: Ask a Question - Unverified Email (Edge Case)
**As an** unverified Student
**I want to** be blocked from asking questions
**So that** the platform is protected from spam and unverified accounts

**Acceptance Criteria:**
- [ ] If the authenticated student has `IsEmailVerified = false`, calling `/api/questions/ask` throws an error.
- [ ] Returns HTTP 400 BadRequest with the message: "Only verified students can ask questions."

**Technical Notes:**
- Endpoint: `POST /api/questions/ask`
- Auth: JWT
- Related entities: `User`

---
### US-54: Answer a Question (Happy Path)
**As a** verified Teacher
**I want to** write an answer to a student's question
**So that** I can share knowledge and help them understand the lesson

**Acceptance Criteria:**
- [ ] Inserts a new `Answer` record linked to the Question.
- [ ] Requires the user to have the Teacher role and be verified (`IsVerifiedTeacher = true`).

**Technical Notes:**
- Endpoint: `POST /api/questions/answer`
- Auth: JWT + Role (Teacher)
- Related entities: `User`, `Question`, `Answer`

---
### US-55: Answer a Question - Unverified Teacher (Edge Case)
**As an** unverified Teacher
**I want** my attempts to answer questions to be rejected
**So that** only qualified, admin-approved teachers can publish answers

**Acceptance Criteria:**
- [ ] Attempting to answer a question when `IsVerifiedTeacher = false` throws an exception.
- [ ] Returns HTTP 400 BadRequest with "Only verified teachers can answer."

**Technical Notes:**
- Endpoint: `POST /api/questions/answer`
- Auth: JWT + Role (Teacher)
- Related entities: `User`, `Answer`

---
### US-56: Vote on Answer (Happy Path)
**As an** authenticated user
**I want to** upvote or downvote an answer
**So that** high-quality answers are highlighted and inaccurate ones are downranked

**Acceptance Criteria:**
- [ ] Modifying answer votes increments `Upvotes` or `Downvotes` depending on the `IsUpvote` flag.
- [ ] Persists only aggregate answer vote counts, allowing consumers to show a vote rate/score without showing who voted what.
- [ ] Sets voting cooldown in application cache for 30 seconds for the User-Answer combination; this temporary cache key is not persisted.
- [ ] Ensures voter identity is kept fully anonymous in stored entities (no voter ID, vote history, or answer audit link is saved).

**Technical Notes:**
- Endpoint: `POST /api/questions/vote`
- Auth: JWT
- Related entities: `Answer`

---
### US-57: Vote Cooldown Active (Edge Case)
**As a** user voting on answers
**I want to** be blocked from submitting multiple rapid votes on the same answer
**So that** the voting system cannot be manipulated

**Acceptance Criteria:**
- [ ] Submitting a vote request for an AnswerId while the cache key `vote:{userId}:{answerId}` exists returns HTTP 429 TooManyRequests.
- [ ] Response message contains "You are voting too quickly. Please wait and try again."

**Technical Notes:**
- Endpoint: `POST /api/questions/vote`
- Auth: JWT
- Related entities: `Answer`

---
## Epic: Explanation Sessions

---
### US-58: List Approved Explanation Sessions (Happy Path)
**As a** visitor
**I want to** browse explanation sessions
**So that** I can find and purchase live walkthrough events

**Acceptance Criteria:**
- [ ] Returns a paginated list of sessions where `IsApproved = true`.
- [ ] Does *not* return unapproved sessions.
- [ ] Publicly accessible without authentication.

**Technical Notes:**
- Endpoint: `GET /api/sessions`
- Auth: None
- Related entities: `ExplanationSession`

---
### US-59: List Sessions Empty Catalog (Edge Case)
**As a** visitor
**I want** the sessions endpoint to return an empty success array when no approved sessions exist
**So that** the client application can handle the empty state gracefully

**Acceptance Criteria:**
- [ ] If no approved explanation sessions exist in the database, returns HTTP 200 with an empty list.

**Technical Notes:**
- Endpoint: `GET /api/sessions`
- Auth: None
- Related entities: `ExplanationSession`

---
### US-60: Create Explanation Session (Happy Path)
**As a** Teacher
**I want to** create a new explanation session
**So that** students can purchase access to live problem-solving events

**Acceptance Criteria:**
- [ ] Creates a session with Title, Description, Price, and AccessDurationDays.
- [ ] Session defaults to `IsApproved = false`.
- [ ] Restricts creation to users with the Teacher role.

**Technical Notes:**
- Endpoint: `POST /api/sessions`
- Auth: JWT + Role (Teacher)
- Related entities: `User`, `ExplanationSession`

---
### US-61: Create Session - Student Unauthorized (Edge Case)
**As a** Student
**I want** my attempts to create a session to be rejected
**So that** explanation sessions are only authored by teachers

**Acceptance Criteria:**
- [ ] Submitting a POST to `/api/sessions` as a Student returns `403 Forbidden`.
- [ ] No session record is added to the database.

**Technical Notes:**
- Endpoint: `POST /api/sessions`
- Auth: JWT + Role (Teacher)
- Related entities: `ExplanationSession`

---
### US-62: Purchase Explanation Session (Happy Path)
**As a** Student
**I want to** buy an explanation session
**So that** I can gain access to its exclusive videos and content for the access duration

**Acceptance Criteria:**
- [ ] Creates a new `SessionPurchase` for the student with an expiry date calculated as `Now + Session.AccessDurationDays`.
- [ ] Verifies the student hasn't already purchased this session (or that their previous purchase has expired).
- [ ] Only allows students to perform this action.

**Technical Notes:**
- Endpoint: `POST /api/sessions/purchase`
- Auth: JWT + Role (Student)
- Related entities: `ExplanationSession`, `SessionPurchase`

---
### US-63: Purchase Already Owned Session (Edge Case)
**As a** Student
**I want** my purchase request to be blocked if I have an active purchase for the session
**So that** I do not accidentally buy the same session twice and waste money

**Acceptance Criteria:**
- [ ] Attempting to purchase a session when a `SessionPurchase` with `UserId == CurrentUserId` and `SessionId == RequestSessionId` is active (`ExpiresAt > DateTime.UtcNow`) throws an exception.
- [ ] Returns HTTP 400 BadRequest with the message "Session already purchased."

**Technical Notes:**
- Endpoint: `POST /api/sessions/purchase`
- Auth: JWT + Role (Student)
- Related entities: `SessionPurchase`

---
## Epic: Subscriptions & Payments

---
### US-64: Get Subscription Status (Happy Path)
**As a** Student
**I want to** query my subscription status
**So that** I know if I have active access to the QA solution database

**Acceptance Criteria:**
- [ ] Queries subscriptions for the student.
- [ ] If an active subscription exists (`IsActive == true` and `EndDate > Now`), returns `IsActive = true` along with start and end dates.
- [ ] If no subscription exists, returns `IsActive = false` with null dates.
- [ ] Restricts access to Student role.

**Technical Notes:**
- Endpoint: `GET /api/subscription/status`
- Auth: JWT + Role (Student)
- Related entities: `Subscription`

---
### US-65: Get Subscription Status - Teacher Unauthorized (Edge Case)
**As a** Teacher
**I want** my request for subscription status to be rejected
**So that** subscription operations are exclusive to student accounts

**Acceptance Criteria:**
- [ ] Querying subscription status as a Teacher returns HTTP 403 Forbidden.

**Technical Notes:**
- Endpoint: `GET /api/subscription/status`
- Auth: JWT + Role (Student)
- Related entities: None

---
### US-66: Submit Payment Verification (Happy Path)
**As a** Student
**I want to** submit a payment request with a screenshot proof of transaction
**So that** the administrator can verify it and grant me access to my subscription or session

**Acceptance Criteria:**
- [ ] Creates a `Payment` record with Amount, PaymentType (QaSubscription/SessionPurchase), ScreenshotPath, and optional `SessionId`.
- [ ] Requires `SessionId` when `PaymentType = SessionPurchase`.
- [ ] Rejects `SessionId` when `PaymentType = QaSubscription`.
- [ ] Payment is created with `IsApproved = false`.
- [ ] Restricts creation to the Student role.

**Technical Notes:**
- Endpoint: `POST /api/subscription/payment`
- Auth: JWT + Role (Student)
- Related entities: `Payment`

---
### US-67: Submit Payment with Invalid Type (Edge Case)
**As a** Student
**I want** my payment submission to fail if the payment type is invalid
**So that** I cannot submit unsupported payment formats

**Acceptance Criteria:**
- [ ] If the payment type string in the request does not map to a valid `PaymentType` enum (e.g. invalid name), it throws an exception.
- [ ] Returns HTTP 400 BadRequest stating "Invalid payment type."

**Technical Notes:**
- Endpoint: `POST /api/subscription/payment`
- Auth: JWT + Role (Student)
- Related entities: `Payment`

---
## Epic: Tags Management

---
### US-68: List Tags (Happy Path)
**As a** visitor
**I want to** list or search tags
**So that** I can filter videos or questions by interest area

**Acceptance Criteria:**
- [ ] Returns a paginated list of tags, optionally filtered by a `search` string matching part of the tag name, sorted alphabetically.
- [ ] Accessible without authentication.

**Technical Notes:**
- Endpoint: `GET /api/tags`
- Auth: None
- Related entities: `Tag`

---
### US-69: Search Tags - No Results (Edge Case)
**As a** visitor
**I want** tag searches with no matches to return a successful empty list
**So that** filtering systems can show a "No tags found" indicator

**Acceptance Criteria:**
- [ ] When searching with a query that has no match, returns HTTP 200 with an empty list.

**Technical Notes:**
- Endpoint: `GET /api/tags`
- Auth: None
- Related entities: `Tag`

---
### US-70: Create Tag (Happy Path)
**As an** Administrator
**I want to** create a new tag
**So that** students and teachers have standard terms to classify questions and videos

**Acceptance Criteria:**
- [ ] Creates a new Tag record with a trimmed name.
- [ ] Only accessible by the Admin role.
- [ ] Returns the newly created tag.

**Technical Notes:**
- Endpoint: `POST /api/tags`
- Auth: JWT + Role (Admin)
- Related entities: `Tag`

---
### US-71: Create Duplicate Tag (Edge Case)
**As an** Administrator
**I want** tag creation to fail if the tag name already exists
**So that** the tagging system is kept clean and free of duplicates

**Acceptance Criteria:**
- [ ] Creating a tag whose name (case-insensitive) matches an existing tag name throws an exception.
- [ ] Returns HTTP 400 BadRequest with "Tag already exists."

**Technical Notes:**
- Endpoint: `POST /api/tags`
- Auth: JWT + Role (Admin)
- Related entities: `Tag`

---
### US-72: Update Tag (Happy Path)
**As an** Administrator
**I want to** rename an existing tag
**So that** I can correct spelling or standardize terminology

**Acceptance Criteria:**
- [ ] Renames the tag matching the ID to the newly requested name.
- [ ] Restricted to Admins.

**Technical Notes:**
- Endpoint: `PUT /api/tags/{id}`
- Auth: JWT + Role (Admin)
- Related entities: `Tag`

---
### US-73: Update Tag to Duplicate Name (Edge Case)
**As an** Administrator
**I want** my tag updates to fail if the target name conflicts with another existing tag
**So that** I do not merge or duplicate tag names inadvertently

**Acceptance Criteria:**
- [ ] Renaming a tag to a name (case-insensitive) that is already used by a *different* tag ID throws an exception.
- [ ] Returns HTTP 400 BadRequest with "Tag already exists."

**Technical Notes:**
- Endpoint: `PUT /api/tags/{id}`
- Auth: JWT + Role (Admin)
- Related entities: `Tag`

---
### US-74: Delete Tag (Happy Path)
**As an** Administrator
**I want to** soft-delete a tag
**So that** it is removed from active tagging autocomplete lists

**Acceptance Criteria:**
- [ ] Sets the Tag's `IsDeleted` flag to true.
- [ ] Restricted to Admin role.

**Technical Notes:**
- Endpoint: `DELETE /api/tags/{id}`
- Auth: JWT + Role (Admin)
- Related entities: `Tag`

---
### US-75: Delete Tag Not Found (Edge Case)
**As an** Administrator
**I want** deleting a non-existent tag to return a 404 error
**So that** I know if the tag was already deleted or has a bad ID

**Acceptance Criteria:**
- [ ] Sending a DELETE request for a non-existent tag ID returns HTTP 404 NotFound.

**Technical Notes:**
- Endpoint: `DELETE /api/tags/{id}`
- Auth: JWT + Role (Admin)
- Related entities: `Tag`

---
## Epic: Storage & Media Uploads

---
### US-76: Presign Upload URL (Happy Path)
**As an** authenticated user
**I want to** request a presigned S3 PUT upload URL and ticket
**So that** I can upload a file (video, screenshot) directly to secure storage from my client

**Acceptance Criteria:**
- [ ] Generates a secure, temporary S3 upload URL for a key pattern `uploads/{userId}/{date}/{guid}-{filename}`.
- [ ] Inserts a corresponding `UploadTicket` in the database (with fields for size, content type, purpose, expiry date).
- [ ] Requires JWT.

**Technical Notes:**
- Endpoint: `POST /api/uploads/presign`
- Auth: JWT
- Related entities: `UploadTicket`

---
### US-77: Presign Upload File Too Large (Edge Case)
**As a** user uploading a file
**I want** the system to reject uploads that exceed maximum file size limits
**So that** platform storage usage is kept under control and costs are managed

**Acceptance Criteria:**
- [ ] If the requested upload size exceeds the `Upload:MaxSizeBytes` configuration limit, the API throws an exception.
- [ ] Returns HTTP 400 BadRequest indicating size limits were exceeded. No ticket is generated.

**Technical Notes:**
- Endpoint: `POST /api/uploads/presign`
- Auth: JWT
- Related entities: `UploadTicket`

---
### US-78: Upload/Register Video (Happy Path)
**As a** verified Teacher
**I want to** register an uploaded video on the system using my validated upload ticket
**So that** the video metadata is saved and queued for admin review

**Acceptance Criteria:**
- [ ] Associates the uploaded video with a Question or Session.
- [ ] Consumes the `UploadTicket` (marks `IsUsed = true`), validating that the ticket belongs to the user, has not expired, and matches the blob path.
- [ ] Verifies the blob exists in S3.
- [ ] Sets the video to `IsApproved = false`.
- [ ] Only accessible by verified Teachers (`IsVerifiedTeacher = true`).

**Technical Notes:**
- Endpoint: `POST /api/videos/upload`
- Auth: JWT + Role (Teacher)
- Related entities: `User`, `Video`, `UploadTicket`

---
### US-79: Upload Video with Expired Ticket (Edge Case)
**As a** Teacher
**I want** my video registration to fail if the upload ticket is invalid or expired
**So that** teachers cannot bypass the secure upload workflow or reuse tickets

**Acceptance Criteria:**
- [ ] Submitting a registration request with an expired, consumed, or mismatched `UploadTicket` throws an exception.
- [ ] Returns HTTP 400 BadRequest with "Invalid or expired upload ticket."
- [ ] Video record is not created.

**Technical Notes:**
- Endpoint: `POST /api/videos/upload`
- Auth: JWT + Role (Teacher)
- Related entities: `Video`, `UploadTicket`

---
### US-80: Get Secure Video URL (Happy Path)
**As an** authenticated user
**I want to** fetch a secure signed play link with a personalized watermark
**So that** I can stream video content securely without unauthorized redistribution

**Acceptance Criteria:**
- [ ] Generates a secure, temporary S3 signed GET URL.
- [ ] Returns a dynamic watermark string consisting of the user's email and phone number.
- [ ] Verifies access permissions:
  - If the video is free/non-premium: permits access.
  - If video is premium and solution: requires active student QA subscription.
  - If video is session-based: requires active session purchase.

**Technical Notes:**
- Endpoint: `GET /api/videos/{id}/secure-url`
- Auth: JWT
- Related entities: `User`, `Video`, `Subscription`, `SessionPurchase`

---
### US-81: Get Secure Video URL - Access Denied (Edge Case)
**As an** unsubscribed Student
**I want** my request for a premium video URL to be rejected
**So that** premium content is restricted to active subscribers and purchasers

**Acceptance Criteria:**
- [ ] Attempting to access a premium video without an active subscription (or session purchase) fails the authorization check.
- [ ] Returns HTTP 403 Forbidden with the message "Access denied."
- [ ] S3 URL is not generated or leaked.

**Technical Notes:**
- Endpoint: `GET /api/videos/{id}/secure-url`
- Auth: JWT
- Related entities: `Video`, `Subscription`

---
## Epic: Platform Administration

---
### US-82: Verify Teacher (Happy Path)
**As an** Administrator
**I want to** approve and verify a teacher's account
**So that** they can publish answers and upload sessions/videos

**Acceptance Criteria:**
- [ ] Updates the target user's `IsVerifiedTeacher` flag to true.
- [ ] Restricted to the Admin role.

**Technical Notes:**
- Endpoint: `POST /api/admin/verify-teacher/{teacherId}`
- Auth: JWT + Role (Admin)
- Related entities: `User`

---
### US-83: Verify Teacher - Not Found (Edge Case)
**As an** Administrator
**I want** teacher verification requests for non-existent IDs to fail cleanly
**So that** I am informed of invalid references

**Acceptance Criteria:**
- [ ] Submitting a teacher ID that does not exist returns HTTP 404 NotFound stating "Teacher not found."

**Technical Notes:**
- Endpoint: `POST /api/admin/verify-teacher/{teacherId}`
- Auth: JWT + Role (Admin)
- Related entities: `User`

---
### US-84: Approve Uploaded Video (Happy Path)
**As an** Administrator
**I want to** approve a teacher's uploaded video
**So that** the video becomes visible and playable for authorized students

**Acceptance Criteria:**
- [ ] Sets the video's `IsApproved` status to true.
- [ ] Only accessible by Admins.

**Technical Notes:**
- Endpoint: `POST /api/admin/approve-video/{videoId}`
- Auth: JWT + Role (Admin)
- Related entities: `Video`

---
### US-85: Approve Video - Not Found (Edge Case)
**As an** Administrator
**I want** video approvals for non-existent IDs to return 404
**So that** I am alerted of invalid video reference keys

**Acceptance Criteria:**
- [ ] Submitting a video ID that does not exist returns HTTP 404 NotFound with "Video not found."

**Technical Notes:**
- Endpoint: `POST /api/admin/approve-video/{videoId}`
- Auth: JWT + Role (Admin)
- Related entities: `Video`

---
### US-86: Approve Payment (Happy Path)
**As an** Administrator
**I want to** approve a student's manual payment request
**So that** their session access or QA subscription is finalized

**Acceptance Criteria:**
- [ ] Sets the Payment's `IsApproved` flag to true.
- [ ] Automatically grants or extends the student's QA subscription by 1 month if the payment type is `QaSubscription`.
- [ ] Automatically creates a `SessionPurchase` if the payment type is `SessionPurchase` and the student does not already have active access to that session.
- [ ] Does not duplicate access if the payment was already approved or active access already exists.
- [ ] This admin approval flow is temporary until third-party payment provider integration replaces manual approval.
- [ ] Only accessible by Admins.

**Technical Notes:**
- Endpoint: `POST /api/admin/approve-payment/{paymentId}`
- Auth: JWT + Role (Admin)
- Related entities: `Payment`, `Subscription`, `SessionPurchase`, `ExplanationSession`

---
### US-87: Approve Payment - Not Found (Edge Case)
**As an** Administrator
**I want** payment approvals for non-existent IDs to return 404
**So that** I am notified of invalid transactions

**Acceptance Criteria:**
- [ ] Submitting a payment ID that does not exist returns HTTP 404 NotFound with "Payment not found."

**Technical Notes:**
- Endpoint: `POST /api/admin/approve-payment/{paymentId}`
- Auth: JWT + Role (Admin)
- Related entities: `Payment`

---

## 3. Comprehensive Summary Table

| US-# | Title | Actor | Endpoint | Priority |
| :--- | :--- | :--- | :--- | :--- |
| **US-01** | User Registration (Happy Path) | Public Visitor | `POST /api/auth/register` | P0 |
| **US-02** | Register Duplicate Email (Edge Case) | Public Visitor | `POST /api/auth/register` | P2 |
| **US-03** | Teacher Register Without Bio (Edge Case) | Public Visitor | `POST /api/auth/register` | P2 |
| **US-04** | User Login (Happy Path) | Registered User | `POST /api/auth/login` | P0 |
| **US-05** | Login with Invalid Credentials (Edge Case) | Registered User | `POST /api/auth/login` | P1 |
| **US-06** | Refresh Token (Happy Path) | Authenticated Client| `POST /api/auth/refresh` | P0 |
| **US-07** | Refresh with Expired/Revoked (Edge Case) | Authenticated Client| `POST /api/auth/refresh` | P1 |
| **US-08** | User Logout (Happy Path) | Logged-In User | `POST /api/auth/logout` | P0 |
| **US-09** | Logout with Invalid Token (Edge Case) | Logged-In User | `POST /api/auth/logout` | P3 |
| **US-10** | Verify Email (Happy Path) | Registered User | `GET /api/auth/verify-email` | P0 |
| **US-11** | Verify Email with Invalid Token (Edge Case) | Registered User | `GET /api/auth/verify-email` | P1 |
| **US-12** | Resend Verification Email (Happy Path) | Authenticated User| `POST /api/auth/resend-verification` | P1 |
| **US-13** | Resend Verification Cooldown (Edge Case) | Authenticated User| `POST /api/auth/resend-verification` | P3 |
| **US-14** | Test Auth - Dev Mode (Happy Path) | Developer | `GET /api/auth/test-auth` | P3 |
| **US-15** | Test Auth - Prod Mode (Edge Case) | Developer | `GET /api/auth/test-auth` | P3 |
| **US-16** | Dev WhoAmI Claims Check (Happy Path) | Developer | `GET /api/auth/whoami` | P3 |
| **US-17** | Dev WhoAmI Claims - Prod Mode (Edge Case) | Developer | `GET /api/auth/whoami` | P3 |
| **US-18** | Get Profile (Happy Path) | Authenticated User| `GET /api/user/profile` | P1 |
| **US-19** | Get Profile - Expired JWT (Edge Case) | Authenticated User| `GET /api/user/profile` | P2 |
| **US-20** | Update Profile (Happy Path) | Logged-In User | `PUT /api/user/profile` | P1 |
| **US-21** | Update Profile - User Not Found (Edge Case)| Logged-In User | `PUT /api/user/profile` | P2 |
| **US-22** | List Books (Happy Path) | Public Visitor | `GET /api/books` | P1 |
| **US-23** | List Books Pagination Clamping (Edge Case) | Public Visitor | `GET /api/books` | P3 |
| **US-24** | Get Book By ID (Happy Path) | Public Visitor | `GET /api/books/{id}` | P1 |
| **US-25** | Get Book Not Found (Edge Case) | Public Visitor | `GET /api/books/{id}` | P2 |
| **US-26** | Create Book (Happy Path) | Administrator | `POST /api/books` | P2 |
| **US-27** | Create Book - Unauthorized (Edge Case) | Student / Teacher | `POST /api/books` | P2 |
| **US-28** | Update Book (Happy Path) | Administrator | `PUT /api/books/{id}` | P2 |
| **US-29** | Update Book Not Found (Edge Case) | Administrator | `PUT /api/books/{id}` | P3 |
| **US-30** | Delete Book (Happy Path) | Administrator | `DELETE /api/books/{id}` | P2 |
| **US-31** | Delete Book Not Found (Edge Case) | Administrator | `DELETE /api/books/{id}` | P3 |
| **US-32** | List Chapters of a Book (Happy Path) | Public Visitor | `GET /api/chapters/book/{bookId}`| P1 |
| **US-33** | List Chapters for Invalid Book (Edge Case) | Public Visitor | `GET /api/chapters/book/{bookId}`| P3 |
| **US-34** | Get Chapter by ID (Happy Path) | Public Visitor | `GET /api/chapters/{id}` | P1 |
| **US-35** | Get Chapter Not Found (Edge Case) | Public Visitor | `GET /api/chapters/{id}` | P2 |
| **US-36** | Create Chapter (Happy Path) | Administrator | `POST /api/chapters` | P2 |
| **US-37** | Create Chapter Missing Book (Edge Case) | Administrator | `POST /api/chapters` | P3 |
| **US-38** | Update Chapter (Happy Path) | Administrator | `PUT /api/chapters/{id}` | P2 |
| **US-39** | Update Chapter Not Found (Edge Case) | Administrator | `PUT /api/chapters/{id}` | P3 |
| **US-40** | Delete Chapter (Happy Path) | Administrator | `DELETE /api/chapters/{id}` | P2 |
| **US-41** | Delete Chapter Not Found (Edge Case) | Administrator | `DELETE /api/chapters/{id}` | P3 |
| **US-42** | List Lessons of a Chapter (Happy Path) | Public Visitor | `GET /api/lessons/chapter/{chapterId}`| P1 |
| **US-43** | List Lessons Chapter Invalid (Edge Case) | Public Visitor | `GET /api/lessons/chapter/{chapterId}`| P3 |
| **US-44** | Get Lesson by ID (Happy Path) | Public Visitor | `GET /api/lessons/{id}` | P1 |
| **US-45** | Get Lesson Not Found (Edge Case) | Public Visitor | `GET /api/lessons/{id}` | P2 |
| **US-46** | Create Lesson (Happy Path) | Administrator | `POST /api/lessons` | P2 |
| **US-47** | Create Lesson - Non-Admin Forbidden (Edge)| Student / Teacher | `POST /api/lessons` | P2 |
| **US-48** | Update Lesson (Happy Path) | Administrator | `PUT /api/lessons/{id}` | P2 |
| **US-49** | Update Lesson Not Found (Edge Case) | Administrator | `PUT /api/lessons/{id}` | P3 |
| **US-50** | Delete Lesson (Happy Path) | Administrator | `DELETE /api/lessons/{id}` | P2 |
| **US-51** | Delete Lesson Not Found (Edge Case) | Administrator | `DELETE /api/lessons/{id}` | P3 |
| **US-52** | Ask a Question (Happy Path) | Verified Student | `POST /api/questions/ask` | P1 |
| **US-53** | Ask a Question - Unverified Email (Edge Case)| Verified Student | `POST /api/questions/ask` | P2 |
| **US-54** | Answer a Question (Happy Path) | Verified Teacher | `POST /api/questions/answer` | P1 |
| **US-55** | Answer - Unverified Teacher (Edge Case) | Verified Teacher | `POST /api/questions/answer` | P2 |
| **US-56** | Vote on Answer (Happy Path) | Auth User | `POST /api/questions/vote` | P2 |
| **US-57** | Vote Cooldown Active (Edge Case) | Auth User | `POST /api/questions/vote` | P3 |
| **US-58** | List Approved Explanation Sessions (Happy) | Public Visitor | `GET /api/sessions` | P1 |
| **US-59** | List Sessions Empty Catalog (Edge Case) | Public Visitor | `GET /api/sessions` | P3 |
| **US-60** | Create Explanation Session (Happy Path) | Teacher | `POST /api/sessions` | P1 |
| **US-61** | Create Session - Student Forbidden (Edge Case)| Student | `POST /api/sessions` | P2 |
| **US-62** | Purchase Explanation Session (Happy Path) | Student | `POST /api/sessions/purchase` | P1 |
| **US-63** | Purchase Already Owned Session (Edge Case) | Student | `POST /api/sessions/purchase` | P2 |
| **US-64** | Get Subscription Status (Happy Path) | Student | `GET /api/subscription/status` | P0 |
| **US-65** | Subscription Status - Teacher Forbidden (Edge)| Teacher | `GET /api/subscription/status` | P2 |
| **US-66** | Submit Payment Verification (Happy Path) | Student | `POST /api/subscription/payment` | P0 |
| **US-67** | Submit Payment with Invalid Type (Edge Case) | Student | `POST /api/subscription/payment` | P2 |
| **US-68** | List Tags (Happy Path) | Public Visitor | `GET /api/tags` | P2 |
| **US-69** | Search Tags - No Results (Edge Case) | Public Visitor | `GET /api/tags` | P3 |
| **US-70** | Create Tag (Happy Path) | Administrator | `POST /api/tags` | P2 |
| **US-71** | Create Duplicate Tag (Edge Case) | Administrator | `POST /api/tags` | P2 |
| **US-72** | Update Tag (Happy Path) | Administrator | `PUT /api/tags/{id}` | P2 |
| **US-73** | Update Tag to Duplicate Name (Edge Case) | Administrator | `PUT /api/tags/{id}` | P3 |
| **US-74** | Delete Tag (Happy Path) | Administrator | `DELETE /api/tags/{id}` | P2 |
| **US-75** | Delete Tag Not Found (Edge Case) | Administrator | `DELETE /api/tags/{id}` | P3 |
| **US-76** | Presign Upload URL (Happy Path) | Auth User | `POST /api/uploads/presign` | P1 |
| **US-77** | Presign Upload File Too Large (Edge Case) | Auth User | `POST /api/uploads/presign` | P2 |
| **US-78** | Upload/Register Video (Happy Path) | Verified Teacher | `POST /api/videos/upload` | P1 |
| **US-79** | Upload Video with Expired Ticket (Edge Case) | Verified Teacher | `POST /api/videos/upload` | P2 |
| **US-80** | Get Secure Video URL (Happy Path) | Auth User | `GET /api/videos/{id}/secure-url`| P1 |
| **US-81** | Get Secure Video URL - Access Denied (Edge) | Auth User | `GET /api/videos/{id}/secure-url`| P2 |
| **US-82** | Verify Teacher (Happy Path) | Administrator | `POST /api/admin/verify-teacher/{teacherId}`| P1 |
| **US-83** | Verify Teacher - Not Found (Edge Case) | Administrator | `POST /api/admin/verify-teacher/{teacherId}`| P3 |
| **US-84** | Approve Uploaded Video (Happy Path) | Administrator | `POST /api/admin/approve-video/{videoId}`| P1 |
| **US-85** | Approve Video - Not Found (Edge Case) | Administrator | `POST /api/admin/approve-video/{videoId}`| P3 |
| **US-86** | Approve Payment (Happy Path) | Administrator | `POST /api/admin/approve-payment/{paymentId}`| P1 |
| **US-87** | Approve Payment - Not Found (Edge Case) | Administrator | `POST /api/admin/approve-payment/{paymentId}`| P3 |
