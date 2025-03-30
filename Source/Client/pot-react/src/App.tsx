import { Separator } from '@radix-ui/react-separator';
import { Navigate, Route, Routes } from 'react-router';

import { ThemeProvider } from './components/theme/ThemeProvider';
import {
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from './components/ui/sidebar';
import { AppSidebar } from './components/ui-custom/AppSidebar';
import AccountsPage from './features/accounts/AccountsPage';
import DashboardPage from './features/dashboard/DashboardPage';
import ProjectionsPage from './features/projections/ProjectionsPage';

const App = () => {
  console.info(`Running mode: ${import.meta.env.MODE}`);

  return (
    <ThemeProvider defaultTheme="system" storageKey="pot-ui-theme">
      <div className="flex h-screen w-screen">
        <SidebarProvider>
          <AppSidebar />
          {/* Everything below is yet to be cleaned up */}
          <SidebarInset>
            <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-[[data-collapsible=icon]]/sidebar-wrapper:h-12">
              <div className="flex items-center gap-2 px-4">
                <SidebarTrigger className="-ml-1" />
                <Separator orientation="vertical" className="mr-2 h-4" />
                <span>Other stuff here</span>
              </div>
            </header>
            <div className="flex-1 h-full">
              <Routes>
                <Route path="/" element={<Navigate replace to="dashboard" />} />
                <Route path="/dashboard" element={<DashboardPage />} />
                <Route path="/projections" element={<ProjectionsPage />} />
                <Route path="/accounts" element={<AccountsPage />} />
              </Routes>
            </div>
          </SidebarInset>
        </SidebarProvider>
      </div>
    </ThemeProvider>
  );
};

export default App;
