import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { UnexpectedError } from '@/api/errors/apiErrors';
import { useApiGetProjection } from '@/api/hooks/useProjections';
import ProjectionsPage from '@/features/projections/ProjectionsPage';
import useProjectionStorage from '@/features/projections/hooks/useProjectionStorage';
import { FailResult, SuccessResult } from '@/lib';

import type { ProjectionMetric } from '@/data/projection';

import { createProjection } from '../../shared/factories/projectionFactory';

vi.mock('@/api/hooks/useProjections', () => ({
  useApiGetProjection: vi.fn(),
}));

vi.mock('@/features/projections/hooks/useProjectionStorage', () => ({
  default: vi.fn(),
  projectionStorageDefaults: {
    metric: 'balance',
    period: 6,
    hiddenSeries: [],
  },
}));

vi.mock('@/features/projections/components', () => ({
  ProjectionsHeader: () => <h1>Projections</h1>,
  ProjectionChart: ({
    period,
    selectedMetric,
    hiddenSeries,
    onPeriodChange,
    onMetricChange,
    onHiddenSeriesChange,
    onStartDateChange,
  }: {
    period: number;
    selectedMetric: ProjectionMetric;
    hiddenSeries: string[];
    onPeriodChange: (period: number) => void;
    onMetricChange: (metric: ProjectionMetric) => void;
    onHiddenSeriesChange: (hiddenSeries: string[]) => void;
    onStartDateChange: (date: Date | undefined) => void;
  }) => (
    <div>
      <div>{`chart period:${period}`}</div>
      <div>{`chart metric:${selectedMetric}`}</div>
      <div>{`chart hidden:${hiddenSeries.join('|')}`}</div>
      <button type="button" onClick={() => onPeriodChange(9)}>
        Set period
      </button>
      <button type="button" onClick={() => onMetricChange('available')}>
        Set metric
      </button>
      <button type="button" onClick={() => onHiddenSeriesChange(['account-1'])}>
        Set hidden series
      </button>
      <button type="button" onClick={() => onStartDateChange(new Date())}>
        Set start date to today
      </button>
    </div>
  ),
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
  },
}));

describe('ProjectionsPage', () => {
  const getProjectionStorageDataMock = vi.fn();
  const setProjectionStorageDataMock = vi.fn();
  const removeStorageStartDateMock = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    getProjectionStorageDataMock.mockReturnValue({
      period: 6,
      metric: 'balance',
      hiddenSeries: [],
    });

    vi.mocked(useProjectionStorage).mockReturnValue({
      getProjectionStorageData: getProjectionStorageDataMock,
      setProjectionStorageData: setProjectionStorageDataMock,
      removeStorageStartDate: removeStorageStartDateMock,
    });

    vi.mocked(useApiGetProjection).mockReturnValue({
      data: new SuccessResult(createProjection()),
      isLoading: false,
      isFetching: false,
    } as unknown as ReturnType<typeof useApiGetProjection>);
  });

  test('renders projections header and chart when projection data succeeds', () => {
    render(<ProjectionsPage />);

    expect(screen.getByText('Projections')).toBeInTheDocument();
    expect(screen.getByText('chart period:6')).toBeInTheDocument();
    expect(screen.getByText('chart metric:balance')).toBeInTheDocument();
    expect(screen.getByText('chart hidden:')).toBeInTheDocument();
  });

  test('persists period, metric, and hidden series changes from chart interactions', async () => {
    render(<ProjectionsPage />);

    await userEvent.click(screen.getByRole('button', { name: 'Set period' }));
    await userEvent.click(screen.getByRole('button', { name: 'Set metric' }));

    await userEvent.click(
      screen.getByRole('button', { name: 'Set hidden series' }),
    );

    expect(setProjectionStorageDataMock).toHaveBeenCalledWith({
      period: 9,
      metric: 'balance',
      hiddenSeries: [],
    });

    expect(setProjectionStorageDataMock).toHaveBeenCalledWith({
      period: 6,
      metric: 'available',
      hiddenSeries: [],
    });

    expect(setProjectionStorageDataMock).toHaveBeenCalledWith({
      period: 6,
      metric: 'balance',
      hiddenSeries: ['account-1'],
    });
  });

  test('removes stored start date when start date is changed to today', async () => {
    render(<ProjectionsPage />);

    await userEvent.click(
      screen.getByRole('button', { name: 'Set start date to today' }),
    );

    expect(removeStorageStartDateMock).toHaveBeenCalledTimes(1);
  });

  test('cleans up stale stored start date during initialization', () => {
    getProjectionStorageDataMock.mockReturnValue({
      startDate: '2000-01-01',
      period: 6,
      metric: 'balance',
      hiddenSeries: [],
    });

    render(<ProjectionsPage />);

    expect(removeStorageStartDateMock).toHaveBeenCalledTimes(1);
  });

  test('shows loading overlay while projection data is loading', () => {
    vi.mocked(useApiGetProjection).mockReturnValue({
      data: undefined,
      isLoading: true,
      isFetching: false,
    } as unknown as ReturnType<typeof useApiGetProjection>);

    render(<ProjectionsPage />);

    expect(screen.getByText('Loading...')).toBeInTheDocument();
  });

  test('renders and dismisses api error sheet', async () => {
    vi.mocked(useApiGetProjection).mockReturnValue({
      data: new FailResult(new UnexpectedError('Projection API failed')),
      isLoading: false,
      isFetching: false,
    } as unknown as ReturnType<typeof useApiGetProjection>);

    render(<ProjectionsPage />);

    expect(screen.getByText('Unexpected Error')).toBeInTheDocument();
    expect(screen.getByText('Projection API failed')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Dismiss' }));

    await waitFor(() => {
      expect(screen.queryByText('Unexpected Error')).not.toBeInTheDocument();
    });
  });
});
