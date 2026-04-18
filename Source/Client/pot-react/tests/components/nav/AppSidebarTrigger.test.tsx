import { render, screen } from '@testing-library/react';
import { describe, expect, test, vi } from 'vitest';

import AppSidebarTrigger from '@/components/nav/AppSidebarTrigger';

vi.mock('@/components/ui/sidebar', () => ({
  SidebarTrigger: ({ className }: { className?: string }) => (
    <button type="button" data-testid="sidebar-trigger" className={className}>
      Toggle
    </button>
  ),
}));

describe('AppSidebarTrigger', () => {
  test('renders trigger with expected composition classes', () => {
    render(<AppSidebarTrigger />);

    expect(screen.getByTestId('sidebar-trigger')).toHaveClass(
      'mr-3',
      '!size-10',
      '[&>svg]:!size-6',
      '-ml-2',
      'self-start',
    );
  });
});
