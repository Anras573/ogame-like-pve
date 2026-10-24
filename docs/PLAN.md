# OGame-like PvE — Project Plan

A browser-based, persistent, real-time space strategy game inspired by OGame.
Players build up planets, research technology, construct fleets and defenses —
but instead of raiding each other, they fight **NPC factions**: pirates, alien
hives, rogue AIs and ancient derelicts.

## 1. Vision & design pillars

1. **Same satisfying economy loop as OGame.** Mines → resources → buildings,
   research, ships → bigger mines. Real-time timers that keep running while the
   player is offline.
2. **PvE instead of PvP.** No player can ever lose their stuff to another
   player. The danger and the loot come from NPCs whose strength is tuned to
   each player's progress.
3. **Fair to casual players.** Because nobody can be farmed while asleep, there's
   no need to log in at 3 a.m. to save your fleet. Pacing is set by game design,
   not by other players' activity.
4. **Cooperation over competition.** Players can team up (alliances, joint
   attacks on world bosses), and leaderboards are there for bragging rights, not
   for preying on each other.

## 2. Why no game engine

Like OGame, this is a web application, not a real-time rendered game:

- The UI is pages, tables, timers and menus — standard web frontend work.
- The "simulation" is **formulas evaluated against timestamps** (resource
  production, build times, flight times) plus a **turn-based combat
  calculator**. Nothing runs at 60 fps.
- The hard parts are a correct, server-authoritative event system and good
  balancing. A game engine helps with neither.

So the stack is: a backend API + database + a background worker for timed
events + a web frontend.

## 3. Tech stack (proposed)

| Concern | Choice | Notes |
|---|---|---|
| Backend | **ASP.NET Core (.NET 10), minimal APIs** | Vertical-slice structure (one folder per feature). |
| Persistence | **PostgreSQL + EF Core** | Relational data fits well (planets, queues, fleets). |
| Timed events | **Hosted background worker** polling an `events` table (`due_at` index, `FOR UPDATE SKIP LOCKED`) | Simple, durable, horizontally safe. Can move to a dedicated queue later if needed. |
| Real-time UI updates | **SignalR** | Push "fleet arrived", "building finished", "you are under attack". |
| Frontend | **React + TypeScript (Vite)** | Alternative: Blazor, to keep one language. Decide in milestone 0. |
| Auth | ASP.NET Core Identity (cookie auth) | External logins later. |
| Testing | xUnit (+ Moq), Testcontainers for Postgres | Formula + combat engine get heavy unit-test coverage. |
| Local dev / hosting | Docker Compose (api, worker, db) | Deploy to a single VM/container host initially. |

The **game rules** (formulas, combat, NPC generation) live in a pure,
dependency-free `Game.Core` library so they can be unit-tested and reused by a
balancing/simulation tool.

## 4. Core game systems

### 4.1 Universe
- Galaxy → system → position (e.g. 5 galaxies × 499 systems × 15 positions to
  start; configurable).
- Each position is empty, a player planet, an **NPC planet/base**, or a special
  object (asteroid field, derelict, wormhole).
- Position affects planet size and temperature (temperature affects deuterium
  and solar output), same as OGame.

### 4.2 Resources & economy
- **Metal, Crystal, Deuterium**, plus **Energy** (not stored, a balance).
- Production is computed **lazily**: store `amount` + `last_updated_at`, and
  calculate the current value on read/when spent:
  `current = min(storage_cap, amount + rate × elapsed)`.
- Storage buildings set caps; energy shortfall scales production down.

### 4.3 Buildings
Metal/Crystal/Deuterium mines, Solar plant, Fusion reactor, Storages, Robotics
factory, Nanite factory, Shipyard, Research lab, Missile silo, and later Terraformer.
Cost and time grow exponentially by level (`base × factor^(level-1)`).
One build queue slot per planet at first (more can be unlocked later).

### 4.4 Research
Player-wide technologies: Energy, Laser, Ion, Plasma, Hyperspace, Combustion /
Impulse / Hyperspace drives, Espionage, Computer (fleet slots), Astrophysics
(colony slots + expeditions), Weapons / Shielding / Armour. Tech tree with
prerequisites, all data-driven.

### 4.5 Ships & defenses
Data-driven definitions (cost, hull, shield, weapon, speed, cargo, fuel use,
rapid fire table). Initial set: Small/Large cargo, Light/Heavy fighter,
Cruiser, Battleship, Bomber, Destroyer, Recycler, Espionage probe, Colony ship.
Defenses: Rocket launcher, Light/Heavy laser, Gauss cannon, Ion cannon, Plasma
turret, Small/Large shield dome.

