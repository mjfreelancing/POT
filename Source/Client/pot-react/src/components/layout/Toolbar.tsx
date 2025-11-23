import type { ReactNode } from 'react';

/**
 * Toolbar - A reusable flex container for page-level controls (filters, search, actions).
 *
 * Children are left/right aligned using a flex row with justify-between.
 */
type ToolbarProps = {
  children: ReactNode;
  className?: string;
};

function Toolbar({ children, className = '' }: ToolbarProps) {
  return (
    <div
      className={`flex flex-wrap items-center justify-between gap-3 mb-2 bg-muted/60 rounded-md px-4 py-3 border ${className}`}
      role="toolbar"
    >
      {children}
    </div>
  );
}

export default Toolbar;
export type { ToolbarProps };
// ...existing code from Toolbar.tsx...
