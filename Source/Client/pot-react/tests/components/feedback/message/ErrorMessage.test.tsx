import { render, screen } from '@testing-library/react';
import { describe, expect, test } from 'vitest';

import ErrorMessage from '@/components/feedback/message/ErrorMessage';

describe('ErrorMessage', () => {
  test('renders the provided error message', () => {
    render(<ErrorMessage message="Something went wrong" />);

    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
  });
});
