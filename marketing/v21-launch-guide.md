# Nanuq v2.1 — Launch Day Distribution Guide

Created: 2026-05-09  
Purpose: Complete step-by-step guide for firing the v2.1 distribution when the CEO posts the GitHub Discussion.  
Estimated time: 45 minutes total (spread over 3 days)

**Gate:** CEO posts GitHub Discussion → copies the URL → fills `[link]` placeholders → distribution fires.

---

## Pre-flight (CEO action, ~10 min)

1. Go to github.com/waelouf/Nanuq → Discussions → New Discussion
2. Set category: Announcements (or General)
3. Title: "Nanuq v2.1 Roadmap — Linux/macOS credential security"
4. Paste the content (the roadmap document already exists — see README.md Roadmap section and v2.1-announcement-devto.md for the narrative)
5. Publish Discussion → copy the URL
6. Open `marketing/v2.1-social-posts.md` and replace all `[link]` placeholders with the Discussion URL
7. Open `marketing/v2.1-reddit-posts.md` and replace `[link]` placeholders similarly
8. Open `marketing/v2.1-announcement-devto.md` and update the Discussion URL in the body

Once URLs are filled in, pass to CMO or execute the steps below.

---

## Day 1 — The main push (30 min)

### Step 1 — Post Twitter/X (5 min)

Open `marketing/v2.1-social-posts.md`:
- **Today:** Post 1 (Announcement) — fires immediately when Discussion is live
- Post the LinkedIn post (also in social-posts.md)

### Step 2 — Post Reddit (15 min)

Open `marketing/v2.1-reddit-posts.md`:
- **r/selfhosted** — Post 1, primary target (~400K subscribers). Post first.
- **r/devops** — Post 2, same day. Cross-post the selfhosted thread.

Best timing: 10am–12pm ET on the posting day.

### Step 3 — Publish Dev.to announcement (10 min)

Open `marketing/v2.1-announcement-devto.md`:
1. Go to dev.to → New Post → paste the body
2. Set title: `Nanuq v2.1 — Linux/macOS self-hosting with proper credential security`
3. Tags: `opensource, devtools, dotnet, security`
4. Publish — post the Dev.to URL as a comment on the GitHub Discussion

---

## Day 2–3 (15 min)

### Step 4 — Post remaining Twitter posts

Open `marketing/v2.1-social-posts.md`:
- **Day 2:** Post 2 (Technical detail — IKeyProtector design)
- **Day 3:** Post 3 (Call to action — testing on Linux)

### Step 5 — Post r/dotnet (Day 2, 5 min)

Open `marketing/v2.1-reddit-posts.md` → r/dotnet post (Post 3):
- Post on Day 2 — after r/selfhosted and r/devops posts have settled
- .NET ecosystem angle: IKeyProtector interface + DI-composable credential security

### Step 6 — Submit newsletter pitches

Open `marketing/newsletter-pitches.md` (the v2.1 pitches — .NET Weekly + InfoQ):

⚠️ These pitches require v2.1 to be released + GHSA published. Do NOT send before the release is tagged on GitHub and the GHSA is public.

Once the release is confirmed:
1. **.NET Weekly** — submit via dotnetweekly.com
2. **InfoQ** — submit via infoq.com/news or contact editors
3. **Console.dev tool directory** — update the existing listing with v2.1 link

---

## Day 3–7 — Awesome list PRs (from v2.0.1 guide)

If the awesome list PRs from the v2.0.1 distribution haven't been submitted yet, submit them now in parallel:
- awesome-selfhosted (highest priority)
- awesome-kafka
- awesome-dotnet
- awesome-redis

See `marketing/awesome-list-submissions.md` for exact entry text and PR body.

---

## What fires at v2.1 GA (GHSA publication gate)

These are gated on v2.1.0 tagged release + GHSA published (not just Discussion):
- **GHSA publication** — make public via GitHub Security Advisories
- **.NET Weekly** and **InfoQ** newsletter pitches (see Step 5 note)
- **Show HN** post (`marketing/show-hn-draft.md`) — post Tue/Wed 7-9am ET, 1-2 weeks after Discussion

---

## Quick Reference

| Item | File | Gate | Estimated time |
|------|------|------|---------------|
| GitHub Discussion | (CEO action) | — | 10 min |
| Twitter/X Post 1 + LinkedIn | `v2.1-social-posts.md` | Discussion URL | 5 min |
| Reddit r/selfhosted + r/devops | `v2.1-reddit-posts.md` | Discussion URL | 15 min |
| Dev.to announcement | `v2.1-announcement-devto.md` | Discussion URL | 10 min |
| Twitter Posts 2 + 3 | `v2.1-social-posts.md` | Day 2-3 | 5 min |
| Reddit r/dotnet | `v2.1-reddit-posts.md` | Day 2 | 5 min |
| Newsletter pitches (.NET Weekly, InfoQ) | `newsletter-pitches.md` | v2.1 GA + GHSA public | 15 min |
| Show HN | `show-hn-draft.md` | v2.1 GA + 1-2 weeks post-Discussion | 10 min |
| GHSA publication | GitHub Security Advisories | v2.1 GA | 5 min |
