@echo off
echo Generating SVGs from Mermaid diagrams...

echo Processing server-only-diagram.mmd
call mmdc -i server-only-diagram.mmd -o server-only-diagram.svg

echo Processing client-server-diagram.mmd
call mmdc -i client-server-diagram.mmd -o client-server-diagram.svg

echo Done!
