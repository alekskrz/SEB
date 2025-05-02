# Sports Exercise Battle (SEB) 🏋️‍♂️

A console-based HTTP server built in C# to track user push-up battles, scores (ELO), and achievements.  
Users can register, log in, track workouts, and compete in 2-minute tournaments.

---

Features

- User Registration & Login (token-based)
- Push-up tracking with duration
- ELO-based score system
- Achievement unlocks (First Push-Up, Beast Mode, Grinder, Champion)
- In-memory tournament rounds (auto-start & ELO-adjusted)
- Full curl-based batch test support

---

Getting Started

### 1. Requirements
- [.NET SDK 7.0+]
- [PostgreSQL 13+]
- A terminal (PowerShell / bash / CMD)

---

### 2. Clone & Setup

```bash
git clone https://github.com/alekskrz/SEB.git
cd sports-exercise-battle
dotnet restore

### 3. Configure Database
Ensure your PostgreSQL database is running.
Run the SQL setup script to create required tables:


-- Run in your PostgreSQL shell or GUI
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username TEXT UNIQUE NOT NULL,
    password TEXT NOT NULL,
    elo INTEGER NOT NULL DEFAULT 1000,
    token TEXT,
    achievements TEXT[]
);

CREATE TABLE pushup_records (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(id),
    count INTEGER NOT NULL,
    duration_seconds INTEGER NOT NULL,
    timestamp TIMESTAMP NOT NULL
);
Update the connectionString in DatabaseManager.cs to match your credentials.

### 4. Run the Server
dotnet run
Server starts at http://localhost:8080
