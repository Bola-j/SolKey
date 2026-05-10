# **SPEC-1-SolKey**

## **Background**

SolKey is a modern educational platform inspired by platforms like Brainly, Nagwa, and Bassthalk, but focused specifically on Egyptian Prep and High School students studying STEM and National System curricula.

The platform combines:

* Structured educational content  
* Q\&A systems  
* Paid explanation sessions  
* Premium answer systems  
* Teacher monetization  
* Secure educational video delivery

The platform architecture is designed to:

* Scale efficiently  
* Reduce video piracy  
* Support future mobile applications  
* Support future AI integrations  
* Maintain low infrastructure costs

The system uses:

* ASP.NET Core Web API (.NET 10\)  
* Angular SPA frontend  
* SQL Server  
* Cloudflare R2 for video storage  
* JWT \+ Refresh Tokens  
* Pragmatic Clean Architecture

---

# **Requirements**

## **MoSCoW Prioritization**

---

## **MUST HAVE**

### **Authentication & Authorization**

* JWT Authentication  
* Refresh Token system  
* Email verification  
* Role-based authorization  
* Active device session tracking  
* Auto-expire inactive sessions

### **Roles**

* Student  
* Teacher  
* Admin

### **Student Features**

* Browse books  
* Browse chapters  
* Browse lessons  
* Browse questions  
* Watch question solution videos  
* Watch explanation sessions  
* Ask questions  
* Upload payment screenshots  
* Purchase explanation sessions  
* Subscribe to QA plan

### **Teacher Features**

* Upload explanation videos  
* Upload question-solving videos  
* Create sessions  
* Answer student questions  
* View analytics  
* View earnings  
* Edit profile

### **Admin Features**

* Full platform control  
* Approve teachers  
* Approve videos  
* Approve payments  
* Manage books  
* Manage curriculum  
* Manage tags  
* Manage subscriptions

### **Content Hierarchy**

Book  
→ Chapter  
→ Lesson  
→ Questions  
→ Video Solutions

### **Explanation Sessions**

Each session:

* Has price  
* Lasts 14 days after purchase  
* Contains explanation videos  
* Contains related question sheets

### **Subscription Rules**

QA Subscription unlocks:

* Question answers  
* Solution videos

Session Purchase unlocks:

* Explanation session videos  
* Question sheets only  
* NOT answers unless subscribed

### **Video Security**

* Signed URLs  
* Cloudflare R2 secure delivery  
* Dynamic watermark overlay  
* Device/session limits

### **Anti-Sharing Protection**

* Active device tracking  
* Session limits  
* Watermarked videos  
* Signed URLs  
* Refresh token validation

---

## **SHOULD HAVE**

* Dark/Light mode  
* Recommendation system  
* Teacher analytics dashboard  
* Wallet system  
* Video approval workflows  
* Tags and filtering

---

## **COULD HAVE**

* AI recommendations  
* Mobile apps  
* HLS adaptive streaming  
* Push notifications  
* Live sessions

---

## **WON'T HAVE (MVP)**

* DRM  
* Livestreaming  
* Offline downloads  
* AI-generated answers

---

# **Method**

## **Architecture**

The platform uses:

# **Pragmatic Clean Architecture**

SolKey.API  
SolKey.Application  
SolKey.Domain  
SolKey.Infrastructure

---

## **Layer Responsibilities**

### **SolKey.Domain**

Contains:

* Entities  
* Enums  
* Core domain rules

### **SolKey.Application**

Contains:

* Business logic  
* Services  
* DTOs  
* Interfaces  
* Access control

### **SolKey.Infrastructure**

Contains:

* EF Core  
* Cloudflare R2 integration  
* Authentication implementations  
* External services

### **SolKey.API**

Contains:

* Controllers  
* Middleware  
* API endpoints  
* Swagger configuration

---

# **System Architecture Diagram**

@startuml

package "Angular Frontend" {  
  \[Student App\]  
  \[Teacher App\]  
  \[Admin App\]  
}

package "ASP.NET Core Backend" {  
  \[API Controllers\]  
  \[Application Services\]  
  \[AccessControlService\]  
  \[JWT/Auth\]  
  \[Cloudflare R2 Service\]  
}

database "SQL Server" {  
  \[Database\]  
}

cloud "Cloudflare R2" {  
  \[Video Storage\]  
}

\[Student App\] \--\> \[API Controllers\]  
\[Teacher App\] \--\> \[API Controllers\]  
\[Admin App\] \--\> \[API Controllers\]

\[API Controllers\] \--\> \[Application Services\]  
\[Application Services\] \--\> \[AccessControlService\]  
\[Application Services\] \--\> \[Database\]  
\[Application Services\] \--\> \[Cloudflare R2 Service\]

\[Cloudflare R2 Service\] \--\> \[Video Storage\]

@enduml

---

# **Database Design**

## **Core Entities**

---

## **User**

User  
{  
    Guid Id  
    string Email  
    string PasswordHash

    UserRole Role

    bool IsEmailVerified  
    bool IsVerifiedTeacher

    string? PhoneNumber  
    string? PhotoUrl  
    string? Bio

    DateTime CreatedAt  
}

---

## **UserSession**

UserSession  
{  
    Guid Id  
    Guid UserId

    string DeviceId  
    string DeviceName  
    string IPAddress

    bool IsActive

    DateTime LastSeenAt  
}

---

## **RefreshToken**

RefreshToken  
{  
    Guid Id  
    Guid UserId  
    Guid SessionId

    string Token  
    bool IsRevoked

    DateTime ExpiresAt  
}

---

## **Book Hierarchy**

Book  
→ Chapter  
→ Lesson  
→ Question

---

## **Video**

Video  
{  
    Guid Id

    string Title

    VideoType Type

    string BlobPath

    bool IsPremium

    bool IsApproved

    Guid TeacherId  
}

