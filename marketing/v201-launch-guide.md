# Nanuq v2.0.1 — Launch Day Distribution Guide

Created: 2026-05-08  
Purpose: Complete step-by-step guide for firing the v2.0.1 distribution queue.  
Estimated time: 90 minutes total (spread over 3 days)

---

## When to Fire

Best days: Tuesday or Wednesday morning (10-11am ET). Dev.to and IH get higher traffic mid-week.
Do NOT wait for v2.1 — the v2.0.1 distribution is independent.

---

## Day 1 (60 min) — The main push

### Step 1 — Publish Dev.to post (10 min)

⚠️ **Pre-flight: open private GHSA draft first (~10 min, do before posting)**
The post says "I've filed a GHSA advisory for this." Open the private draft in GitHub before publishing so the statement is accurate. Steps:
1. Go to github.com/waelouf/Nanuq → Security → Advisories → New draft security advisory
2. Paste content from `_default/nanuq-ghsa-v1-key-derivation-draft.md`
3. Request CVE ID — it stays private until v2.1 GA
Once the draft is open in GitHub (not published — just opened as private draft), proceed:

1. Open `marketing/devto-v201-launch-post.md`
2. Go to dev.to → New Post → paste the Body section content
3. Set title: `Nanuq v2.0.1 — unified UI for Kafka, Redis, RabbitMQ, SQS, and Azure Service Bus`
4. Set tags: `opensource, devtools, kafka, dotnet`
5. Set `published: true`
6. Publish — copy the Dev.to URL for use in social posts

### Step 2 — Post Indie Hackers (10 min)

1. Open `marketing/indie-hackers-post.md`
2. Go to indiehackers.com → New Post → paste content
3. Link the Dev.to post in comments once it has a few upvotes

### Step 3 — Post Twitter/X social sequence (10 min)

Open `marketing/v201-social-posts.md`:

- **Today (Day 1):** Post 1 — "tired of switching between 5 browser tabs"
- **Tomorrow (Day 2):** Post 2 — "Nanuq v2.0.1 covers..."
- **Day 3:** Post 3 — test coverage sprint story (5% → 81.57%)

Also post the LinkedIn post and Bluesky post on Day 1.

### Step 4 — Submit newsletter pitches (15 min)

Open `marketing/v201-newsletter-pitches.md`:

1. **Console.dev tool directory** (highest priority, free listing):
   - Go to console.dev/tools → submit the form
   - Content is pre-written in the file

2. **TLDR DevOps link submit** (~5 min):
   - Go to tldr.tech/submit
   - Submit the Dev.to URL with the TLDR pitch text from the file

---

## Day 3-7 (30 min) — Awesome list PRs

Open `marketing/awesome-list-submissions.md` — 4 PRs, submit in this order:

| PR | Target | Notes |
|----|--------|-------|
| 1 | awesome-selfhosted | Highest impact (~200K stars) — do this one first |
| 2 | awesome-kafka | Targeted audience |
| 3 | awesome-dotnet | .NET ecosystem |
| 4 | awesome-redis | Redis community |

Each PR takes ~10 min. The file has the exact entry text and PR body pre-written.
Best timing: same week as Show HN (for v2.1), but can go any time.

---

## What NOT to fire yet (gated on v2.1 + GHSA)

These are in separate files and require the v2.1.0 release + GHSA publication:
- `.NET Weekly` and `InfoQ` newsletter pitches (`newsletter-pitches.md`)
- `Show HN` post (`show-hn-draft.md`)
- Reddit posts (`v2.1-reddit-posts.md`)
- v2.1 social posts (`v2.1-social-posts.md`)
- v2.1 Dev.to post (`v2.1-announcement-devto.md`)

---

## Quick reference

| Item | File | Status |
|------|------|--------|
| Dev.to post | `devto-v201-launch-post.md` | Ready (published: false) |
| IH post | `indie-hackers-post.md` | Ready |
| Social (Twitter x3, LinkedIn, Bluesky) | `v201-social-posts.md` | Ready |
| Newsletter (console.dev + TLDR) | `v201-newsletter-pitches.md` | Ready (~15 min) |
| Awesome list PRs (x4) | `awesome-list-submissions.md` | Ready (~45 min) |
