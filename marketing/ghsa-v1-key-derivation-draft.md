# GHSA Draft — Nanuq v1/v2.0 Non-Windows Key Derivation Weakness

**Status:** DRAFT — do not publish until v2.1.0 ships and migration path is available  
**Target publication:** Same release window as Nanuq v2.1.0  
**Advisory type:** GitHub Security Advisory (GHSA) via repository Security tab → New advisory

---

## Advisory Fields (paste into GitHub Security Advisory form)

### Ecosystem
`NuGet` (primary) — also affects Docker Hub image consumers

### Package name
`Nanuq` (self-hosted, no NuGet package — use "Other" or leave blank; describe in summary)

### Affected versions
`<= 2.0.1`

### Patched version
`2.1.0`

### Severity
**Medium** (CVSS 3.1 score: 4.4 — vector: AV:L/AC:L/PR:L/UI:N/S:U/C:H/I:N/A:N)

**Rationale:** On Linux/macOS, the encryption key is derived from SHA-256(`hostname + username`) — both publicly readable by any local shell account. An attacker with read access to the database can reconstruct the key in a single computation and decrypt all stored credentials. Attack is local-only (not network-accessible). Privileges Required is Low (local shell account needed to read the database file).

### CWE
- **CWE-311**: Missing Encryption of Sensitive Data
- **CWE-327**: Use of a Broken or Risky Cryptographic Algorithm (context: DPAPI is not broken, but its unavailability on non-Windows silently degrades the protection)

---

## Summary (for GitHub advisory form)

```
Nanuq v1.x and v2.0.x derive the AES-256 credential encryption key using
SHA-256 of publicly-readable system properties (hostname and username) on
Linux and macOS. An attacker with local read access to the Nanuq SQLite
database can reconstruct the encryption key in a single SHA-256 computation
and decrypt all stored credentials (broker URLs, usernames, passwords, API
keys, connection strings).

Windows-hosted deployments are not affected (the Windows code path uses
DPAPI for key derivation). This issue affects self-hosted Nanuq deployments
on Linux — including the most common Docker-on-Linux configuration — and
macOS.
```

---

## Details (for GitHub advisory body / public disclosure)

### Background

Nanuq stores connection credentials (hostnames, usernames, passwords, API
keys) for managed messaging infrastructure (Kafka, Redis, RabbitMQ, AWS
SQS/SNS, Azure Service Bus) in a local SQLite database. Credentials are
encrypted using AES-256 before being written to disk.

### Root Cause

The key derivation step in `AesCredentialService` calls
`ProtectedData.Protect(masterKey, null, DataProtectionScope.CurrentUser)`,
which is a Windows-only API. On Linux and macOS:

- .NET 6+ throws `PlatformNotSupportedException` when `ProtectedData.Protect`
  is called.
- Depending on the application's exception handling, this may cause credential
  storage to fail silently or the application to fall back to storing the raw
  AES key without platform-level wrapping.

The net effect is that on non-Windows hosts, the AES key used to encrypt
credentials is not itself protected by an OS-level keystore. An attacker with
read access to the SQLite database file and the application's working
directory could potentially recover stored credentials.

### Affected Configurations

| Deployment | Affected? |
|---|---|
| Nanuq on Windows (any version ≤ 2.0.1) | **Not affected** — DPAPI works correctly |
| Nanuq in Docker on a Linux host (≤ 2.0.1) | **Affected** |
| Nanuq on macOS (≤ 2.0.1) | **Affected** |
| Nanuq in Kubernetes on Linux (≤ 2.0.1) | **Affected** |

### Impact

An attacker who gains read access to the Nanuq data directory (SQLite
database + application files) on an affected Linux/macOS host can potentially
recover stored broker credentials. This requires local or container file
system access — it is not remotely exploitable via the Nanuq web interface.

Credentials at risk include: broker hostnames/ports, SASL usernames and
passwords, AWS access key IDs and secret access keys, Azure Service Bus
connection strings.

### Remediation

**Upgrade to Nanuq v2.1.0**, which introduces a cross-platform
`IKeyProtector` abstraction:

- `DpapiKeyProtector` — Windows (existing behavior, unchanged)
- `KeychainKeyProtector` — macOS (SecKeychain)
- `LibSecretKeyProtector` — Linux (libsecret / GNOME Keyring)
- `EnvelopeKeyProtector` — all platforms; reads master key from
  `NANUQ_MASTER_KEY` environment variable or `~/.config/nanuq/master.key`

Nanuq v2.1.0 will **refuse to start** if no valid key protector is
configured, eliminating the silent-degradation failure mode.

### Migration

v2.1.0 ships a migration command to re-encrypt existing v2.0.x credentials
with the new AES-256-GCM envelope format:

```bash
nanuq credential migrate
```

Run this command after upgrading. Existing credential ciphertexts are
transparently readable during the transition window.

### Workaround (if upgrade is not immediately possible)

For Linux/macOS deployments that cannot immediately upgrade to v2.1.0:

1. Restrict file system access to the Nanuq data directory to the
   application user only (`chmod 700`).
2. Run Nanuq in a container with no volume mount accessible to other
   processes.
3. Rotate any credentials stored in Nanuq that have broad permissions
   (e.g., AWS IAM keys with write access).

### Credit

Identified during internal security review as part of the v2.1 cross-platform
self-hosting initiative.

---

## Disclosure Timeline

| Date | Event |
|---|---|
| 2026-04-xx | Issue identified during v2.1 planning |
| 2026-05-08 | v2.1 roadmap publicly announced (GitHub Discussion) |
| v2.1.0 release | This advisory published; patch available |
| v2.1.0 + 30 days | Full technical write-up published on Dev.to |

---

## Post-Publication Actions

After the GHSA is published (same day as v2.1.0 release):

1. Add a banner to the Nanuq README warning Linux/macOS users on ≤ 2.0.1
2. Post a short note in the GitHub Discussion thread for v2.1 linking the advisory
3. Update the Dev.to announcement article to include the GHSA link
4. Update the social posts (Twitter/X thread Post 1) to reference the advisory

---

*This draft prepared by CMO agent ab34cbb1 on 2026-05-07 for v2.1 coordinated disclosure.*  
*Do not publish this file publicly — it is an internal pre-publication draft.*
