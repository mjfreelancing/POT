import { render } from '@testing-library/react';
import { Line, LineChart } from 'recharts';
import { describe, expect, test } from 'vitest';

import { type ChartConfig, ChartContainer } from '@/components/ui/chart';

const config: ChartConfig = {
  balance: { label: 'Balance', color: '#2563eb' },
};

describe('ChartContainer', () => {
  test('renders the chart shell with a stable chart id and config-driven CSS variables', () => {
    const { container } = render(
      <ChartContainer config={config} className="h-64">
        <LineChart data={[{ x: 1, y: 2 }]}>
          <Line dataKey="y" />
        </LineChart>
      </ChartContainer>,
    );

    const shell = container.querySelector('[data-slot="chart"]');
    expect(shell).not.toBeNull();
    expect(shell?.getAttribute('data-chart')).toMatch(/^chart-/);

    // ChartStyle injects the theme CSS variable for each colored series.
    const styleTag = container.querySelector('style');
    expect(styleTag?.textContent).toContain('--color-balance: #2563eb');
  });

  test('renders no style element when the config has no colors or themes', () => {
    const { container } = render(
      <ChartContainer config={{}}>
        <LineChart data={[]}>
          <Line dataKey="y" />
        </LineChart>
      </ChartContainer>,
    );

    expect(container.querySelector('style')).toBeNull();
  });
});
