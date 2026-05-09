# Nanuq — Indie Hackers Post

Platform: indiehackers.com/post  
Status: Ready to publish now (Nanuq v2.0.1 is live)  
Timing: Post any time — or use as pre-launch for v2.1

---

## Title

```
I got tired of switching between 5 browser tabs to manage my messaging stack, so I built a unified UI
```

---

## Body

```
Every time I worked with a stack that had Kafka, Redis, and RabbitMQ, I'd spend the 
first 20 minutes of a debugging session cycling through browser tabs: 
Kafka UI → RedisInsight → RabbitMQ management console → back to Kafka UI.

The commercial tools that unified this cost hundreds to thousands per month. The 
open source alternatives each covered one system well and required separate 
instances. I wanted one interface.

So I built Nanuq.

---

**What it is**

Nanuq is an open-source web UI that manages 5 messaging and caching platforms 
from a single interface:

- Kafka (topic browser, consumer group offsets, message publish)
- Redis (all 6 data types: Strings, Hashes, Lists, Sets, Sorted Sets, Streams)
- RabbitMQ (exchanges, queues, bindings, message operations)
- AWS SQS/SNS (queue/topic management, cross-region support)
- Azure Service Bus (queues, topics, subscriptions)

It runs from a single Docker Compose command. Credentials are encrypted with 
AES-256 before being stored. You manage all your broker connections from one 
screen.

The v2.0.1 release shipped with 478 automated tests (155 backend, 323 frontend) 
with zero failures, full test coverage across all 5 platforms, and CI/CD to 
Docker Hub.

---

**The build**

Stack: .NET 10 Minimal APIs + Vue.js 3. Total build time including tests and CI/CD 
setup: about 4 months of part-time work.

The hardest part wasn't the platform integrations — it was making the credential 
storage genuinely secure without making setup painful. I ended up with a design 
where each broker connection's credentials are encrypted individually with a 
per-record key, wrapped with a master key derived from Windows DPAPI. It works 
well on Windows; the limitation is that DPAPI isn't available cross-platform.

Which brings me to v2.1.

---

**What I'm working on next**

The main complaint I see in issues and DMs: "I run this in Docker on Linux and the 
encryption doesn't work." That's correct — DPAPI is Windows-only.

v2.1 fixes this with an IKeyProtector abstraction:
- DpapiKeyProtector (Windows — unchanged)
- KeychainKeyProtector (macOS, SecKeychain)
- LibSecretKeyProtector (Linux, libsecret)
- EnvelopeKeyProtector (Docker/K8s — reads master key from env var)

For Docker users specifically: inject one environment variable 
(NANUQ_MASTER_KEY), get proper credential encryption on any Linux host. The app 
refuses to boot without a valid key protector configured — no silent insecure 
fallback.

I posted the v2.1 roadmap as a GitHub Discussion if you want to follow along.

---

**Revenue**

Zero. It's MIT open source, no paid tier, no SaaS version. This is a developer 
tool I built because I needed it and the commercial alternatives were out of budget 
for personal projects. I'm not opposed to a managed hosting tier eventually, but 
it's not the current focus.

---

**Lessons**

The thing I underestimated: how much time thorough testing takes when you're 
integrating with 5 different external APIs with different auth models, connection 
protocols, and error patterns. The test suite went from 5% to 81% coverage in a 
single focused sprint — but that sprint was 2 weeks of concentrated work.

The thing that surprised me: people actually use this. I open-sourced it mostly 
expecting it to live in a GitHub repository with 3 stars. It has actual users 
running it in production, which motivates the v2.1 cross-platform work more than 
anything.

---

**GitHub:** github.com/waelouf/Nanuq  
**Stack:** .NET 10 + Vue.js 3 + Docker  
**License:** MIT  
**Current version:** v2.0.1 (478 tests, all passing)

Happy to answer questions about the .NET + Vue architecture, the messaging 
platform integrations, or the IKeyProtector design for v2.1.
```

---

## Timing Note

This post can go up any time — v2.0.1 is live and the GitHub Discussion is posted.  
Best timing: same week as the v2.1 GitHub Discussion goes live (more to talk about).  
Alternatively: v2.1 launch day for maximum momentum.
