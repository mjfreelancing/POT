import { render, screen } from '@testing-library/react';
import { describe, expect, test } from 'vitest';

import { Toolbar } from '@/components/layout';

describe('Toolbar', () => {
  test('renders toolbar role and children', () => {
    render(
      <Toolbar>
        <button type="button">Filters</button>
        <button type="button">Actions</button>
      </Toolbar>,
    );

    const toolbar = screen.getByRole('toolbar');

    expect(toolbar).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Filters' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Actions' })).toBeInTheDocument();
  });

  test('applies default and custom class names', () => {
    render(
      <Toolbar className="custom-toolbar-class">
        <span>Content</span>
      </Toolbar>,
    );

    const toolbar = screen.getByRole('toolbar');

    expect(toolbar).toHaveClass(
      'flex',
      'flex-wrap',
      'items-center',
      'justify-between',
      'gap-3',
      'mb-2',
      'bg-muted/60',
      'rounded-md',
      'px-4',
      'py-3',
      'border',
      'custom-toolbar-class',
    );
  });
});
