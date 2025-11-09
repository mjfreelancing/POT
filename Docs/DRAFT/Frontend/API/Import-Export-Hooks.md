# Import/Export API Hooks Documentation

## Overview

This document explains the design decisions for the import/export API hooks and why they cannot use the generic `useGet()` and `usePost()` hooks.

## Why Custom Hooks Instead of Generic `useGet()`/`usePost()`?

### Export Hook (`useApiExport`)

**Cannot use generic `useGet()` because:**

1. **Response Type Requirements**

   ```typescript
   // Needs blob response type for file downloads
   responseType: 'blob';
   ```

2. **Return Structure**

   ```typescript
   // Generic useGet only returns response.data
   return new SuccessResult(response.data);

   // Export needs BOTH blob AND headers
   return new SuccessResult({
     blob: response.data,
     headers: response.headers,
   });
   ```

3. **Hook Pattern**

   - Uses `useMutation` (user-triggered action)
   - Generic `useGet` uses `useQuery` (automatic data fetching)

4. **Generic Limitation**
   - `performOperation()` function only returns `response.data`
   - Export requires access to response headers for filename extraction

### Import Hook (`useApiImport`)

**Cannot use generic `usePost()` because:**

1. **Payload Format**

   ```typescript
   // Requires FormData instead of JSON
   const formData = new FormData();
   formData.append('file', file);
   ```

2. **Content-Type Header**

   ```typescript
   // Needs multipart/form-data
   headers: {
     'Content-Type': 'multipart/form-data',
   }
   ```

3. **Parameter Pattern**
   ```typescript
   // Takes File parameter instead of generic TData
   mutationFn: async (file: File) => { ... }
   ```

## React Query Pattern: `useQuery` vs `useMutation`

### Why Export Uses `useMutation`

Export is a **user action**, not data fetching:

```typescript
// Correct: Manual action triggered by user
const { exportData } = useExport();
<Button onClick={exportData}>Export Data</Button>
```

**Characteristics of export:**

- User-initiated (button click)
- Side effect (downloads file)
- Not cached (each export is fresh)
- Action-oriented ("Generate and download file")

**If we used `useQuery` (WRONG):**

- Would auto-execute on component mount
- Would cache blob results unnecessarily
- Would refetch in background
- Doesn't match user intention

### Why Import Uses `useMutation`

Similar reasoning - import is a user action:

- User selects file and clicks import
- Processes/uploads file
- Shows progress and results
- One-time operation per file

## CORS Configuration Note

For the Content-Disposition header to be accessible to JavaScript:

```csharp
// ASP.NET Core - expose the header to client
app.UseCors(policy => policy
    .WithOrigins("http://localhost:5175")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders("content-disposition")); // ← Required for filename extraction
```
