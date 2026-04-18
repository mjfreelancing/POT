import { render } from '@testing-library/react';
import { CircleCheck } from 'lucide-react';
import { describe, expect, test, vi } from 'vitest';

import { IconToast } from '@/components/feedback/toast/IconToast';
import { SuccessToast } from '@/components/feedback/toast/SuccessToast';

vi.mock('@/components/feedback/toast/IconToast', () => ({
  IconToast: vi.fn(() => null),
}));

describe('SuccessToast', () => {
  test('passes success visual props to IconToast', () => {
    render(
      <SuccessToast
        icon={CircleCheck}
        title="Saved"
        description="Changes were saved"
        details="Sync completed"
      />,
    );

    expect(IconToast).toHaveBeenCalledWith(
      {
        icon: CircleCheck,
        iconColor: 'text-green-600',
        title: 'Saved',
        description: 'Changes were saved',
        details: 'Sync completed',
      },
      undefined,
    );
  });
});
