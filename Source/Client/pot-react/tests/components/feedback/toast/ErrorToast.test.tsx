import { AlertTriangle } from 'lucide-react';
import { render } from '@testing-library/react';
import { describe, expect, test, vi } from 'vitest';

import { ErrorToast } from '@/components/feedback/toast/ErrorToast';
import { IconToast } from '@/components/feedback/toast/IconToast';

vi.mock('@/components/feedback/toast/IconToast', () => ({
  IconToast: vi.fn(() => null),
}));

describe('ErrorToast', () => {
  test('passes error visual props to IconToast', () => {
    render(<ErrorToast title="Failed" description="Unable to save" />);

    expect(IconToast).toHaveBeenCalledWith(
      {
        icon: AlertTriangle,
        iconColor: 'text-red-600',
        title: 'Failed',
        description: 'Unable to save',
      },
      undefined,
    );
  });
});
