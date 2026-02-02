# 🏋️‍♂️ Gym Tracker (Serverless .NET + Vue)

A high-performance, cost-optimized gym tracking application. Built to demonstrate modern **Clean Architecture**, **Serverless System Design**, and a custom **JWT Authentication** implementation.

---

## 🏛️ System Architecture

This project is designed to stay within the **AWS Free Tier** indefinitely. It uses a decoupled frontend and backend hosted on serverless infrastructure.

```mermaid
graph TD
    User((User/Browser))
    
    subgraph AWS_Cloud [AWS Cloud - Cost Optimized]
        CF[CloudFront - CDN]
        S3[S3 Bucket - Static Web Hosting]
        APIGW[API Gateway - REST Endpoint]
        Lambda[AWS Lambda - .NET 10 Minimal API]
    end

    subgraph External_Providers [Database]
        DB[(PostgreSQL - Neon/Supabase)]
    end

    User -->|HTTPS/Vue App| CF
    CF --> S3
    User -->|API Calls| APIGW
    APIGW --> Lambda
    Lambda -->|EF Core| DB
```

---

## 📗 Data Model

The database schema follows a relational structure to ensure data integrity and easy reporting on lifting progress over time.

```mermaid
erDiagram
    USERS {
        uuid id PK
        text email
        text password_hashed
        timestamp created_at
    }

    MUSCLE_CATEGORIES {
        uuid id PK
        uuid user_id
        text name
    }

    MUSCLE_GROUPS {
        uuid id PK
        uuid user_id
        uuid category
        text name
    }

    EXERCISES {
        uuid id PK
        uuid user_id
        uuid muscle_group_id
        text name
    }

    WORKOUTS {
        uuid id PK
        uuid user_id
        text name
    }

    SESSIONS {
        uuid id PK
        uuid workout_id
        timestamp created_at
    }

    SESSION_EXERCISES {
        uuid id PK
        uuid session_id
        uuid exercise_id
        int exercise_number
    }

    SETS {
        uuid id PK
        uuid session_exercise_id
        int set_number
        double weight
        int reps
    }

    USERS ||--o{ WORKOUTS : owns_and_creates
    USERS ||--o{ EXERCISES : owns_and_creates
    USERS ||--o{ MUSCLE_CATEGORIES : owns_and_creates
    USERS ||--o{ MUSCLE_GROUPS : owns_and_creates

    MUSCLE_CATEGORIES ||--o{ MUSCLE_GROUPS : categorises
    MUSCLE_GROUPS ||--o{ EXERCISES : categorises

    WORKOUTS ||--o{ SESSIONS : has
    SESSIONS ||--o{ SESSION_EXERCISES : has
    EXERCISES ||--o{ SESSION_EXERCISES : performed_as
    SESSION_EXERCISES ||--o{ SETS : has
```

---

## 🔒 Authentication Flow

For learning purposes, this app implements a custom **JWT-based authentication** system using **Argon2id** for password hashing.

```mermaid
sequenceDiagram
    participant Client as Vue 3 App
    participant API as .NET Lambda
    participant DB as Postgres

    Client->>API: POST /auth/login
    API->>DB: Fetch Hash
    API->>API: Verify Password (Argon2id)
    API-->>Client: 200 OK + JWT Token
    Note right of Client: Token stored in Secure Cookie
```

---

## 🛠️ Tech Stack

* **Frontend:** Vue 3 (Composition API), TypeScript, Pinia (State), Vite.
* **Backend:** .NET 10 Minimal APIs, Entity Framework Core.
* **Infrastructure:** AWS Lambda (Serverless), S3, CloudFront, GitHub Actions (CI/CD).
* **Database:** PostgreSQL via Neon or Supabase.