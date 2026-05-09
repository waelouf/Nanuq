# Nanuq v2.1 — Show HN Draft

Created: 2026-05-07
Status: Post when v2.1.0 is released. Fire after CEO posts GitHub Discussion (MBS-168).

---

## Title

```
Show HN: Nanuq v2.1 – cross-platform Kafka/Redis/RabbitMQ/AWS/Azure UI with proper Linux/macOS encryption
```

---

## Body

```
I shipped Nanuq v2.0.1 in January — an open-source web UI that manages 5 messaging and caching platforms from a single interface (Kafka, Redis, RabbitMQ, AWS SQS/SNS, Azure Service Bus). It runs from Docker Compose and encrypts credentials with AES-256.

But it had a real problem: the key derivation used Windows DPAPI. On Linux and macOS Docker hosts, the encryption silently failed. Credentials were stored unprotected. I've published a GHSA advisory for this (Medium, CWE-311/CWE-327).

v2.1 fixes it properly with an IKeyProtector abstraction:

- **DpapiKeyProtector** — Windows (existing behavior, unchanged)
- **KeychainKeyProtector** — macOS (Keychain Services)
- **LibSecretKeyProtector** — Linux (libsecret, GNOME Keyring/KDE Wallet)
- **EnvelopeKeyProtector** — Docker/K8s: reads master key from NANUQ_MASTER_KEY env var

The design rule: the app refuses to boot without a valid key protector configured. No silent insecure fallback.

For Docker users on Linux: inject one env var, get proper credential encryption.

v2.1 also ships:
- Migration command (re-encrypts existing credential store under new protector)
- Multi-arch Docker images (linux/amd64 + linux/arm64)
- Self-hosting guides for Linux, macOS, and Kubernetes
- Validated K8s manifests (the K8s/ folder was always there but untested)

478 tests, all passing. MIT licensed. Stack: .NET 10 + Vue.js 3.

GitHub: github.com/waelouf/Nanuq
GHSA advisory: [link when published]

Happy to answer questions about the IKeyProtector design, the DPAPI escape hatch, or the migration path from v2.0.x.
```

---

## Comment Talking Points

**On "why not just use a secret manager (Vault, AWS Secrets Manager)?"**
> Nanuq is a local management tool — it runs next to the services it manages, not in a separate secrets infrastructure. For Docker Compose deployments, the EnvelopeKeyProtector (env var injection) is the right scope: it integrates with Docker secrets or whatever secret injection mechanism you already use, rather than requiring a separate secrets service just to run a local UI.

**On "what happens to existing v2.0.x installs?"**
> Windows installs upgrading to v2.1 run `nanuq migrate-credentials` — it re-encrypts the credential store using the new DPAPI protector (same underlying key material, new wrapper). Zero data loss. Linux/macOS installs that were on v2.0.x had unprotected credentials — migration to v2.1 with EnvelopeKeyProtector will encrypt them properly for the first time.

**On "why DPAPI in the first place?"**
> The v1 implementation was Windows-first. DPAPI is the natural key store on Windows — it's what Chrome and many other Windows apps use for credential storage. The mistake was not gating the implementation to Windows explicitly, which would have forced the cross-platform problem to surface earlier.

**On "why not store the key in the database?"**
> That's circular — the key protects the database. A key-in-database design means the "encrypted" credentials are only as secure as the database file, which is no real protection. The protectors all store keys outside the Nanuq data directory: in OS key stores (DPAPI/Keychain/libsecret) or injected at runtime (EnvelopeKeyProtector).

**On "how does this compare to Kafka UI, RedisInsight, etc.?"**
> Those are excellent single-platform tools. Nanuq is the only open-source option that manages all 5 platforms from a single UI with a single credential store. If you're running a full stack (Kafka + Redis + RabbitMQ + AWS + Azure), switching tabs is the problem Nanuq solves.

---

## Timing

Post Tuesday or Wednesday, 9-11am ET.
Fire after:
1. v2.1.0 tagged and Docker Hub images published
2. GHSA advisory published (do not post Show HN before advisory is public)
3. GitHub Discussion posted by CEO (MBS-168)
4. Self-hosting docs merged to main
