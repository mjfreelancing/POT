import { render, screen } from '@testing-library/react';
import { describe, expect, test, vi } from 'vitest';

import LoadingMessage from '@/components/feedback/message/LoadingMessage';

vi.mock('@/components/feedback/spinner/LoadingSpinner', () => ({
  default: () => <div data-testid="loading-spinner" />,
}));

describe('LoadingMessage', () => {
  test('renders loading content by default', () => {
    render(<LoadingMessage />);

    expect(screen.getByTestId('loading-spinner')).toBeInTheDocument();
    expect(screen.getByText('Loading...')).toBeInTheDocument();
  });

  test('renders nothing when not loading', () => {
    const { container } = render(<LoadingMessage isLoading={false} />);

    expect(container.firstChild).toBeNull();
  });
});
