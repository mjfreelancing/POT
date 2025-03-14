import { AppSidebar } from "./components/app-sidebar";
import { ThemeProvider } from "./components/theme/theme-provider";
import { SidebarProvider, SidebarTrigger } from "./components/ui/sidebar";
import SummaryPage from "./features/summary/summary-page";

const App = () => {
  return (
    <ThemeProvider defaultTheme="system" storageKey="pot-ui-theme">
      <SidebarProvider>
        <AppSidebar />
        <SidebarTrigger />
        {/* Yet to add routing */}
        <SummaryPage />
      </SidebarProvider>
    </ThemeProvider>
  );
};

export default App;
