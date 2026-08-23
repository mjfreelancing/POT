---
name: mmd2png
description: Convert Mermaid diagram (.mmd) files to PNG images using mmdc CLI
pattern: "**/*.mmd"
---

# Convert Mermaid Diagrams to PNG

Convert the active Mermaid diagram file (.mmd) to PNG format using the Mermaid CLI (`mmdc`).

## Workflow

1. **Check active file**: If the currently open file has a `.mmd` extension, convert that file.

2. **Search for .mmd files**: If no active .mmd file is open, search the workspace for all `.mmd` files.

3. **Handle multiple files**:
   - If only one .mmd file is found, convert it automatically.
   - If multiple .mmd files are found, ask the user which files to convert (allow multi-select).

4. **Run conversion**: For each selected file, execute the command with the following priority:
   
   **On Windows (preferred order):**
   - Try `mmdc.cmd` first (avoids PowerShell execution policy issues):
     ```powershell
     mmdc.cmd -i "path/to/file.mmd" -o "path/to/file.png" -s 3
     ```
   - If that fails, try `npx.cmd`:
     ```powershell
     npx.cmd -y @mermaid-js/mermaid-cli mmdc -i "path/to/file.mmd" -o "path/to/file.png" -s 3
     ```
   - As last resort, try PowerShell version:
     ```powershell
     mmdc -i "path/to/file.mmd" -o "path/to/file.png" -s 3
     ```
   
   **On other platforms:**
   - Use `npx`:
     ```bash
     npx -y @mermaid-js/mermaid-cli mmdc -i "path/to/file.mmd" -o "path/to/file.png" -s 3
     ```
   
   The `-s 3` parameter sets the scale factor to 3 for higher resolution output.

5. **Report results**: After conversion, confirm which files were successfully converted and the output file paths.

## Prerequisites

Ensure the Mermaid CLI (`mmdc`) is installed. If not found, inform the user to install it via:
```powershell
npm install -g @mermaid-js/mermaid-cli
```

## Error Handling

- If `mmdc` is not installed, provide installation instructions.
- If conversion fails, report the specific error for each file.
- Ensure output files are created in the same directory as the source .mmd files.
