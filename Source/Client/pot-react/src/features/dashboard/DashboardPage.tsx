import {
  Activity,
  ArrowDownRight,
  ArrowUpRight,
  CreditCard,
  DollarSign,
  Landmark,
} from 'lucide-react';
import { useEffect, useState } from 'react';

import { useApiGetAllAccounts } from '@/api/hooks';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { DisplayError, formatMoneyValue } from '@/lib';

import { AccountsOverview, DashboardHeader, QuickActions } from './components';

type MetricCardProps = {
  title: string;
  value: string;
  change?: string;
  changeType?: 'positive' | 'negative' | 'neutral';
  icon: React.ReactNode;
  description?: string;
};

function MetricCard({
  title,
  value,
  change,
  changeType,
  icon,
  description,
}: MetricCardProps) {
  const getChangeColor = () => {
    if (changeType === 'positive') return 'text-success';
    if (changeType === 'negative') return 'text-destructive';
    return 'text-muted-foreground';
  };

  const getChangeIcon = () => {
    if (changeType === 'positive') return <ArrowUpRight className="h-3 w-3" />;
    if (changeType === 'negative')
      return <ArrowDownRight className="h-3 w-3" />;
    return null;
  };

  return (
    <Card className="card-elevated">
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-sm font-medium text-muted-foreground">
          {title}
        </CardTitle>
        <div className="text-primary">{icon}</div>
      </CardHeader>
      <CardContent>
        <div className="metric-value mb-1">{value}</div>
        {change && (
          <div
            className={`flex items-center gap-1 text-xs ${getChangeColor()}`}
          >
            {getChangeIcon()}
            <span>{change}</span>
          </div>
        )}
        {description && (
          <p className="text-xs text-muted-foreground mt-1">{description}</p>
        )}
      </CardContent>
    </Card>
  );
}

function DashboardPage() {
  const [error, setError] = useState<DisplayError | null>(null);
  const { data: result, isLoading } = useApiGetAllAccounts();

  useEffect(() => {
    // Transient error handling
    if (result) {
      if (result.success) {
        setError(null);
      } else {
        setError({
          title: result.error.code,
          description: result.error.description,
        });
      }
    }
  }, [result]);

  const accounts = result?.success ? result.value : [];

  // Calculate dashboard metrics
  const totalBalance = accounts.reduce((sum, acc) => sum + acc.balance, 0);
  const totalAvailable = accounts.reduce((sum, acc) => sum + acc.available, 0);
  const activeAccounts = accounts.length;
  const lowBalanceAccounts = accounts.filter(
    acc => acc.available < acc.balance * 0.1,
  ).length;

  return (
    <div className="flex flex-col flex-1 min-h-screen bg-gradient-to-br from-background to-muted/20">
      <DashboardHeader />

      <LoadingMessage isLoading={isLoading} />

      <div className="flex-1 p-6 space-y-6">
        {/* Key Metrics */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <MetricCard
            title="Total Balance"
            value={formatMoneyValue(totalBalance)}
            change="+2.5% from last month"
            changeType="positive"
            icon={<DollarSign className="h-4 w-4" />}
            description="Across all accounts"
          />
          <MetricCard
            title="Available Funds"
            value={formatMoneyValue(totalAvailable)}
            change="+1.2% from last week"
            changeType="positive"
            icon={<CreditCard className="h-4 w-4" />}
            description="Ready to spend"
          />
          <MetricCard
            title="Active Accounts"
            value={activeAccounts.toString()}
            icon={<Landmark className="h-4 w-4" />}
            description="Accounts in use"
          />
          <MetricCard
            title="Alerts"
            value={lowBalanceAccounts.toString()}
            changeType={lowBalanceAccounts > 0 ? 'negative' : 'neutral'}
            icon={<Activity className="h-4 w-4" />}
            description="Low balance warnings"
          />
        </div>

        <QuickActions />
        <AccountsOverview accounts={accounts} />

        {error && (
          <ErrorSheet
            title={error.title}
            description={error.description}
            onDismiss={() => setError(null)}
          />
        )}
      </div>
    </div>
  );
}

export default DashboardPage;
