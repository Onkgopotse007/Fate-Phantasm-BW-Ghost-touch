# Fate-Phantasm

*(originally done with dotnet-7-rpg — upgraded to .NET 8 for long-term support and stability)*

*https://fate-phantasm-bw-ghost-touch.onrender.com*

---

## About

I'm currently building a turn-based RPG inspired by the *Fate* anime series (*Fate/Zero* & *Fate/Stay Night*) using **ASP.NET Core 8**. Each player controls **two characters**, each from different classes. I’m carefully balancing 14 unique heroic spirits, including the rare "Avenger" class — Angra Mainyu.

The core mechanics are in place, and gameplay is currently at a **very basic level**, playable entirely through API calls. I'm iterating on the battle system and turn logic before moving to anything UI-related.

---

## What I'm Learning / Demonstrating

This project is helping me sharpen key backend skills and apply best practices:

- 🔧 Global exception handling with custom middleware  
- ✅ Input validation using **FluentValidation**  
- 📋 DTO mapping with **AutoMapper**  
- 🗂️ ORM via **Entity Framework Core**  
- 🔐 Authentication & Authorization with JWT bearer tokens and role-based access  
- 📝 Logging structured events with **Serilog**

---

## Branches

- `development-crud-only`: Minimal CRUD version for reference  
- `development`: Full game logic and ongoing development

---

## Getting Started

```bash
dotnet build
# If that fails:
dotnet restore
dotnet watch run
```

Open to feedback, code reviews, and good conversations.

📞 +267 75295351

📧 tonyonkgopotserichard@gmail.com
