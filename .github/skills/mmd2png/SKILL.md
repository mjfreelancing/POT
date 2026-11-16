---
name: mmd2png
description: Convert Mermaid diagram (.mmd) files to PNG images using the mmdc CLI.
argument-hint: "[path to .mmd file]"
---

# Convert Mermaid Diagrams to PNG

1. Pick the files: the path in the user input, else the `.mmd` file open in the editor, else search the workspace for `.mmd` files. If several are found, ask which to convert.
2. For each file, write the PNG beside the source with scale 3 (`-s 3`). On Windows try, in order:
   1. `mmdc.cmd -i "path/file.mmd" -o "path/file.png" -s 3` (avoids PowerShell execution-policy issues)
   2. `npx.cmd -y @mermaid-js/mermaid-cli mmdc -i "path/file.mmd" -o "path/file.png" -s 3`
   3. `mmdc -i "path/file.mmd" -o "path/file.png" -s 3`

   On other platforms: `npx -y @mermaid-js/mermaid-cli mmdc -i "path/file.mmd" -o "path/file.png" -s 3`.

   A wide or dense diagram loses text and overlaps nodes at that size. Render it larger with
   `--size 3840 -s 2` instead of `-s 3`: from mermaid-cli 12 there is no `-w`/`-H` pair, and `--size` caps the
   larger of the two dimensions. The deployment diagram at
   `Docs/reference/diagrams/pot_azure_diagram.mmd` is rendered that way, at 4830x7680.

   If the browser will not start — `Failed to launch the browser process`, exit code `3221225477`
   (`0xC0000005`) on Windows — point Puppeteer at an installed Chrome instead of the one it downloaded. Write
   a JSON file and pass it with `-p`:

   ```json
   {
     "executablePath": "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
     "args": ["--no-sandbox", "--disable-gpu"]
   }
   ```

3. Report which files converted and the output paths. If `mmdc` is missing, tell the user to run `npm install -g @mermaid-js/mermaid-cli`. If a conversion fails, report the specific error for that file.

