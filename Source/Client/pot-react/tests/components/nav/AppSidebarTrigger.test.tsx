import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, test, vi } from 'vitest';

import AppSidebarTrigger from '@/components/nav/AppSidebarTrigger';
import { SidebarProvider } from '@/components/ui/sidebar';

vi.mock('@/hooks/use-mobile', () => ({
  useIsMobile: () => false,
}));

describe('AppSidebarTrigger', () => {
  test('renders trigger with expected composition classes', () => {
    render(
      <SidebarProvider>
        <AppSidebarTrigger />
      </SidebarProvider>,
    );

    const trigger = screen.getByRole('button', { name: 'Toggle Sidebar' });

    expect(trigger).toHaveClass(
      'mr-3',
      '!size-10',
      '[&>svg]:!size-6',
      '-ml-2',
      'self-start',
    );

    expect(trigger.querySelector('svg')).not.toBeNull();

    fireEvent.click(trigger);
  });
});