---

## **ExplanationSession**

ExplanationSession  
{  
    Guid Id

    string Title  
    string Description

    decimal Price

    Guid TeacherId

    int AccessDurationDays

    bool IsApproved  
}

---

## **SessionPurchase**

SessionPurchase  
{  
    Guid Id

    Guid UserId  
    Guid SessionId

    DateTime PurchasedAt  
    DateTime ExpiresAt  
}

---

## **Subscription**

Subscription  
{  
    Guid Id

    Guid UserId

    DateTime StartDate  
    DateTime EndDate

    bool IsActive  
}

---

## **Payment**

Payment  
{  
    Guid Id

    Guid UserId

    PaymentType Type

    decimal Amount

    string ScreenshotUrl

    bool IsApproved  
}

---

# **Video Security Architecture**

## **Video Flow**

Frontend  
→ Request Secure URL  
→ Backend Access Validation  
→ Generate Signed URL  
→ Return Temporary URL  
→ Angular Player

---

## **Security Features**

### **Signed URLs**

* Generated dynamically  
* Expire after 15 minutes  
* Regenerated automatically

### **Dynamic Watermark**

Overlay:

* Email  
* Phone number

Watermark:

* Semi-transparent  
* Randomized movement  
* Changes position periodically

### **Session Limits**

| Role | Allowed Devices |
| ----- | ----- |
| Free Student | 1 |
| QA Subscriber | 2 |
| Teacher | 3 |

---

# **Access Rules**

## **QA Subscription**

Unlocks:

* Question answers  
* Premium solution videos

---

## **Session Purchase**

Unlocks:

* Explanation session videos  
* Question sheets

Does NOT unlock:

* Answers

---

## **Combined Access**

User with:

* QA subscription  
  AND  
* Session purchase

Gets:

* Full access

---

# **Authentication Flow**

## **Login**

Login  
→ Validate credentials  
→ Check session limits  
→ Create UserSession  
→ Generate JWT  
→ Generate RefreshToken  
→ Return tokens

---

## **Refresh Flow**

JWT expires  
→ Send RefreshToken  
→ Validate session  
→ Generate new JWT

---

## **Session Cleanup**

Daily cleanup job:

* Remove inactive sessions  
* Revoke expired refresh tokens

---

# **Cloudflare R2 Integration**

## **Why Cloudflare R2**

Chosen because:

* Very low cost  
* No egress fees  
* S3-compatible  
* Excellent CDN performance

---

## **Upload Flow**

Teacher Upload  
→ Backend Validation  
→ Upload to R2  
→ Save BlobPath  
→ Admin Approval

---

# **API Design**

## **Authentication**

POST /api/auth/register  
POST /api/auth/login  
POST /api/auth/refresh  
POST /api/auth/logout  
POST /api/auth/verify-email

---

## **User**

GET /api/user/profile  
PUT /api/user/profile

---

## **Sessions**

GET /api/sessions  
GET /api/sessions/{id}  
POST /api/sessions  
POST /api/sessions/{id}/purchase

---

## **Videos**

POST /api/videos/upload  
GET /api/videos/{id}/secure-url

---

## **Questions**

POST /api/questions/ask  
POST /api/questions/answer  
GET /api/questions/{id}

---

## **Subscription**

GET /api/subscription/status  
POST /api/subscription/payment

---

## **Admin**

POST /api/admin/verify-teacher  
POST /api/admin/approve-video  
POST /api/admin/approve-payment

---

# **Class Diagram**

@startuml

class User  
class UserSession  
class RefreshToken  
class Subscription  
class ExplanationSession  
class SessionPurchase  
class Video  
class Question  
class Answer

User "1" \--\> "\*" UserSession  
User "1" \--\> "\*" RefreshToken  
User "1" \--\> "\*" Subscription  
User "1" \--\> "\*" SessionPurchase

ExplanationSession "1" \--\> "\*" Video  
Question "1" \--\> "\*" Answer

@enduml

---

# **Use Case Diagram**

@startuml

actor Student  
actor Teacher  
actor Admin

Student \--\> (Browse Content)  
Student \--\> (Buy Session)  
Student \--\> (Subscribe)  
Student \--\> (Watch Videos)  
Student \--\> (Ask Question)

Teacher \--\> (Upload Videos)  
Teacher \--\> (Create Session)  
Teacher \--\> (Answer Questions)

Admin \--\> (Approve Teachers)  
Admin \--\> (Approve Payments)  
Admin \--\> (Manage Platform)

@enduml

---

# **Implementation**

## **Phase 1 — Foundation**

* Create solution structure  
* Setup Clean Architecture  
* Configure EF Core  
* Configure JWT

---

## **Phase 2 — Core Domain**

* Users  
* Sessions  
* Subscriptions  
* Payments

---

## **Phase 3 — Content System**

* Books  
* Lessons  
* Questions  
* Videos

---

## **Phase 4 — Security**

* Refresh tokens  
* Session tracking  
* Signed URLs  
* Watermarks

---

## **Phase 5 — Admin System**

* Teacher approval  
* Payment approval  
* Video moderation

---

## **Phase 6 — Frontend Integration**

* Angular APIs  
* Authentication flow  
* Secure player

---

# **Milestones**

| Milestone | Description |
| ----- | ----- |
| M1 | Clean architecture setup |
| M2 | Authentication system |
| M3 | QA system |
| M4 | Session system |
| M5 | Video security |
| M6 | Admin dashboard |
| M7 | Angular integration |
| M8 | Production deployment |

---

# **Gathering Results**

Success metrics:

* Stable signed video playback  
* Low account sharing abuse  
* Fast API response times  
* Teacher onboarding success  
* Student engagement  
* Subscription conversion

