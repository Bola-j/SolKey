# **SolKey Backend Full Implementation Prompt (For Codex 5.3)**

You are a senior ASP.NET Core architect and backend engineer.

Your task is to build the COMPLETE backend infrastructure for a production-grade educational platform called “SolKey”.
### Please follow the detailed requirements below to implement the backend using best practices and clean architecture principles. 
### Each set of changes must be fully implemented and production-ready before being commited and moving to the next. 

The backend MUST follow:

* Pragmatic Clean Architecture  
* SOLID principles  
* Scalable modular architecture  
* ASP.NET Core Web API (.NET 10\)  
* Entity Framework Core (Code First)  
* SQL Server  
* JWT Authentication  
* Refresh Tokens  
* Cloudflare R2 integration  
* Signed video URLs  
* Anti-account-sharing system

The implementation must be production-ready, scalable, clean, and modular.

IMPORTANT:  
DO NOT generate placeholder pseudo-code.  
Generate REAL production-grade code.

---

# **1\. PROJECT STRUCTURE**

Create the following solution structure:

* SolKey.sln  
*   
* src/  
*  ├── SolKey.API  
*  ├── SolKey.Application  
*  ├── SolKey.Domain  
*  ├── SolKey.Infrastructure  
    
  ---

  # **2\. ARCHITECTURE RULES**

Use Pragmatic Clean Architecture:

* API  
* → Application  
* → Domain  
* → Infrastructure

  # **🚀 BACKEND ARCHITECTURE (BEST PRACTICE)**

  # **ONE BACKEND API**

  SolKey.API

  with:  
* Admin endpoints  
* Student endpoints  
* Teacher endpoints

  secured via:

* JWT  
* Policies  
* Role permissions  
  


Rules:

* Controllers must NEVER access DbContext directly  
* Business logic must exist in Application layer  
* Infrastructure handles EF Core, Auth, Cloudflare, external services  
* Domain contains entities and enums only  
* Use dependency injection everywhere  
* Use async/await everywhere  
* Use DTOs for API responses  
* Use response envelopes for all APIs  
  ---

  # **3\. TECH STACK**

Backend:

* ASP.NET Core Web API (.NET 10\)

Database:

* SQL Server  
* EF Core Code First

Auth:

* JWT Access Tokens  
* Refresh Tokens  
* Session Tracking

Storage:

* Cloudflare R2

Docs:

* Swagger/OpenAPI

Validation:

* FluentValidation

Logging:

* Serilog

Mapping:

* AutoMapper  
  ---

  # **4\. IMPLEMENT CLEAN ARCHITECTURE**

Create:

## **SolKey.Domain**

Contains:

* Entities  
* Enums  
* Base entities

  ## **SolKey.Application**

Contains:

* DTOs  
* Interfaces  
* Services  
* Validators  
* Access control logic

  ## **SolKey.Infrastructure**

Contains:

* DbContext  
* EF Configurations  
* Repositories if needed  
* JWT implementation  
* Cloudflare R2 implementation  
* Refresh token management

  ## **SolKey.API**

Contains:

* Controllers  
* Middleware  
* Swagger setup  
* Auth configuration  
* Global exception handling  
  ---

  # **5\. BASE ENTITY SYSTEM**

Create auditable base entity:

* BaseEntity  
* {  
*     Guid Id  
*   
*     DateTime CreatedAt  
*     DateTime? ModifiedAt  
*   
*     string CreatedBy  
*     string ModifiedBy  
*   
*     bool IsDeleted  
* }


Automatically populate audit fields using EF interceptors or SaveChanges override.

---

# **6\. USER SYSTEM**

Implement roles:

* Student  
* Teacher  
* Admin


Create User entity with:

* User  
* {  
*     Guid Id  
*   
*     string FirstName  
*     string LastName  
*   
*     string Email  
*     string PasswordHash  
*   
*     string? PhoneNumber  
*     string? PhotoUrl  
*     string? Bio  
*   
*     bool IsEmailVerified  
*     bool IsVerifiedTeacher  
*   
*     UserRole Role  
* }


Requirements:

* Teachers must have Bio  
* Email verification required  
* Password hashing required  
* Use ASP.NET Identity OR custom auth system  
* Prefer ASP.NET Identity integrated cleanly  
  ---

  # **7\. JWT \+ REFRESH TOKEN SYSTEM**

Implement:

## **JWT Access Tokens**

Expiry:

* 15–30 minutes

Include claims:

* UserId  
* Role  
* IsVerifiedTeacher  
* IsEmailVerified  
  ---

  ## **Refresh Tokens**

Create entity:

