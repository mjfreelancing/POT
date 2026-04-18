import { render, screen } from '@testing-library/react';
import { describe, expect, test, vi } from 'vitest';

import LoadingOverlay from '@/components/feedback/spinner/LoadingOverlay';

vi.mock('@/components/feedback/spinner/LoadingSpinner', () => ({
  default: () => <div data-testid="loading-spinner" />,
}));

describe('LoadingOverlay', () => {
  test('renders default loading message', () => {
    render(<LoadingOverlay />);

    expect(screen.getByTestId('loading-spinner')).toBeInTheDocument();
    expect(screen.getByText('Loading...')).toBeInTheDocument();
  });

  test('renders custom loading message', () => {
    render(<LoadingOverlay message="Preparing data" />);

    expect(screen.getByText('Preparing data')).toBeInTheDocument();
  });
});
