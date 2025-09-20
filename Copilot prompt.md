# Documentation Review Guide

This section provides structured instructions for conducting thorough documentation reviews against the source code.

## Review Process

### 1. Initial Setup

```typescript
// Use semantic_search for broad overview
semantic_search("look for major features, components, and architecture");

// Use grep_search for specific patterns
grep_search("interface|type|class", true); // Find type definitions
grep_search("export (function|const|class)", true); // Find exported items
```

### 2. Component Architecture Review

- Verify each documented component against implementation:
  - Check prop types match documentation
  - Validate component hierarchy
  - Cross-reference hooks and utilities
  - Compare error handling patterns

### 3. Feature Module Analysis

For each feature (auth, accounts, expenses, etc.):

1. Compare documented structure with `/features/{name}` implementation
2. Verify API integration matches documentation
3. Check state management implementation
4. Cross-reference error handling patterns
5. Validate form validation rules

### 4. Data Model Verification

- Compare documented types with actual implementations
- Verify Zod schemas match documentation
- Check database models against documented structure
- Validate API request/response types

### 5. Missing Documentation Check

Look for implemented but undocumented features:

- New components without documentation
- Utility functions missing from docs
- Hooks without usage documentation
- Error handling patterns
- Form validation rules
- API endpoints

## Review Tools

### Code Search

```typescript
// Find all exported types
grep_search("export (type|interface)", true);

// Locate API endpoints
grep_search("MapGet|MapPost|MapPut|MapDelete", true);

// Find React components
grep_search("export (function|const).*React\\.", true);

// Search for validation schemas
grep_search("z\\.object|createFormSchema", true);
```

### Component Analysis

```typescript
// Search for specific component implementations
semantic_search("Look for implementations of [component]");

// Find component usage
list_code_usages("[component name]");
```

### File Structure Verification

```typescript
// Get current structure
list_dir("/features");
list_dir("/components");
list_dir("/api");

// Compare with documented structure
semantic_search("project structure documentation");
```

## Common Review Areas

### 1. Authentication

- Token management implementation
- Login/logout flow
- Authorization patterns
- Protected routes
- Error handling

### 2. Data Management

- State management patterns
- API integration
- Local storage usage
- Form handling
- Validation rules

### 3. UI Components

- Component hierarchy
- Prop interfaces
- Event handling
- Error states
- Loading states

### 4. Error Handling

- Error boundary usage
- API error handling
- Form validation errors
- User feedback patterns
- Error recovery flows

### 5. Type Safety

- TypeScript interfaces/types
- Zod schemas
- API types
- Form validation types
- State management types

## Priority Review Areas

1. Security-critical features:

   - Authentication implementation
   - Authorization rules
   - Data encryption
   - Token management

2. Data integrity features:

   - Form validation
   - API validation
   - Error handling
   - State management

3. Core user features:

   - Account management
   - Expense tracking
   - Income management
   - Financial projections

4. Infrastructure:
   - Project structure
   - Build configuration
   - Environment setup
   - Docker implementation

## Documentation Updates

When making documentation updates:

1. Update relevant sections
2. Cross-reference all changes
3. Update table of contents
4. Verify code snippets
5. Check for broken links
