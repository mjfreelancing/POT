import { render } from '@testing-library/react';
import { describe, expect, test } from 'vitest';

import FilterResultsCount from '@/components/filters/FilterResultsCount';

describe('FilterResultsCount', () => {
  test('shows default summary text when unfiltered', () => {
    const { container } = render(
      <FilterResultsCount filteredCount={10} totalCount={10} />,
    );

    const badge = container.querySelector('[data-slot="badge"]');

    expect(badge).toHaveTextContent('Showing 10 of 10 items');
  });

  test('uses filtered style when counts differ', () => {
    const { container } = render(
      <FilterResultsCount filteredCount={4} totalCount={10} />,
    );

    const badge = container.querySelector('[data-slot="badge"]');

    expect(badge).toHaveClass(
      'bg-blue-500/20',
      'text-blue-700',
      'border-blue-500/30',
    );
  });

  test('uses isFilterActive override when counts are equal', () => {
    const { container } = render(
      <FilterResultsCount
        filteredCount={10}
        totalCount={10}
        isFilterActive={true}
      />,
    );

    const badge = container.querySelector('[data-slot="badge"]');

    expect(badge).toHaveClass(
      'bg-blue-500/20',
      'text-blue-700',
      'border-blue-500/30',
    );
  });
});
