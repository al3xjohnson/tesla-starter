# Claude Rules for TeslaStarter Project

## Architecture Decision Records (ADRs)

When making important architecture decisions in this codebase, you should:

1. **Create a new ADR** in `docs/adr/` whenever you:
   - Choose a framework or library
   - Design a major component or system
   - Define an API structure
   - Make security-related decisions
   - Choose between different technical approaches
   - Establish coding patterns or conventions

2. **ADR Format**:
   - Use sequential numbering (001, 002, etc.)
   - Follow the template established in `docs/adr/001-record-architecture-decisions.md`
   - Include: Title, Date, Status, Context, Decision, and Consequences

3. **When to create an ADR**:
   - If the decision will affect future development
   - If someone might ask "why did we do it this way?"
   - If there were multiple viable options considered
   - If the decision has long-term implications

Remember: ADRs help future developers (including yourself) understand why certain choices were made.

## Code Style and Formatting

**IMPORTANT**: Before completing any task, you MUST:

1. **Run code formatting** using `dotnet format` to ensure all code follows the project's style guidelines
2. **Fix any warnings** related to:
   - Collection initialization simplifications
   - Using explicit type names instead of 'var'
   - Using target-typed `new()` when the variable type is explicit
   - Ordering using statements alphabetically
   - Removing unused directives

3. **Code style preferences**:
   - Prefer explicit type names over 'var' (e.g., `string name = "test"` not `var name = "test"`)
   - Use target-typed new when type is explicit (e.g., `Entity entity = new(); List<string> items = [];`)
   - Keep using statements ordered alphabetically
   - Remove any unused using directives
   - Remove any un-needed whitespace
   - Simplify collection initializations where possible

4. **Modern C# patterns**:
   - **Use expression-bodied members** for methods/properties that can be expressed as a single expression
   - **Formatting**: Place `=>` on the same line as the method signature
   - **Multi-line expressions**: Split complex expressions across lines with proper indentation
   - **Use null-coalescing with throw** for guard clauses (e.g., `GetValue() ?? throw new Exception()`)
   - **Avoid unnecessary intermediate variables** before return statements
   - **Use pattern matching** for type/null checks (e.g., `obj is Entity entity && entity.IsValid`)
   - **Simplify boolean expressions** by combining conditions with logical operators
   
   Examples:
   ```csharp
   // ✅ Good - Simple expression
   public override int GetHashCode() => Id.GetHashCode();
   
   // ✅ Good - Multi-line expression
   public override bool Equals(object? obj) =>
       obj is Entity<TId> entity && Equals(entity);
   
   // ✅ Good - Complex multi-line expression
   public override bool Equals(object? obj) =>
       obj is Enumeration otherValue &&
       GetType() == obj.GetType() &&
       Id.Equals(otherValue.Id);
   
   // ✅ Good - Null-coalescing with throw
   public static T Parse<T>(string value) => 
       GetValue(value) ?? throw new InvalidOperationException($"Invalid: {value}");
   
   // ❌ Avoid - Multi-line method body
   public override int GetHashCode()
   {
       return Id.GetHashCode();
   }
   ```

## Best Practices to Follow

Keep commits small and focused (typically < 100 lines changed)
Write self-documenting code with clear variable/method names
Include XML documentation comments for public APIs
Follow SOLID principles and DDD patterns
Write unit tests for new functionality (ask if unsure about test scenarios)
Handle edge cases and errors gracefully
Use meaningful exception types and messages

Response Format
Always maintain clear communication:

Use headers to separate different phases
Use code blocks for file changes
Use bullet points for lists
Highlight important decisions or blockers
Explain technical concepts simply

Remember: The goal is to write production-quality code while helping someone learn. Be thorough in explanations but efficient in implementation.

# important-instruction-reminders
Do what has been asked; nothing more, nothing less.
NEVER create files unless they're absolutely necessary for achieving your goal.
ALWAYS prefer editing an existing file to creating a new one.
NEVER proactively create documentation files (*.md) or README files. Only create documentation files if explicitly requested by the User.