# Nanuq — Awesome List Submissions

Created: 2026-05-08  
Status: Ready to submit after Show HN post (or anytime for tool directories)  
Context: PR submissions to curated awesome lists. Wael submits as the project author
from personal GitHub account. These are PRs, not form submissions.

---

## Target Lists (Priority Order)

| List | Stars | Category fit | When to submit |
|------|-------|-------------|----------------|
| awesome-selfhosted | ~200K | Software Development / Monitoring | After Show HN (v2.1 preferred for cross-platform) |
| awesome-kafka | ~6K | Management UIs | Any time |
| awesome-dotnet | ~20K | Tools | After Show HN |
| awesome-redis | ~15K | GUI Clients | Any time |

---

## 1. awesome-selfhosted

**Repo:** github.com/awesome-selfhosted/awesome-selfhosted  
**PR title:** Add Nanuq — unified management UI for Kafka, Redis, RabbitMQ, AWS SQS, Azure Service Bus  
**Target section:** `Software Development`

**Entry to add (find alphabetical position under S in Software Development):**

```markdown
- [Nanuq](https://github.com/waelouf/Nanuq) - Unified web UI for managing Kafka topics,
  Redis keys (all 6 data types), RabbitMQ exchanges/queues, AWS SQS/SNS, and Azure Service
  Bus from a single Docker Compose interface. AES-256 encrypted credential storage.
  `MIT` `Docker` `.NET`
```

**PR body:**
```
## Description

Nanuq is a self-hosted unified management UI that replaces separate browser tabs for
Kafka UI, RedisInsight, RabbitMQ management console, AWS console, and Azure portal.

Single `docker compose up -d` to run. Manages:
- Kafka: topic browser, consumer group offsets, message publish
- Redis: all 6 data types with inline editing
- RabbitMQ: exchanges, queues, bindings, message send/receive
- AWS SQS/SNS: 15 regions
- Azure Service Bus: 30+ regions

AES-256 encrypted credential storage. 478 automated tests (0 failures). MIT licensed.

- GitHub: https://github.com/waelouf/Nanuq
- License: MIT
- Demo: N/A (self-hosted; Docker Compose in README)
- Language: .NET 10 (backend), Vue.js 3 (frontend)

## Checklist

- [x] I am the author/maintainer of this project
- [x] The project is self-hostable (Docker Compose)
- [x] The project is open source (MIT)
- [x] The project has documentation (README)
- [x] I have read the contributing guidelines
```

**Note:** Maintainers may suggest moving to a different section (e.g., "Monitoring" or
creating a new "Message Queue Management" subsection). Accept their guidance.

---

## 2. awesome-kafka

**Repo:** github.com/monksy/awesome-kafka (main community list)  
**PR title:** Add Nanuq — open source multi-broker management UI including Kafka  
**Target section:** `Tools` or `Management UIs`

**Entry to add:**

```markdown
- [Nanuq](https://github.com/waelouf/Nanuq) - Unified web UI for Kafka (topic browser,
  consumer group offsets, message publish) alongside Redis, RabbitMQ, AWS SQS, and Azure
  Service Bus. Single Docker Compose. MIT licensed. .NET 10 + Vue.js 3.
```

**PR body:**
```
Adding Nanuq to the management UI tools section.

Nanuq manages Kafka topic browsing, consumer group offset monitoring, and message
publishing in a unified web UI alongside Redis, RabbitMQ, AWS SQS/SNS, and Azure
Service Bus. Alternative to running separate Kafka UI + broker-specific tools.

GitHub: https://github.com/waelouf/Nanuq
License: MIT
```

---

## 3. awesome-dotnet

**Repo:** github.com/quozd/awesome-dotnet  
**PR title:** Add Nanuq — open source .NET 10 unified messaging infrastructure UI  
**Target section:** `Messaging` or `Database` → GUI

**Entry to add:**

```markdown
- [Nanuq](https://github.com/waelouf/Nanuq) - Open source .NET 10 + Vue.js 3 web UI for 
  managing Kafka, Redis, RabbitMQ, AWS SQS/SNS, and Azure Service Bus from a single 
  Docker Compose interface. AES-256 encrypted credentials. 478 tests.
```

**PR body:**
```
Adding Nanuq under Messaging (or Database tools — please redirect if there's a better section).

Nanuq is a .NET 10 Minimal APIs + Vue.js 3 project with:
- FastEndpoints routing
- Separate class libraries per platform (Nanuq.Kafka, Nanuq.Redis, etc.)
- Entity Framework Core + SQLite
- xUnit + FluentAssertions backend testing
- Vitest frontend testing (478 tests total, 81.57% coverage)

GitHub: https://github.com/waelouf/Nanuq
MIT licensed.
```

---

## 4. awesome-redis

**Repo:** github.com/JamzyWang/awesome-redis or github.com/senadev42/awesome-redis  
**PR title:** Add Nanuq — unified multi-broker UI with full Redis data type support  
**Target section:** `GUI` or `Client Libraries` → GUI Tools

**Entry to add:**

```markdown
- [Nanuq](https://github.com/waelouf/Nanuq) - Web UI for Redis management (Strings, 
  Hashes, Lists, Sets, Sorted Sets, Streams — all 6 data types with inline editing) 
  alongside Kafka, RabbitMQ, AWS SQS, and Azure Service Bus. Docker Compose. MIT.
```

---

## Timing and Submission Notes

**Best time to submit PRs:** Same week as Show HN post — the awesome list submissions
will get approved faster when the project has visible traction (GitHub stars, HN comments).

**Process:**
1. Fork each repo on GitHub
2. Add the entry in the correct alphabetical position within the target section
3. Open a PR with the text above
4. Monitor for maintainer feedback (usually 1-4 weeks)
5. If asked to revise the description, keep it factual and concise

**Expected timeline from submission to merge:**
- awesome-selfhosted: 2-8 weeks (high volume of PRs; needs bot checks to pass)
- awesome-kafka: 1-3 weeks (smaller list, faster review)
- awesome-dotnet: 2-4 weeks
- awesome-redis: 1-4 weeks (depends on which fork is active)

**Total time commitment:** ~45 minutes to open all 4 PRs (copy-paste from this file).
