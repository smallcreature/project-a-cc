# Repository Guidelines

## Project Layout
- `project-a-unity/` is the Unity project. Open it with Unity `6000.4.6f1`.
- `project-a-unity/Assets/Scenes/SampleScene.unity` is the only scene currently in build settings.
- `project-a-unity/Assets/Settings/` contains URP render pipeline settings and profiles.
- `project-a-unity/Assets/InputSystem_Actions.inputactions` is the project Input System actions asset.
- `project-a-unity/Assets/Scripts/` is reserved for project gameplay/application code and is currently empty.
- `project-a-unity/Assets/AssetStore/` contains imported third-party art and support scripts. Avoid editing vendor assets unless the task explicitly requires it.
- `Project Wiki/` is an Obsidian vault. `raw/` holds immutable source text; `wiki/` holds LLM-generated pages, index, log, entities, concepts, and synthesis.
- `Project Wiki/AGENTS.md` defines the wiki-agent schema and should be read before editing the vault.
- `.agents/skills/obsidian-wiki-agent/` is the local skill for turning raw technical text into a linked Obsidian wiki.

## Project Context
- The main design/product context for the project lives in `Project Wiki/`.
- Before making design, gameplay, content, or implementation decisions that depend on project intent, read `Project Wiki/AGENTS.md` and start from `Project Wiki/wiki/index.md`.
- Treat `Project Wiki/wiki/` as the compiled knowledge base and `Project Wiki/raw/` as source material.

## Development Workflow
- Codex acts as CTO and Project Manager for feature work: owns architecture, decomposition, prioritization, task assignment, decision review, and final code review.
- For any non-trivial feature, first consult the relevant project roles, then provide an implementation plan for user review before starting development.
- If the user marks a request as `сложная фича` or `complex feature`, treat it as an explicit instruction to use subagents and consult them according to the Project Roles rules before proposing or implementing the plan.
- Do not start implementation until the user approves the plan, unless the requested change is small, mechanical, or explicitly asks for immediate coding.
- Plans should include: feature summary, wiki/design context, proposed architecture, team input, task breakdown, file ownership, risks, and verification.
- Use `Project Wiki/` for product intent and `project-a-unity/` for implementation facts.

## Project Roles
- Senior Client Programmer: owns gameplay/client runtime code, Unity C#, input, local state, UI/runtime systems, and client-side implementation proposals.
- Senior Network Programmer: owns networking architecture, authority model, synchronization, replication, prediction/reconciliation, serialization, and review of client code for multiplayer readiness.
- Technical Artist: owns URP/rendering concerns, shaders, materials, VFX, lighting, visual optimization, asset display, and review of visual/rendering decisions.
- CTO/PM mediates disagreements, challenges proposed solutions, decides final architecture, and ensures role feedback is reflected in the plan.

## File Ownership
- Every implementation plan must assign file ownership before code edits begin.
- Only one role may edit a file at a time.
- Two roles must not make concurrent changes to the same file.
- If ownership needs to move between roles, finish or pause the current edit first, then explicitly reassign the file.

## Unity Asset Rules
- Unity is configured for text serialization (`m_SerializationMode: 2`) and visible meta files. Keep `.meta` files with their assets and preserve GUIDs.
- Prefer using the Unity Editor for scene, prefab, material, and import-setting changes. Only hand-edit Unity YAML when the change is small, clear, and easy to review.
- Do not commit generated or machine-local Unity output such as `Library/`, `Temp/`, `Logs/`, `UserSettings/`, generated `.csproj`, or generated `.sln` files.
- If `Logs/` or `UserSettings/` show as modified, treat them as local/editor state unless the user specifically asks to inspect or change them.

## C# Conventions
- Use standard Unity C# style: `PascalCase` for types, methods, and properties; `camelCase` for locals and parameters.
- Prefer `[SerializeField] private` fields over public fields for Inspector wiring.
- Keep one primary `MonoBehaviour` per file, with the file name matching the class name.
- Put new project code under `Assets/Scripts/` or an appropriate new project-owned folder, not under `Assets/AssetStore/`.
- Keep runtime code independent from editor-only APIs; place editor tooling in an `Editor/` folder if added.

## Packages And Rendering
- The project uses URP (`com.unity.render-pipelines.universal` `17.4.0`).
- The project uses the Unity Input System (`com.unity.inputsystem` `1.19.0`).
- Unity Test Framework is available (`com.unity.test-framework` `1.6.0`).

## Verification
- For code-only changes, prefer adding or running Unity EditMode tests when practical.
- Example test command, if Unity is installed in the default Hub location:
  ```powershell
  & "C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe" -batchmode -quit -projectPath "$PWD\project-a-unity" -runTests -testPlatform EditMode -testResults "$PWD\TestResults\EditMode.xml"
  ```
- If the sandbox reports Git dubious ownership, use a one-off safe-directory override:
  ```powershell
  git -c safe.directory=C:/projects/project-a-cc status --short
  ```

## Collaboration Notes
- The working tree may already contain user/editor changes. Do not revert unrelated changes.
- Keep generated project files and large imported assets untouched unless the requested work depends on them.
