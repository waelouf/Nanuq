# Nanuq — Product Hunt Listing

Created: 2026-05-07
Status: Submit as "upcoming" before v2.1 launch; post on launch day
Timing: Launch day = day v2.1.0 Docker images are published + GHSA advisory is public

---

## Listing Details

**Product Name:** Nanuq

**Tagline** (60 chars max):
```
One UI for Kafka, Redis, RabbitMQ, AWS, and Azure
```

**Description** (260 chars):
```
Open-source management UI for 5 messaging platforms. Kafka, Redis, RabbitMQ, AWS SQS/SNS, Azure Service Bus — from one Docker Compose command. v2.1 adds Linux/macOS self-hosting with proper encrypted credential storage.
```

**Topics:** Developer Tools, Open Source, DevOps, Infrastructure

**Platforms:** Web

**Pricing:** Free / Open Source (MIT)

---

## Gallery (5 images, 1270×952px each)

**Image 1 — Dashboard overview (lead)**
Screenshot of the main dashboard showing all 5 platform cards: Kafka (topic count, consumer group count), Redis (keys, memory), RabbitMQ (queues, messages), AWS (SQS queues + SNS topics), Azure Service Bus (queues + topics). Clean dark/light UI. Caption: "All your messaging platforms, one screen."

**Image 2 — Kafka topic browser**
Topic list with message counts, partition details, and consumer group offsets visible. Shows a message being browsed in the right panel. Caption: "Browse Kafka topics without the terminal."

**Image 3 — Redis data types**
Redis key browser showing all 6 data types (String, Hash, List, Set, Sorted Set, Stream) with inline editing. Caption: "All 6 Redis data types. In-browser editing."

**Image 4 — Credential management**
The credential management screen showing multiple broker connections, each with connection test status. Caption: "Secure credential storage. AES-256 encrypted."

**Image 5 — Docker Compose setup**
Terminal showing `docker compose up -d` followed by the Nanuq dashboard loading in a browser. Caption: "One command to run. Zero configuration."

---

## Maker Comment (First Comment)

```
Hi Product Hunt 👋

I built Nanuq because I was tired of switching between 5 browser tabs every time 
I needed to debug a messaging stack issue. Kafka UI here, RedisInsight there, 
RabbitMQ management console somewhere else.

The commercial unified tools cost hundreds per month. So I built the open-source 
version.

v2.1 is the release I'm most proud of: it fixes a real security issue (credentials 
weren't properly encrypted on Linux/macOS Docker hosts in v1), and makes Nanuq 
genuinely self-hostable on any OS. If you're running a full backend stack, this 
replaces 5 separate tools.

Happy to answer questions about any of the integrations or the architecture.

GitHub: github.com/waelouf/Nanuq
Docs: [docs link]
```

---

## Pre-Launch Checklist

- [ ] Create Product Hunt account for @waelouf or @mbsoftsystems
- [ ] Submit as "upcoming" 3-5 days before v2.1 launch
- [ ] Line up 10+ hunters to upvote on launch day (use the developer community)
- [ ] Post at 12:01am PT on launch day (Product Hunt resets at midnight PT)
- [ ] Have maker comment ready to post within 5 minutes of listing going live
- [ ] Monitor comments and respond within 1 hour throughout launch day
- [ ] Share PH link in Show HN thread and relevant Slack/Discord communities on launch day