* RefreshToken  
* {  
*     Guid Id  
*     Guid UserId  
*     Guid SessionId  
*   
*     string Token  
*   
*     DateTime ExpiresAt  
*   
*     bool IsRevoked  
* }


Requirements:

* Secure refresh token generation  
* Revoke on logout  
* Rotate refresh tokens  
* Refresh endpoint

Endpoints:

* POST /api/auth/login  
* POST /api/auth/refresh  
* POST /api/auth/logout  
  ---

  # **8\. SESSION TRACKING SYSTEM**

Implement anti-account-sharing system.

Create:

* UserSession  
* {  
*     Guid Id  
*   
*     Guid UserId  
*   
*     string DeviceId  
*     string DeviceName  
*     string IPAddress  
*   
*     bool IsActive  
*   
*     DateTime LastSeenAt  
* }


Requirements:

* Track active sessions  
* Update LastSeenAt on every authenticated request  
* Auto-expire inactive sessions after 7 days  
* Limit simultaneous devices

Rules:

* Free student → 1 device  
* QA subscriber → 2 devices  
* Teacher → 3 devices

If limit exceeded:

* invalidate oldest session

Create middleware for session activity tracking.

---

# **9\. CONTENT HIERARCHY**

Implement:

* Book  
* → Chapter  
* → Lesson  
* → Question


Entities:

## **Book**

* Title  
* Description  
* CoverImage

  ## **Chapter**

* BookId  
* Title  
* Order

  ## **Lesson**

* ChapterId  
* Title  
* Order

  ## **Question**

* LessonId  
* Text  
* Tags  
  ---

  # **10\. VIDEO SYSTEM**

Implement secure video management system.

Video types:

* QuestionSolution  
* ExplanationSession

Create entity:

* Video  
* {  
*     Guid Id  
*   
*     string Title  
*   
*     VideoType Type  
*   
*     string BlobPath  
*   
*     bool IsPremium  
*   
*     bool IsApproved  
*   
*     Guid TeacherId  
*   
*     Guid? QuestionId  
*     Guid? SessionId  
* }


Requirements:

* Teachers upload videos  
* Admin approves videos  
* Videos stored on Cloudflare R2  
* DO NOT store full URLs  
* Store only BlobPath  
  ---

  # **11\. CLOUDFLARE R2 INTEGRATION**

Implement:

* Upload service  
* Signed URL generation  
* Temporary secure URLs

Create:

* IStorageService


Methods:

* UploadAsync()  
* GenerateSignedUrlAsync()

Requirements:

* Signed URLs expire after 15 minutes  
* URLs regenerated when needed  
* S3-compatible SDK

Use:

* AWS SDK for S3-compatible API

DO NOT use local storage.

---

# **12\. VIDEO ACCESS CONTROL**

Create:

* AccessControlService


Implement logic:

## **Question Solution Videos**

Require:

* QA subscription

  ## **Explanation Session Videos**

Require:

* Purchased session

  ## **Combined Access**

If student purchased session but NOT QA subscription:

* Can watch explanation videos  
* Can see sheet questions  
* Cannot access answers

Implement:

* CanAccessVideo()  
* CanAccessQuestionAnswers()  
* CanPurchaseSession()  
  ---

  # **13\. WATERMARK SYSTEM**

Implement frontend-compatible watermark support.

Video secure endpoint must return:

* {  
*   "url": "...",  
*   "watermarkText": "email | phone"  
* }


Requirements:

* Dynamic watermark text  
* Used by Angular frontend  
* Support moving watermark overlay  
  ---

  # **14\. EXPLANATION SESSIONS**

Create:

* ExplanationSession  
* {  
*     Guid Id  
*   
*     string Title  
*     string Description  
*   
*     decimal Price  
*   
*     Guid TeacherId  
*   
*     int AccessDurationDays  
*   
*     bool IsApproved  
* }


Requirements:

* Default duration \= 14 days  
* Session purchase grants temporary access  
* Sessions contain videos  
* Sessions contain question sheets  
  ---

  # **15\. SESSION PURCHASE SYSTEM**

Create:

* SessionPurchase  
* {  
*     Guid Id  
*   
*     Guid UserId  
*     Guid SessionId  
*   
*     DateTime PurchasedAt  
*     DateTime ExpiresAt  
* }


Requirements:

* Validate active access  
* Expire automatically  
  ---

  # **16\. QA SUBSCRIPTION SYSTEM**

Create:

* Subscription  
* {  
*     Guid Id  
*   
*     Guid UserId  
*   
*     DateTime StartDate  
*     DateTime EndDate  
*   
*     bool IsActive  
* }


Requirements:

* QA subscription unlocks:  
  * Answers  
  * Solution videos  
* Free users:  
  * 3 question views/day  
  * Cannot ask unlimited questions

