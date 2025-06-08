import { lazy, Suspense } from 'react';
import { Navigate, Route, Routes } from 'react-router';

import LoadingMessage from '../components/feedback/message/LoadingMessage';

// Lazy load page components to enable code splitting and reduce the initial bundle size
const AccountsPage = lazy(() => import('../features/accounts/AccountsPage'));
const DashboardPage = lazy(() => import('../features/dashboard/DashboardPage'));
const IncomesPage = lazy(() => import('../features/incomes/IncomesPage'));
const ExpensesPage = lazy(() => import('../features/expenses/ExpensesPage'));
const ProjectionsPage = lazy(
  () => import('../features/projections/ProjectionsPage'),
);
const CreateAccountSheet = lazy(
  () => import('../features/accounts/create/CreateAccountSheet'),
);
const EditAccountSheet = lazy(
  () => import('../features/accounts/edit/EditAccountSheet'),
);
const CreateIncomeSheet = lazy(
  () => import('../features/incomes/create/CreateIncomeSheet'),
);
const EditIncomeSheet = lazy(
  () => import('../features/incomes/edit/EditIncomeSheet'),
);
const CreateExpenseSheet = lazy(
  () => import('../features/expenses/create/CreateExpenseSheet'),
);
const EditExpenseSheet = lazy(
  () => import('../features/expenses/edit/EditExpenseSheet'),
);

// Suspense provides a loading fallback while the lazy-loaded components are being downloaded
export const AppRoutes = () => {
  return (
    <Suspense fallback={<LoadingMessage />}>
      <Routes>
        <Route path="/" element={<Navigate replace to="dashboard" />} />
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/projections" element={<ProjectionsPage />} />
        <Route path="/accounts" element={<AccountsPage />}>
          <Route path="create" element={<CreateAccountSheet />} />
          <Route path="edit/:id" element={<EditAccountSheet />} />
        </Route>
        <Route path="/incomes" element={<IncomesPage />}>
          <Route path="create" element={<CreateIncomeSheet />} />
          <Route path="edit/:id" element={<EditIncomeSheet />} />
        </Route>
        <Route path="/expenses" element={<ExpensesPage />}>
          <Route path="create" element={<CreateExpenseSheet />} />
          <Route path="edit/:id" element={<EditExpenseSheet />} />
        </Route>
      </Routes>
    </Suspense>
  );
};
