# 30-Day Optimization Plan (CSharp /.NET SDK)

## Outcome Target

- Deliver a high-confidence .NET SDK with strong runtime safety, clear observability, and enterprise onboarding quality.
- Keep first API success under 10 minutes and first SSE integration under 30 minutes.

## P0 (Day 1-14): Reliability and Contract Hardening

| Workstream | Task | Files | Acceptance |
| --- | --- | --- | --- |
| API Contract Alignment | Align endpoint paths/methods with latest backend contract and docs | `src/EGroupAI.AiSandbox.Sdk/AiSandboxClient.cs`, `openapi/ai-sandbox-v1.yaml`, `docs/INTEGRATION.md` | 11 API operations validated with no mismatch |
| Safe Retry Policy | Restrict default retries to idempotent methods and add explicit write retry policy | `AiSandboxClient.cs`, `README.md` | No duplicate write operations in retry simulations |
| Resource Safety | Ensure response disposal on retry and JSON paths to avoid resource pressure | `AiSandboxClient.cs` | Long-run load test shows stable resource usage |
| QA Baseline | Add unit tests for serialization, retries, and SSE parsing | `src/EGroupAI.AiSandbox.Sdk/*.cs`, `tests/*` (new), `*.csproj` | CI tests pass with critical-path coverage target |
| CI/CD Guardrails | Add workflow for restore/build/test/package validation | `.github/workflows/ci.yml` (new), `*.csproj` | Required checks block failing PRs |

## P1 (Day 15-30): Developer Experience and Growth

| Workstream | Task | Files | Acceptance |
| --- | --- | --- | --- |
| Example Expansion | Upgrade quickstart to full flow with streaming and KB calls | `examples/Quickstart.cs`, `README.md` | Example runs with env vars only |
| Visual Docs Upgrade | Add troubleshooting matrix and common exception recipes | `README.md`, `docs/INTEGRATION.md` | Partner onboarding issues reduced |
| Release Quality | Add structured release checklist and compatibility notes | `CHANGELOG.md`, `CONTRIBUTING.md` | Every release includes impact notes |
| Security Posture | Enable dependency and secret scanning in CI | `.github/workflows/ci.yml`, `SECURITY.md` | No unresolved high-severity issue at release gate |

## Language File Checklist

- `README.md`
- `docs/INTEGRATION.md`
- `docs/30D_OPTIMIZATION_PLAN.md`
- `src/EGroupAI.AiSandbox.Sdk/AiSandboxClient.cs`
- `src/EGroupAI.AiSandbox.Sdk/ApiException.cs`
- `src/EGroupAI.AiSandbox.Sdk/EGroupAI.AiSandbox.Sdk.csproj`
- `examples/Quickstart.cs`
- `openapi/ai-sandbox-v1.yaml`
- `CHANGELOG.md`
- `CONTRIBUTING.md`
- `SECURITY.md`

## Definition of Done (DoD)

- [ ] 11/11 API operations pass production integration validation.
- [ ] SSE async stream returns chunks and terminates on `[DONE]`.
- [ ] Retry defaults avoid duplicate non-idempotent writes.
- [ ] CI pipeline enforces restore + build + test + quality checks.
- [ ] Quickstart runs in a clean environment with required env vars only.
