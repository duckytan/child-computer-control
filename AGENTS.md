# AGENTS.md

## 自动推送规则

**每次完成文件修改后，必须将更改提交并推送到 GitHub。**

## Repo Structure

- `.monkeycode/specs/child-pc-guard/tasklist.md` — Implementation plan with 3 phases
- `project/child-computer-control/` — Project planning documents
  - `ChildPCGuard_完整项目方案_v2.md` — Complete project plan with source code
  - `_archive/` — Archived superseded documents
- `docs/plans/` — Confirmed design specifications

## Current Phase

**Planning phase** — preparing to transition to implementation. No source code exists yet.

## Implementation Plan

The project plan is stored in `.monkeycode/specs/child-pc-guard/tasklist.md`:
- Phase 1: Engineering documentation (requirements, design, test, acceptance docs)
- Phase 2: Code development (4 components: Shared, GuardService, Agent, LockOverlay)
- Phase 3: Testing and acceptance

## Key Facts

- **Platform**: Windows only (.NET 8, Windows Service, WPF, Win32 API)
- **Architecture**: Windows Service (SYSTEM) + dual Agent processes + LockOverlay UI
- **Data paths**: `C:\ProgramData\ChildPCGuard\` (config, logs, data)
- **Process disguise**: AgentA → svchost.exe, AgentB → RuntimeBroker.exe
