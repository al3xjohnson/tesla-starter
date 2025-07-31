# ADR-003: Nx Monorepo Tooling with pnpm

## Date
2025-07-22

## Status
Proposed

## Context
The TeslaStarter project uses a monorepo structure containing both .NET backend services and a React/TypeScript frontend. We need tooling to efficiently manage builds, dependencies, and task orchestration across these different technology stacks.

## Decision
We will use Nx as our monorepo management tool with pnpm as the package manager for the following components:
- **Nx**: Monorepo orchestration and build system
- **pnpm**: Fast, disk-space efficient package manager with built-in workspace support
- **Hybrid approach**: Nx manages JavaScript/TypeScript projects while .NET projects use MSBuild/dotnet CLI

## Consequences

### Benefits

1. **Unified Task Orchestration**
   - Single command to build/test all projects
   - Intelligent build caching and dependency graph analysis
   - Parallel task execution with proper ordering
   - Affected project detection for CI optimization

2. **pnpm Advantages**
   - Efficient disk space usage through content-addressable storage
   - Strict dependency resolution prevents phantom dependencies
   - Built-in workspace protocol for local package linking
   - Faster installations compared to npm/yarn

3. **Developer Experience**
   - Consistent commands across all projects
   - Shared configurations and tooling
   - Powerful code generation capabilities
   - VSCode/IDE integration with Nx Console

4. **CI/CD Optimization**
   - Build only affected projects on PR
   - Distributed task execution capabilities
   - Remote caching with Nx Cloud (optional)
   - Dependency graph visualization

5. **Incremental Adoption**
   - Can coexist with existing .NET tooling
   - Gradual migration path for additional features
   - No disruption to current workflows

### Implementation Strategy

1. **Nx Configuration**
   ```json
   {
     "npmScope": "teslastarter",
     "affected": {
       "defaultBase": "main"
     },
     "workspaceLayout": {
       "appsDir": "src/apps",
       "libsDir": "src/shared"
     }
   }
   ```

2. **pnpm Workspace Configuration**
   ```yaml
   packages:
     - 'src/apps/*'
     - 'src/shared/typescript/*'
     - 'tests/*'
   ```

3. **Project Structure**
   ```
   tesla-starter/
   ├── nx.json                 # Nx configuration
   ├── pnpm-workspace.yaml     # pnpm workspace config
   ├── package.json            # Root package.json
   ├── .npmrc                  # pnpm configuration
   └── src/
       ├── apps/
       │   └── web/           # React app (Nx project)
       └── shared/
           └── typescript/     # Shared TS libraries (Nx projects)
   ```

4. **Task Examples**
   - `nx build web` - Build React app
   - `nx test web` - Test React app
   - `nx affected:build` - Build only affected projects
   - `nx graph` - Visualize project dependencies
   - `pnpm install` - Install all dependencies
   - `dotnet build && nx build web` - Build everything

### Trade-offs

1. **Learning Curve**
   - Team needs to learn Nx concepts
   - Additional configuration files
   - Different mental model from pure .NET solutions

2. **Tooling Overhead**
   - Additional Node.js dependency for .NET developers
   - More complex initial setup
   - Potential version conflicts between tools

3. **Hybrid Complexity**
   - Two build systems (MSBuild + Nx)
   - Need to maintain compatibility
   - Custom scripts for cross-stack operations

## Alternatives Considered

1. **Lerna + pnpm**
   - Less powerful than Nx
   - No built-in caching or affected detection
   - Limited to JavaScript projects only

2. **Rush**
   - More enterprise-focused
   - Steeper learning curve
   - Less community adoption

3. **Turborepo**
   - Simpler than Nx
   - Less mature ecosystem
   - Limited extensibility

4. **Manual scripts**
   - No intelligent caching
   - No dependency graph analysis
   - Harder to maintain at scale

## References
- [Nx Documentation](https://nx.dev)
- [pnpm Documentation](https://pnpm.io)
- [Nx with .NET](https://nx.dev/recipes/other/nx-and-dotnet)
- [pnpm Workspaces](https://pnpm.io/workspaces)