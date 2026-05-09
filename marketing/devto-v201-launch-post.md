---
title: "I built an open-source unified UI for Kafka, Redis, RabbitMQ, AWS SQS, and Azure Service Bus"
published: false
description: "Why I built Nanuq, what it does, and how 478 tests in one sprint changed how I think about open source maintainability."
tags: opensource, dotnet, devops, kafka
cover_image: https://raw.githubusercontent.com/waelouf/Nanuq/main/images/nanuq-logo.jpg
canonical_url: https://dev.to/waelouf/nanuq-unified-messaging-ui
---

⚠️ **Pre-publish gate:** Line 85 of this post says "I've filed a GHSA advisory for this." Before publishing, open the private GHSA draft in GitHub first (instructions in `_default/nanuq-ghsa-v1-key-derivation-draft.md`, checklist at bottom of that file). The private draft can be opened now — it stays private until v2.1 GA. Opening takes ~10 minutes and requests a CVE ID. Do NOT publish this post with that sentence if the GHSA hasn't been opened in GitHub.

Every time I debugged a messaging stack with Kafka, Redis, and RabbitMQ in it, I spent the first 20 minutes cycling through browser tabs: Kafka UI, RedisInsight, RabbitMQ management console, back to Kafka UI. Add AWS SQS and Azure Service Bus and it's five separate tools with five credential sets.

The commercial products that unify this (CloudAMQP, Conduktor, RedisInsight Pro) are priced for teams — hundreds to thousands per month. The open source alternatives each covered one platform well and required separate Docker instances. I wanted one interface running on my machine.

So I built [Nanuq](https://github.com/waelouf/Nanuq).

## What Nanuq does

Single Docker Compose command. Manages 5 platforms from one UI:

- **Kafka** — topic browser, consumer group offset monitoring, message publish
- **Redis** — all 6 data types (Strings, Hashes, Lists, Sets, Sorted Sets, Streams) with inline editing
- **RabbitMQ** — exchange/queue browser, bindings viewer, message send/receive
- **AWS SQS/SNS** — queue and topic management, 15 regions, cross-region support
- **Azure Service Bus** — queues, topics, subscriptions, 30+ regions

Credentials are encrypted with AES-256 before being stored. Every broker connection's credentials are encrypted individually with a per-record key.

```bash
git clone https://github.com/waelouf/Nanuq
cd Nanuq
docker compose up -d
# open http://localhost:8080
```

## The test coverage sprint

When I shipped v2.0.1, I ran a dedicated testing sprint. Starting from ~5% frontend coverage.

Two weeks later: 323 frontend tests across 12 files (81.57% coverage), 155 backend tests across 24 files. 478 tests total. Zero failures.

Before that sprint, touching one part of the codebase felt like it might break another. After 81% coverage, I could refactor, add platforms, and ship with confidence. I wrote about this in the IH post — it's the single most important thing I did to make Nanuq maintainable.

The breakdown:

**Frontend (Vue.js 3 + Vitest):**
```
activityLog.spec.js      18 tests  100% coverage
azure.spec.js            34 tests  100% coverage
credentials.spec.js      22 tests  100% coverage
kafka.spec.js            32 tests  100% coverage
aws.spec.js              40 tests   96% coverage
redis.spec.js            72 tests   74% coverage
CredentialForm.spec.js   48 tests   71% coverage
```

**Backend (.NET 10 + xUnit):**
```
AesCredentialService     47 tests   ~90% coverage
SQLite repositories      62 tests  100% CRUD + audit
API endpoints            32 tests   50%+ coverage
```

## The architecture

`.NET 10 Minimal APIs` backend with separate class libraries per platform:

```
Nanuq.WebApi/          # FastEndpoints routing
Nanuq.Kafka/           # Confluent.Kafka client
Nanuq.Redis/           # StackExchange.Redis
Nanuq.RabbitMQ/        # RabbitMQ.Client
Nanuq.AWS/             # AWSSDK.SQS + AWSSDK.SNS
Nanuq.Azure/           # Azure.Messaging.ServiceBus
Nanuq.Security/        # AES-256 credential encryption
```

`Vue.js 3` frontend with Vuex modules per platform — each platform is a self-contained Vuex module with its own actions, mutations, and state.

SQLite database via Entity Framework Core with migrations. Each broker connection record stores its credentials encrypted individually.

## What didn't work

**Redis scanning.** The Redis key browser currently loads all keys at once. On a Redis instance with 100K+ keys, this is a problem. The fix (cursor-based scanning with pagination) is on the v2.1 backlog.

**Windows-only credential encryption.** The AES-256 encryption uses Windows DPAPI for key derivation. On Linux/macOS Docker hosts, this silently fails — credentials are stored unprotected. I've filed a GHSA advisory for this. v2.1 replaces it with a proper cross-platform `IKeyProtector` abstraction (DPAPI on Windows, Keychain on macOS, libsecret on Linux, env var injection for Docker).

**Kubernetes manifests exist but are unvalidated.** The `K8s/` directory has manifests but they haven't been end-to-end tested. v2.1 includes validation as a deliverable.

## What's next (v2.1)

The cross-platform encryption work is the main v2.1 milestone. The IKeyProtector abstraction gives Docker users on Linux a proper solution: inject `NANUQ_MASTER_KEY` as an env var and credential encryption works correctly on any OS. The app refuses to boot without a valid key protector — no silent insecure fallback.

I'll post a separate Dev.to write-up when v2.1 ships.

## Stack summary

| Layer | Technology |
|-------|-----------|
| Backend | .NET 10, Minimal APIs, FastEndpoints |
| Frontend | Vue.js 3, Vuex, Vuetify |
| Database | SQLite + EF Core |
| Testing | xUnit, Moq, FluentAssertions, Vitest |
| CI/CD | GitHub Actions → Docker Hub |
| Auth | JWT (not exposed externally — local tool) |

**GitHub:** https://github.com/waelouf/Nanuq  
**License:** MIT  
**Current version:** 2.0.1 (478 tests, 0 failures)

Happy to answer questions about the .NET + Vue architecture, the platform integrations, or the test coverage approach.
