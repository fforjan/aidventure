# Aidventure

A branching gamebook skill for GitHub Copilot CLI — a cinematic escape thriller set in a modern city.

## Invoke

```
/aidventure
```

## About

**The Last Flight** drops you into a hotel room at 3:47 AM with one encrypted message: get to the airport. Flight 7:15 AM. Gate 12. Do not stop.

Between you and the gate: the city, the mafia, the police, and a city hunter whose motives you don't know yet.

- **Freeform input** — no A/B/C menus, type what you want to do
- **Real branching** — 14 scenes, 6 dead ends, 4 distinct endings
- **State tracking** — your choices (hunter alliance, injuries, cash) affect the final confrontation
- **Cinematic narration** — second person, present tense, no inner monologue

## Scene Map

```
S1 (Hotel) → S2 (Lobby) or S3 (Back Alley)
S3 → S5 (Market) or S6 (Riverside)
S5 → S8 (Hunter) / S9 (Shadow) / S10 (Police)
...all roads lead to S11 (Freight Gate) → S12/S13 → S14 (Final)
```

## Endings

| Ending | Condition |
|--------|-----------|
| 🏆 Best — *Ghost Protocol* | Allied with hunter, fought together |
| 🏆 Good — *Clean Hands* | Allied with hunter, split approach |
| 🏆 Neutral — *The Price of a Ticket* | Alone, negotiated |
| 🏆 Hard — *Scar Tissue* | Alone, fought through |
| 💀 ×6 | Various bad decisions along the way |

## Files

- `.github/skills/aidventure/SKILL.md` — full gamebook: scenes, routing, narration rules
- `generate_video2.py` — generates a cinematic MP4 replay of a session
- `session_replay.html` — browser-based animated replay