### 4.6 Fleets & missions
Fleet movement uses OGame-style distance and speed formulas, and burns
deuterium as fuel. Missions:
- **Attack** an NPC target
- **Espionage** (probe report showing resources, fleet, defense)
- **Transport** / **Deploy** between your own planets
- **Colonize** an empty position
- **Recycle** a debris field
- **Expedition** into deep space (random outcome: resources, ships, NPC ambush,
  nothing, loss)

A fleet is a state machine: `Outbound → Arrived (resolve) → Returning → Home`
(plus `Recalled`).

### 4.7 Combat
OGame-style round-based simulation:
- Up to 6 rounds; each unit picks a random enemy target per shot.
- Shields absorb damage and regenerate each round; hull damage is permanent;
  units with heavily damaged hulls may explode (chance-based).
- Rapid fire gives extra shots against specific unit types.
- Outcome: losses, loot (up to 50% of target resources, limited by cargo),
  **debris field** (a % of destroyed ship costs), defenses partially rebuilt.
- **Deterministic given a seed** → replayable battle reports, reproducible bugs,
  and a public **battle simulator** page for players.

## 5. The PvE layer (the main difference from OGame)

### 5.1 NPC factions
| Faction | Personality | Gameplay role |
|---|---|---|
| **Pirates** | Lots of weak, cheap ships; low defense | Early-game targets, raid you occasionally |
| **Hive / Swarm** | Huge numbers, regenerates fast | Mid-game farming, rapid-fire puzzles |
| **Rogue AI** | Heavy defenses, few ships, high tech | Siege targets, need strong fleets + espionage |
| **Ancients / Derelicts** | Dormant, rare, extremely strong | End-game bosses with unique loot (blueprints, artifacts) |

### 5.2 NPC targets
- **NPC planets/outposts** generated in the galaxy, each with a **tier**
  (difficulty level). Their fleets, defenses and resources are generated from the
  tier + faction template + a seed.
- NPCs **regenerate** over time (resources refill, defenses rebuild, fleets
  respawn). This replaces the "inactive player farming" of OGame and keeps the
  game going without exhausting targets.
- **Scaling:** targets near a player's home/colonies are biased toward that
  player's power level (based on points), and the world has a spread of tiers
  so players can choose risk vs. reward.
- **Espionage matters:** NPC loadouts vary, so probing before attacking is
  part of the skill.

### 5.3 NPC aggression (optional per server)
- NPCs occasionally **raid players** (warning shown when the fleet launches,
  just like an incoming attack in OGame). The raid strength is capped relative to
  the player's own strength, and protection rules stop players from being
  crushed (newbie protection, max loss per day, no raids while in vacation mode).
- Defending successfully yields debris + rewards → defenses are worth building.

### 5.4 Content & progression
- **Missions/quests**: tutorial chain, then daily/weekly contracts ("destroy 3
  pirate outposts", "recycle 1M debris").
- **World events**: pirate armadas, hive invasions, a derelict appearing —
  server-wide targets that alliances attack together (ACS-style joint attack).
- **Unique rewards**: blueprints, artifacts that give small permanent bonuses,
  cosmetic titles. No pay-to-win.
- **Leaderboards**: economy, research, NPC kills, event damage.

## 6. Architecture

```
┌────────────┐   HTTP/JSON + SignalR   ┌──────────────────────────┐
│  Web SPA   │ ──────────────────────▶ │  API (ASP.NET Core)      │
└────────────┘                         │  - Features/* (slices)   │
                                       │  - uses Game.Core        │
                                       └─────────┬────────────────┘
                                                 │ EF Core
                                       ┌─────────▼────────────────┐
                                       │  PostgreSQL              │
                                       │  players, planets,       │
                                       │  queues, fleets, events, │
                                       │  npc_targets, reports    │
                                       └─────────▲────────────────┘
                                                 │
                                       ┌─────────┴────────────────┐
                                       │  Worker (hosted service) │
                                       │  - processes due events  │
                                       │  - NPC spawn/regenerate  │
                                       │  - NPC raid scheduler    │
                                       └──────────────────────────┘
```

### Key principles
- **Server-authoritative.** The client only displays state and sends intents
  ("build Metal Mine 5"). All validation, costs and timing happen on the server.
- **Lazy + event-driven time.** Resources are computed on demand. Discrete
  happenings (build done, fleet arrives) are rows in an `events` table with a
  `due_at`; the worker processes them in order, transactionally and
  idempotently.
- **Process "catch-up" correctly.** Before any action on a planet, first apply
  all of that planet's events that are due, in order (so production before and
  after a mine upgrade is right).
