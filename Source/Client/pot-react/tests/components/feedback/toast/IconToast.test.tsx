import { render, screen } from '@testing-library/react';
import { CircleCheck } from 'lucide-react';
import { describe, expect, test } from 'vitest';

import { IconToast } from '@/components/feedback/toast/IconToast';

describe('IconToast', () => {
  test('renders icon title and description', () => {
    render(
      <IconToast
        icon={CircleCheck}
        iconColor="text-green-600"
        title="Saved"
        description="Changes were saved"
      />,
    );

    expect(screen.getByText('Saved')).toBeInTheDocument();
    expect(screen.getByText('Changes were saved')).toBeInTheDocument();
  });

  test('renders details when provided', () => {
    render(
      <IconToast
        icon={CircleCheck}
        iconColor="text-green-600"
        title="Saved"
        description="Changes were saved"
        details="Sync completed"
      />,
    );

    expect(screen.getByText('Sync completed')).toBeInTheDocument();
  });
});
