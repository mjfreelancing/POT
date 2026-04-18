import { render, screen } from '@testing-library/react';
import { describe, expect, test } from 'vitest';

import LoadingSpinner from '@/components/feedback/spinner/LoadingSpinner';

describe('LoadingSpinner', () => {
  test('renders accessible status spinner', () => {
    render(<LoadingSpinner />);

    expect(screen.getByRole('status', { name: 'Loading' })).toHaveAttribute(
      'aria-busy',
      'true',
    );
  });
});