Create:

* subscription status endpoint  
  ---

  # **17\. PAYMENT SYSTEM**

Manual approval MVP.

Create:

* Payment  
* {  
*     Guid Id  
*   
*     Guid UserId  
*   
*     decimal Amount  
*   
*     PaymentType Type  
*   
*     string ScreenshotPath  
*   
*     bool IsApproved  
* }


Requirements:

* Upload screenshot  
* Admin approval  
* Grant subscriptions manually  
* Grant session access manually  
  ---

  # **18\. QUESTION & ANSWER SYSTEM**

Implement:

## **Ask Question**

Rules:

* Must be verified email  
* Respect free tier limits  
* Students only

  ## **Answer Question**

Rules:

* Verified teachers only

  ## **Voting**

* Upvote/downvote answers  
  ---

  # **19\. TAG SYSTEM**

Implement reusable tags.

Entities:

* Tag  
* QuestionTag  
* VideoTag

Requirements:

* Filter videos/questions by tags  
* Paginated filtering  
  ---

  # **20\. ANALYTICS SYSTEM**

Teacher analytics:

* Views  
* Likes  
* Comments  
* Student counts  
* Revenue

Admin analytics:

* Users  
* Teachers  
* Revenue  
* Payments  
* Sessions  
* Videos  
  ---

  # **21\. ADMIN SYSTEM**

Admin capabilities:

* Verify teachers  
* Approve videos  
* Approve sessions  
* Approve payments  
* Manage books  
* Manage tags  
* Full CRUD access  
  ---

  # **22\. API RESPONSE ENVELOPE**

ALL endpoints must return:

## **Success**

* {  
*   "succeeded": true,  
*   "message": "",  
*   "data": {}  
* }


  ## **Paged**

* {  
*   "succeeded": true,  
*   "message": "",  
*   "data": \[\],  
*   "page": 1,  
*   "pageSize": 10,  
*   "totalCount": 100,  
*   "totalPages": 10  
* }


  ## **Failure**

* {  
*   "succeeded": false,  
*   "message": "",  
*   "errors": \[\]  
* }  
    
  ---

  # **23\. GLOBAL MIDDLEWARE**

Implement:

* Global exception handling  
* JWT auth middleware  
* Session activity middleware  
* Logging middleware  
  ---

  # **24\. SWAGGER**

Configure:

* JWT auth support  
* Full endpoint documentation  
  ---

  # **25\. EF CORE CONFIGURATION**

Requirements:

* Fluent API  
* Separate configurations  
* Soft delete support  
* Proper indexing  
* Cascade restrictions

Generate:

* Initial migration  
  ---

  # **26\. VALIDATION**

Use FluentValidation for:

* Register  
* Login  
* Upload video  
* Create session  
* Ask question  
* Payments  
  ---

  # **27\. FILE STRUCTURE REQUIREMENTS**

Generate clean folders:

* Application/  
*  ├── DTOs  
*  ├── Interfaces  
*  ├── Services  
*  ├── Validators  
*   
* Infrastructure/  
*  ├── Persistence  
*  ├── Identity  
*  ├── Storage  
*  ├── Services  
    
  ---

  # **28\. API ENDPOINTS**

Generate complete controllers for:

## **Auth**

* register  
* login  
* refresh  
* logout  
* verify-email

  ## **User**

* profile  
* update profile

  ## **Books**

* CRUD

  ## **Chapters**

* CRUD

  ## **Lessons**

* CRUD

  ## **Questions**

* ask  
* answer  
* vote

  ## **Videos**

* upload  
* secure-url

  ## **Sessions**

* create  
* purchase  
* list

  ## **Subscription**

* status  
* payment

  ## **Admin**

* approve teacher  
* approve payment  
* approve video  
  ---

  # **29\. IMPORTANT IMPLEMENTATION RULES**

DO NOT:

* use generic repository pattern blindly  
* place business logic in controllers  
* expose raw storage URLs  
* store signed URLs in DB

ALWAYS:

* use async methods  
* use dependency injection  
* validate permissions server-side  
* validate access before generating signed URLs  
  ---

  # **30\. OUTPUT REQUIREMENTS**

Generate:

* Full solution structure  
* Full entities  
* Full DbContext  
* Fluent configurations  
* Services  
* Interfaces  
* Controllers  
* DTOs  
* Middleware  
* JWT implementation  
* Refresh token system  
* Cloudflare R2 integration  
* Migrations  
* Swagger setup  
* Validation  
* Seed data

The generated backend must compile and run successfully.

Start implementation from:

1. Domain  
2. Infrastructure  
3. Application  
4. API

Then proceed feature-by-feature until the entire backend is completed.

