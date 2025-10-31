import { lazy, Suspense } from 'react';
import { Navigate, Outlet, Route, Routes } from 'react-router';

import useAuthContext from '@/features/auth/AuthContext';
import LoginPage from '@/features/auth/LoginPage';
import logoutManager from '@/features/auth/logoutManager';

import LoadingMessage from '../components/feedback/message/LoadingMessage';

// Lazy load page components to enable code splitting and reduce the initial bundle size
const AccountsPage = lazy(() => import('../features/accounts/AccountsPage'));
const DashboardPage = lazy(() => import('../features/dashboard/DashboardPage'));
const IncomesPage = lazy(() => import('../features/incomes/IncomesPage'));
const ExpensesPage = lazy(() => import('../features/expenses/ExpensesPage'));
const ProjectionsPage = lazy(
  () => import('../features/projections/ProjectionsPage'),
);
const UsersPage = lazy(() => import('../features/users/UsersPage'));
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
const InviteUserSheet = lazy(
  () => import('../features/users/components/InviteUserSheet'),
);

// ProtectedRoute component for protecting routes
function ProtectedRoute() {
  const { isAuthenticated } = useAuthContext();
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  return <Outlet />;
}

// LogoutRoute component for handling forced logout
function LogoutRoute() {
  // Use the global logout manager to trigger logout - same as other parts of the app
  logoutManager.logout();

  // Always redirect to login after logout attempt
  return <Navigate to="/login" replace />;
}

function AppRoutes() {
  return (
    <Suspense fallback={<LoadingMessage />}>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/logout" element={<LogoutRoute />} />
        <Route element={<ProtectedRoute />}>
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
          <Route path="/users" element={<UsersPage />}>
            <Route path="invite" element={<InviteUserSheet />} />
          </Route>
        </Route>
      </Routes>
    </Suspense>
  );
}

export { AppRoutes };