- **Data-driven content.** Buildings, research, ships, NPC factions and tiers
  live in versioned JSON/YAML definitions loaded by `Game.Core`, so balancing
  doesn't need code changes.
- **Game speed multiplier** (universe setting) for fast test universes.

### Proposed solution layout
```
src/
  Game.Core/          # pure rules: formulas, combat, NPC generation, definitions
  Game.Api/           # ASP.NET Core minimal API, feature slices, SignalR hubs
  Game.Worker/        # event processor + NPC simulation (can run in-process at first)
  Game.Infrastructure/# EF Core DbContext, migrations, repositories
  web/                # frontend SPA
tests/
  Game.Core.Tests/
  Game.Api.Tests/     # integration tests with Testcontainers
tools/
  Game.Balance/       # CLI: simulate economies/battles to tune numbers
docs/
```

## 7. Milestones

Each milestone ends with something playable/demoable.

### M0 — Foundations
- Repo setup: solution, `.editorconfig`, CI (build + test), Docker Compose with
  Postgres.
- Decide frontend (React vs Blazor).
- Auth: register / login / logout.
- **Done when:** a user can register, log in and see an empty dashboard.

### M1 — Economy loop (single planet)
- Universe generation, home planet assigned on signup.
- Lazy resource production, energy, storage caps.
- Buildings with costs, build times, build queue (one slot), cancel with refund.
- Event table + worker; SignalR "building finished" notification.
- **Done when:** a player can grow their mines over real time, offline included.

### M2 — Research & shipyard
- Research lab + tech tree with prerequisites.
- Shipyard and defense construction (batch queue).
- **Done when:** a player can unlock and build ships and defenses.

### M3 — Galaxy, fleets & first NPCs
- Galaxy view (browse systems).
- Fleet dispatch: speed, flight time, fuel, cargo; recall.
- Missions: Transport, Deploy, Colonize, Espionage.
- **Pirate** faction: NPC outposts generated by tier.
- **Done when:** a player can scout a pirate outpost and colonize a second planet.

### M4 — Combat
- Deterministic combat engine in `Game.Core` with extensive tests.
- Attack mission vs. NPCs, loot, debris fields, Recycle mission.
- Battle reports + standalone battle simulator page.
- NPC regeneration.
- **Done when:** the full PvE loop works: scout → attack → loot → recycle → grow.

### M5 — PvE depth
- More factions (Hive, Rogue AI), tier scaling to player power.
- NPC raids on players (with protection rules) + incoming-attack warnings.
- Expeditions.
- Tutorial + daily/weekly missions.
- **Done when:** there are meaningful choices about which NPCs to fight and when.

### M6 — Social & live ops
- Alliances, chat/messages, joint attacks (ACS) on world bosses.
- World events (pirate armada, derelict bosses), leaderboards.
- Admin tools: universe settings, balance overrides, player support.
- **Done when:** a small group of friends can play a universe together for weeks.

### Later / maybe
- Moons, jump gates, sensor phalanx equivalents.
- Multiple universes with different speeds.
- Mobile-friendly UI / PWA notifications.
- Seasonal universes with resets and rewards.

## 8. Risks & open questions

- **Balance** is the biggest risk. Mitigation: data-driven numbers, the
  `Game.Balance` simulator, and a fast-speed test universe.
- **Event processing correctness** (ordering, double-processing, concurrency
  between a player action and the worker). Mitigation: per-planet catch-up in
  one transaction, idempotent event handlers, integration tests.
- **NPC difficulty feel**: too easy is boring, too hard means NPCs farm the
  player. Needs playtesting.
- **IP:** we're inspired by OGame's mechanics, but we must use our own names,
  art, text and exact numbers where it matters. We don't copy OGame's assets or
  branding.
- **Open questions:**
  - Frontend: React + TypeScript, or Blazor?
  - Should NPC raids be on by default, or opt-in ("hardcore" universes)?
  - One shared persistent universe, or seasonal resets?
  - Any monetization at all? (Default: none / cosmetic only.)
