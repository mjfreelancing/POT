import { lazy, Suspense } from 'react';
import { Navigate, Route, Routes } from 'react-router';
import LoadingMessage from '../components/feedback/message/LoadingMessage';

// Lazy load page components to enable code splitting and reduce the initial bundle size
const AccountsPage = lazy(() => import('../features/accounts/AccountsPage'));
const DashboardPage = lazy(() => import('../features/dashboard/DashboardPage'));
const ProjectionsPage = lazy(
  () => import('../features/projections/ProjectionsPage'),
);

// Suspense provides a loading fallback while the lazy-loaded components are being downloaded
export const AppRoutes = () => {
  return (
    <Suspense fallback={<LoadingMessage />}>
      <Routes>
        <Route path="/" element={<Navigate replace to="dashboard" />} />
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/projections" element={<ProjectionsPage />} />
        <Route path="/accounts" element={<AccountsPage />} />
      </Routes>
    </Suspense>
  );
};
