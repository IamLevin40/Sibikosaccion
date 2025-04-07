
## 🌆 Sibikosaccion: A Civic Simulation Strategy Game

Sibikosaccion is a single-player, offline-first strategic board game where you play as the mayor of a growing city. Inspired by Monopoly, it mixes economic decision-making with civic dilemmas to promote civic responsibility, moral leadership, and economic awareness. Built with Unity and C#, it is designed as an engaging educational tool and simulation experience.
## 🚀 Getting Started

🔄 Clone the Repository

To get a local copy up and running:

```bash
git clone https://github.com/your-username/sibikosaccion.git
```

- Open the project folder in Unity (version 2022.3.36f1 or later).
- Wait for the Unity Editor to load and compile scripts.
- Press the ▶️ Play button in the editor to test the game.
## 📱 Download the APK

You can download the latest build of the game [here](https://drive.google.com/drive/folders/1GOY7Y5ncnOQyADbnfwmKKzWGNH8o2PDN?usp=sharing).
## 🎮 Game Objective
You are the mayor! Your goals:

- 🏙️ Own 13 out of 16 properties
- ⚖️ Balance prices and taxes
- 💼 Avoid bankruptcy and corruption

Win by balancing civic integrity and economic growth. Lose if your corruption hits 50, you go bankrupt, or properties get abandoned.
## 🧩 Core Features & Gameplay Mechanics
🧑‍💼 Mayor's Role
- Buy Abandoned Properties on a 5x7 board.
- Set Prices & Taxes for revenue and satisfaction.
- Avoid Corruption by managing customers and tax levels.

🏁 Win Condition
- Own at least 13 properties
- Maintain balanced price & tax
- Stay within budget & corruption limits

☠️ Loss Conditions
- Bankruptcy: No funds left.
- Corruption: Reach 50 points.
- Abandonment: High prices/taxes drive people away.
## 🎲 Turn-Based System
- Start at bottom-right tile.
- Roll a 6-sided die and move clockwise.
- Land on:
    - 🏚️ Abandoned Property: Buy & set pricing/tax.
    - 🏠 Owned Property: Edit pricing/tax.
    - ❓ Mystery Tile: Draw a random event card.
## 🏠 Property Management
- Market Price: Impacts customer satisfaction.
- Tax Rate: Affects income and abandonment risk.
## 🃏 Mystery Tile System
✨ Card Selection
- Pick one of 3 random cards from a pool of 10.
- Option to reroll if you answer a civic question correctly.
- Wrong answer? No more rerolls — move on without a card.
✅ Non-Corrupt Cards

Card | Effect
--- | ---
Community | Grant PHP 25–50 bonus per property if prices are fair
Volunteer | Initiative +1 customer next turn per property
Infrastructure Investment | +10% value on one property permanently
Youth Start-Up Aid | Next property 50% off
Spirit of Bayanihan | +5% tax income from all properties for one collection

❌ Corrupt Cards & Effects

Card | Effect | Corruption
--- | --- | ---
Forced Eviction | Sell property at 1.5x value | +2
Cash Overflow | Get PHP 5,000, no taxes next loop | +10
Ghost Employees | Get PHP 2,500, lose 25% income | +6
Land Grab | Free property, -1 customer/property	| +5
Personal Expenditure | PHP 1,000, -10% income next loop | +4

## ⚠️ Corruption System
- Cap: 50 points max
- Sources:
    - +1 per unhappy customer
    - +5 per overtaxed property
    - +X depending on corrupt card effect
## 💡 Civic Education Mechanics
🧠 Question System
- Multiple-choice civic questions appear during rerolls.
- Correct answer = new card options.
- Wrong answer = lose the card and proceed.
## 🛠️ Tech Stack

Tool | Purpose
--- | ---
Unity | Game Engine
C# | Programming Language
IbisPaint, Figma, Canva | Design and Assets 

## 🎓 Educational Impact
Sibikosaccion is a civic education tool that promotes:
- 🧭 Ethical governance
- 💰 Budget/resource management
- 🏘️ Community engagement
- 🗳️ Civic decision-making
## 👥 Credits
Team Name: CS-Semble
- Kurt Allen R. Alorro
- Lean Vince A. Cabales
- Zuriel Eliazar D. Calix
- Jethro Roland T. Dañocup
- Christine Joy D. Maravilla
