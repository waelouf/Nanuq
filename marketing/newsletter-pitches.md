# Nanuq — Developer Newsletter Pitches

Created: 2026-05-07  
Status: Ready to send when v2.1.0 ships  
Context: These are direct email pitches to newsletter editors. Personal, brief, not press releases.

---

## Target Publications

| Newsletter | Subscribers | Angle | Priority |
|-----------|------------|-------|----------|
| .NET Weekly (dotnetweekly.com) | ~15K | Open source .NET tool for messaging infra | High |
| InfoQ .NET | 500K+ | Technical depth: IKeyProtector abstraction, cross-platform encryption | High |
| Console.dev (open source tools weekly) | ~10K | Open source unified messaging UI | High |
| TLDR DevOps | ~50K | Cross-platform self-hosting for Docker/K8s | Medium |
| Hacker Newsletter (hnewsletter.com) | ~75K | If HN post gets traction — this is automatic |

---

## Pitch 1: .NET Weekly / dotnetweekly.com

**To:** curators@dotnetweekly.com (or via submission form)  
**Subject:** Open source .NET management UI for Kafka/Redis/RabbitMQ — v2.1 ships cross-platform encryption

```
Hi,

Quick submission for .NET Weekly consideration.

Nanuq (github.com/waelouf/Nanuq) is an open-source unified web UI for managing 
Kafka topics, Redis keys (all 6 data types), RabbitMQ, AWS SQS/SNS, and Azure 
Service Bus — all from a single .NET 10 + Vue.js interface.

The v2.1 release landing soon adds something technically interesting: we're 
replacing Windows DPAPI key derivation with a cross-platform IKeyProtector 
abstraction. Four implementations — DpapiKeyProtector (Windows), 
KeychainKeyProtector (macOS), LibSecretKeyProtector (Linux), and 
EnvelopeKeyProtector (env var / file, works in Docker) — resolved via DI at 
startup. The app refuses to boot without a valid protector; no silent insecure 
fallback.

The v2.1 Dev.to writeup: [link once published]
GitHub: github.com/waelouf/Nanuq (478 tests, all passing, MIT)

Thanks for the newsletter — it's a useful weekly read.
[Wael]
```

---

## Pitch 2: InfoQ .NET

**To:** editors@infoq.com (news tip)  
**Subject:** .NET open source: cross-platform credential encryption via IKeyProtector abstraction

```
News tip for InfoQ .NET.

The open-source Nanuq messaging management platform (github.com/waelouf/Nanuq) 
is releasing v2.1 with a cross-platform key protection design that developers 
running .NET apps on Linux/macOS may find useful.

The problem: ProtectedData.Protect() (Windows DPAPI) is the common .NET default 
for key derivation but silently unavailable on non-Windows. Rather than platform-
checking at runtime, v2.1 defines IKeyProtector with DI-registered implementations 
per platform (DPAPI, macOS SecKeychain, Linux libsecret, and an envelope approach 
for Docker via env var). The startup validator rejects the app if no valid 
implementation is injected — eliminating the silent-degradation failure mode.

Related GHSA for the v1 weakness: [link once published]
Technical Dev.to post: [link]
GitHub: github.com/waelouf/Nanuq

Happy to answer technical questions or provide additional context for an article.
[Wael Mansour, MBSoft Systems]
```

---

## Pitch 3: console.dev (Open Source Tools)

**To:** hello@console.dev (or via their submission form at console.dev/tools)  
**Subject:** Tool submission: Nanuq — unified Kafka/Redis/RabbitMQ/AWS/Azure management UI

```
Tool submission for Console.

Nanuq (github.com/waelouf/Nanuq) is an open-source, 
self-hostable web UI for managing multiple messaging and caching systems from 
one interface:

- Kafka: topic browser, consumer group offsets, message publish
- Redis: all 6 data types (Strings, Hashes, Lists, Sets, Sorted Sets, Streams)
- RabbitMQ: exchanges, queues, bindings, message publish
- AWS SQS/SNS: queue/topic management, message ops
- Azure Service Bus: queues, topics, subscriptions

Single Docker Compose command to run. AES-256 encrypted credential storage. 
MIT licensed, .NET 10 + Vue.js, 478 automated tests.

v2.1 (in development) adds proper Linux/macOS self-hosting via cross-platform 
key protection. Currently Windows DPAPI only.

GitHub: github.com/waelouf/Nanuq
Docker Hub: [docker hub link if published]
```

---

## Pitch 4: TLDR DevOps

**To:** via tldr.tech/submit (link submission, not article pitch)  
**Subject:** N/A — submit the GitHub Discussion URL directly

Submit the v2.1 GitHub Discussion link via TLDR's community submission form.  
Frame: "Open source messaging management UI (Kafka/Redis/RabbitMQ/AWS/Azure) 
removes Windows DPAPI dependency, enables Docker-based Linux self-hosting"

---

## Timing

Send all pitches on the same day Nanuq v2.1.0 ships — not before. The pitches 
reference the GitHub Advisory and Dev.to post, which aren't published yet.

Submission lead times:
- .NET Weekly: 1–2 week turnaround (weekly newsletter)
- InfoQ: may take 2–4 weeks for editorial review; often covers tools without articles
- Console.dev: reviews tools, may take 1–3 weeks
- TLDR DevOps: 1–7 day turnaround if link gets upvoted in their community

**Send in order:** InfoQ first (longest lead time), then .NET Weekly, then Console.dev, 
then TLDR on the same day.
