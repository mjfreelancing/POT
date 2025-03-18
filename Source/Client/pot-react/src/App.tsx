import { Separator } from "@radix-ui/react-separator";
import { AppSidebar } from "./components/AppSidebar";
import { ThemeProvider } from "./components/theme/ThemeProvider";
import { Route, Routes } from "react-router-dom";
import {
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from "./components/ui/sidebar";
import DashboardPage from "./features/dashboard/DashboardPage";
import ProjectionsPage from "./features/projections/ProjectionsPage";

const App = () => {
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
                <Route path="/" element={<DashboardPage />} />
                <Route path="/dashboard" element={<DashboardPage />} />
                <Route path="/projections" element={<ProjectionsPage />} />
              </Routes>
            </div>
          </SidebarInset>
        </SidebarProvider>
      </div>
    </ThemeProvider>
  );
};

export default App;
