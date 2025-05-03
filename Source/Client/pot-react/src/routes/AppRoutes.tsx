import { Navigate, Route, Routes } from 'react-router';

import { AccountsPage, DashboardPage, ProjectionsPage } from '../features';

export const AppRoutes = () => {
  return (
    <Routes>
      <Route path="/" element={<Navigate replace to="dashboard" />} />
      <Route path="/dashboard" element={<DashboardPage />} />
      <Route path="/projections" element={<ProjectionsPage />} />
      <Route path="/accounts" element={<AccountsPage />} />
    </Routes>
  );
};
